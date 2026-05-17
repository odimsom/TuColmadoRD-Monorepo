use crate::models::{LowStockAlert, TopProduct};
use sqlx::{postgres::PgPoolOptions, Pool, Postgres};
use uuid::Uuid;

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

#[derive(sqlx::FromRow)]
pub struct SalesRow {
    pub total_revenue: f64,
    pub transaction_count: i64,
}

#[derive(sqlx::FromRow)]
pub struct CustomerStatsRow {
    pub total: i64,
    pub with_debt: i64,
    pub total_debt: f64,
}

pub async fn fetch_sales_summary(
    pool: &DbPool,
    tenant_id: Uuid,
    from: &str,
    to: &str,
) -> anyhow::Result<SalesRow> {
    let row = sqlx::query_as::<_, SalesRow>(
        r#"
        SELECT
            COALESCE(SUM(s."TotalAmount"), 0)::FLOAT8 AS total_revenue,
            COUNT(*)::BIGINT                          AS transaction_count
        FROM "Sales"."Sales" s
        WHERE s."TenantId" = $1
          AND s."CreatedAt"::DATE BETWEEN $2::DATE AND $3::DATE
    "#,
    )
    .bind(tenant_id)
    .bind(from)
    .bind(to)
    .fetch_one(pool)
    .await?;
    Ok(row)
}

pub async fn fetch_top_products(
    pool: &DbPool,
    tenant_id: Uuid,
    from: &str,
    to: &str,
    limit: i64,
) -> anyhow::Result<Vec<TopProduct>> {
    let rows = sqlx::query_as::<_, TopProduct>(
        r#"
        SELECT
            p."Name"                                             AS product_name,
            SUM(sd."Quantity")::FLOAT8                          AS units_sold,
            SUM(sd."UnitPrice" * sd."Quantity")::FLOAT8         AS revenue
        FROM "Sales"."SaleDetails" sd
        JOIN "Sales"."Sales" s        ON s."Id" = sd."SaleId"
        JOIN "Inventory"."Products" p ON p."Id" = sd."ProductId"
        WHERE s."TenantId" = $1
          AND s."CreatedAt"::DATE BETWEEN $2::DATE AND $3::DATE
        GROUP BY p."Name"
        ORDER BY revenue DESC
        LIMIT $4
    "#,
    )
    .bind(tenant_id)
    .bind(from)
    .bind(to)
    .bind(limit)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn fetch_low_stock(
    pool: &DbPool,
    tenant_id: Uuid,
    threshold: f64,
) -> anyhow::Result<Vec<LowStockAlert>> {
    let rows = sqlx::query_as::<_, LowStockAlert>(
        r#"
        SELECT
            p."Id"                         AS product_id,
            p."Name"                       AS product_name,
            c."Name"                       AS category_name,
            p."StockQuantity"::FLOAT8      AS stock_quantity,
            p."SalePrice"::FLOAT8          AS sale_price
        FROM "Inventory"."Products" p
        LEFT JOIN "Inventory"."Categories" c ON c."Id" = p."CategoryId"
        WHERE p."TenantId" = $1
          AND p."IsActive"  = true
          AND p."StockQuantity" <= $2
        ORDER BY p."StockQuantity" ASC
    "#,
    )
    .bind(tenant_id)
    .bind(threshold)
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn fetch_customer_stats(
    pool: &DbPool,
    tenant_id: Uuid,
) -> anyhow::Result<CustomerStatsRow> {
    let row = sqlx::query_as::<_, CustomerStatsRow>(
        r#"
        SELECT
            COUNT(*)::BIGINT                                                    AS total,
            COUNT(*) FILTER (WHERE "Balance" > 0)::BIGINT                      AS with_debt,
            COALESCE(SUM("Balance") FILTER (WHERE "Balance" > 0), 0)::FLOAT8   AS total_debt
        FROM "Sales"."Customers"
        WHERE "TenantId" = $1
    "#,
    )
    .bind(tenant_id)
    .fetch_one(pool)
    .await?;
    Ok(row)
}

pub async fn ping(pool: &DbPool) -> anyhow::Result<()> {
    sqlx::query("SELECT 1").execute(pool).await?;
    Ok(())
}
