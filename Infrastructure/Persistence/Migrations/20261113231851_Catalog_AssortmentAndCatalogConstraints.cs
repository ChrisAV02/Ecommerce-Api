using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Catalog_AssortmentAndCatalogConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "longitude",
                schema: "catalog",
                table: "store",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "latitude",
                schema: "catalog",
                table: "store",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6);

            migrationBuilder.CreateTable(
                name: "store_assortment",
                schema: "catalog",
                columns: table => new
                {
                    store_assortment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_listed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_store_assortment", x => x.store_assortment_id);
                    table.ForeignKey(
                        name: "fk_store_assortment_product",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_store_assortment_store",
                        column: x => x.store_id,
                        principalSchema: "catalog",
                        principalTable: "store",
                        principalColumn: "store_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_store_code_format",
                schema: "catalog",
                table: "store",
                sql: "code ~ '^[A-Z0-9][A-Z0-9._-]{0,29}$'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_store_location_complete",
                schema: "catalog",
                table: "store",
                sql: "(latitude IS NULL) = (longitude IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_product_base_price_currency",
                schema: "catalog",
                table: "product",
                sql: "base_price_currency ~ '^[A-Z]{3}$'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_product_sku_format",
                schema: "catalog",
                table: "product",
                sql: "sku ~ '^[A-Z0-9][A-Z0-9._-]{0,49}$'");

            migrationBuilder.CreateIndex(
                name: "ix_store_assortment_product_id",
                schema: "catalog",
                table: "store_assortment",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "uq_store_assortment_store_id_product_id",
                schema: "catalog",
                table: "store_assortment",
                columns: new[] { "store_id", "product_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "store_assortment",
                schema: "catalog");

            migrationBuilder.DropCheckConstraint(
                name: "ck_store_code_format",
                schema: "catalog",
                table: "store");

            migrationBuilder.DropCheckConstraint(
                name: "ck_store_location_complete",
                schema: "catalog",
                table: "store");

            migrationBuilder.DropCheckConstraint(
                name: "ck_product_base_price_currency",
                schema: "catalog",
                table: "product");

            migrationBuilder.DropCheckConstraint(
                name: "ck_product_sku_format",
                schema: "catalog",
                table: "product");

            migrationBuilder.AlterColumn<decimal>(
                name: "longitude",
                schema: "catalog",
                table: "store",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "latitude",
                schema: "catalog",
                table: "store",
                type: "numeric(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,6)",
                oldPrecision: 9,
                oldScale: 6,
                oldNullable: true);
        }
    }
}
