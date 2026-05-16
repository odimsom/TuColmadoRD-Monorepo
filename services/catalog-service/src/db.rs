use sqlx::{postgres::PgPoolOptions, Pool, Postgres};
use uuid::Uuid;
use crate::models::{Product, Category};

pub type DbPool = Pool<Postgres>;

pub async fn connect(url: &str) -> anyhow::Result<DbPool> {
    let pool = PgPoolOptions::new()
        .max_connections(10)
        .min_connections(2)
        .acquire_timeout(std::time::Duration::from_secs(5))
        .connect(url)
        .await?;
    tracing::info!("PostgreSQL connected");
    Ok(pool)
}

pub async fn fetch_catalog(pool: &DbPool, tenant_id: Uuid) -> anyhow::Result<Vec<Product>> {
    let rows = sqlx::query_as!(
        Product,
        r#"
        SELECT
            p."Id"             AS "id: Uuid",
            p."TenantId"       AS "tenant_id: Uuid",
            p."Name"           AS "name",
            p."CategoryId"     AS "category_id: Uuid",
            c."Name"           AS "category_name",
            CAST(p."SalePrice" AS FLOAT8)     AS "sale_price!",
            CAST(p."StockQuantity" AS FLOAT8) AS "stock_quantity!",
            p."IsActive"       AS "is_active"
        FROM "Inventory"."Products" p
        LEFT JOIN "Inventory"."Categories" c
               ON c."Id" = p."CategoryId"
        WHERE p."TenantId" = $1
          AND p."IsActive" = true
        ORDER BY c."Name", p."Name"
        "#,
        tenant_id
    )
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn fetch_categories(pool: &DbPool, tenant_id: Uuid) -> anyhow::Result<Vec<Category>> {
    let rows = sqlx::query_as!(
        Category,
        r#"
        SELECT
            "Id"        AS "id: Uuid",
            "TenantId"  AS "tenant_id: Uuid",
            "Name"      AS "name",
            "IsActive"  AS "is_active"
        FROM "Inventory"."Categories"
        WHERE "TenantId" = $1
          AND "IsActive" = true
        ORDER BY "Name"
        "#,
        tenant_id
    )
    .fetch_all(pool)
    .await?;
    Ok(rows)
}

pub async fn ping(pool: &DbPool) -> anyhow::Result<()> {
    sqlx::query("SELECT 1").execute(pool).await?;
    Ok(())
}
