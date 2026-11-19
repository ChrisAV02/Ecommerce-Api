using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Catalog_SynchronizationRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "synchronization_run",
                schema: "catalog",
                columns: table => new
                {
                    synchronization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    source_products = table.Column<int>(type: "integer", nullable: false),
                    source_stores = table.Column<int>(type: "integer", nullable: false),
                    source_assortments = table.Column<int>(type: "integer", nullable: false),
                    products_created = table.Column<int>(type: "integer", nullable: false),
                    products_updated = table.Column<int>(type: "integer", nullable: false),
                    categories_created = table.Column<int>(type: "integer", nullable: false),
                    stores_created = table.Column<int>(type: "integer", nullable: false),
                    assortments_created = table.Column<int>(type: "integer", nullable: false),
                    missing_price_skus_json = table.Column<string>(type: "jsonb", nullable: false),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_synchronization_run", x => x.synchronization_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_catalog_sync_started_at",
                schema: "catalog",
                table: "synchronization_run",
                column: "started_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "synchronization_run",
                schema: "catalog");
        }
    }
}
