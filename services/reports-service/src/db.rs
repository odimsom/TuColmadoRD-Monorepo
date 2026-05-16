use sqlx::{postgres::PgPoolOptions, Pool, Postgres};
use uuid::Uuid;
use crate::models::{LowStockAlert, TopProduct};

pub type DbPool = Pool<Postgres>;

pub async fn connect(url: &str) -> anyhow::Result<DbPool> {
    let pool = PgPoolOptions::new()
        .max_connections(8)
        .min_connections(1)
        .acquire_timeout(std::time::Duration::from_secs(5))
        .connect(url)
        .await?;
    tracing::info!("PostgreSQL connected");
    Ok(pool)
}

pub struct SalesRow {
    pub total_revenue:     f64,
    pub transaction_count: i64,
}

pub async fn fetch_sales_summary(
    pool:      &DbPool,
    tenant_id: Uuid,
    from:      &str,
    to:        &str,
) -> anyhow::Result<SalesRow> {
    let row = sqlx::query_as!(
        SalesRow,
        r#"
        SELECT
            COALESCE(SUM(s."TotalAmount"), 0)::FLOAT8 AS "total_revenue!",
            COUNT(*)::BIGINT                          AS "transaction_count!"
        FROM "Sales"."Sales" s
        WHERE s."TenantId" = $1
          AND s."CreatedAt"::DATE BETWEEN $2::DATE AND $3::DATE
        "#,
        tenant_id,
        from,
        to
    )
    .fetch_one(pool)
    .await?;
    Ok(row)
}

pub async fn fetch_top_products(
    pool:      &DbPool,
    tenant_id: Uuid,
    from:      &str,
    to:        &str,
    limit:     i64,
) -> anyhow::Result<Vec<TopProduct>> {
    let rows = sqlx::query_as!(
        TopProduct,
        r#"
        SELECT
            p."Name"               AS "product_name",
            SUM(sd."Quantity")::FLOAT8     AS "units_sold!",
            SUM(sd."UnitPrice" * sd."Quantity")::FLOAT8 AS "revenue!"
        FROM "Sales"."SaleDetails" sd
        JOIN "Sales"."Sales" s      ON s."Id" = sd."SaleId"
        JOIN "Inventory"."Products" p ON p."Id" = sd."ProductId"
        WHERE s."TenantId" = $1
          AND s."CreatedAt"::DATE BETWEEN $2::DATE AND $3::DATE
        GROUP BY p."Name"
        ORDER BY revenue DESC
        LIMIT $4
        "#,
        tenant_id,
        from,
        to,
        limit
    )
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn fetch_low_stock(
    pool:      &DbPool,
    tenant_id: Uuid,
    threshold: f64,
) -> anyhow::Result<Vec<LowStockAlert>> {
    let rows = sqlx::query_as!(
        LowStockAlert,
        r#"
        SELECT
            p."Id"            AS "product_id: Uuid",
            p."Name"          AS "product_name",
            c."Name"          AS "category_name",
            p."StockQuantity"::FLOAT8 AS "stock_quantity!",
            p."SalePrice"::FLOAT8     AS "sale_price!"
        FROM "Inventory"."Products" p
        LEFT JOIN "Inventory"."Categories" c ON c."Id" = p."CategoryId"
        WHERE p."TenantId" = $1
          AND p."IsActive"  = true
          AND p."StockQuantity" <= $2
        ORDER BY p."StockQuantity" ASC
        "#,
        tenant_id,
        threshold
    )
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn fetch_customer_stats(
    pool:      &DbPool,
    tenant_id: Uuid,
) -> anyhow::Result<(i64, i64, f64)> {
    // Returns (total, with_debt, total_debt)
    let row = sqlx::query!(
        r#"
        SELECT
            COUNT(*)::BIGINT                         AS "total!",
            COUNT(*) FILTER (WHERE "Balance" > 0)::BIGINT  AS "with_debt!",
            COALESCE(SUM("Balance") FILTER (WHERE "Balance" > 0), 0)::FLOAT8 AS "total_debt!"
        FROM "Sales"."Customers"
        WHERE "TenantId" = $1
        "#,
        tenant_id
    )
    .fetch_one(pool)
    .await?;
    Ok((row.total, row.with_debt, row.total_debt))
}

pub async fn ping(pool: &DbPool) -> anyhow::Result<()> {
    sqlx::query("SELECT 1").execute(pool).await?;
    Ok(())
}
