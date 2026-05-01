using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuColmadoRD.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(TuColmadoRD.Infrastructure.Persistence.Contexts.TuColmadoDbContext))]
    [Migration("20260501000000_FixMissingTenantProfileAndNcf")]
    public partial class FixMissingTenantProfileAndNcf : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Ensure System schema exists
            migrationBuilder.EnsureSchema(name: "System");

            // 2. Add NcfNumber to Sales.Sales if not exists
            // We use raw SQL to check for existence since EF doesn't have a "CreateColumnIfNotExists"
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='Sales' AND table_name='Sales' AND column_name='NcfNumber') THEN
                        ALTER TABLE ""Sales"".""Sales"" ADD COLUMN ""NcfNumber"" character varying(20);
                    END IF;
                END $$;
            ");

            // 3. Create TenantProfiles table if not exists
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='System' AND table_name='TenantProfiles') THEN
                        CREATE TABLE ""System"".""TenantProfiles"" (
                            ""Id"" uuid NOT NULL,
                            ""TenantId"" uuid NOT NULL,
                            ""BusinessName"" character varying(200) NOT NULL,
                            ""Rnc"" character varying(15) NULL,
                            ""BusinessAddress"" character varying(500) NOT NULL,
                            ""Phone"" character varying(20) NULL,
                            ""Email"" character varying(150) NULL,
                            ""CreatedAt"" timestamp with time zone NOT NULL,
                            ""UpdatedAt"" timestamp with time zone NOT NULL,
                            CONSTRAINT ""PK_TenantProfiles"" PRIMARY KEY (""Id"")
                        );
                        
                        CREATE UNIQUE INDEX ""IX_TenantProfiles_TenantId"" ON ""System"".""TenantProfiles"" (""TenantId"");
                    END IF;
                END $$;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to revert for a fix migration
        }
    }
}
