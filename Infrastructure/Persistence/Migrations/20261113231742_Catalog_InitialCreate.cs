using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce_Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Catalog_InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "category",
                schema: "catalog",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "store",
                schema: "catalog",
                columns: table => new
                {
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    pickup_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    delivery_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_store", x => x.store_id);
                    table.CheckConstraint("ck_store_latitude", "latitude >= -90 AND latitude <= 90");
                    table.CheckConstraint("ck_store_longitude", "longitude >= -180 AND longitude <= 180");
                });

            migrationBuilder.CreateTable(
                name: "product",
                schema: "catalog",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_price_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    base_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product", x => x.product_id);
                    table.CheckConstraint("ck_product_price_nonnegative", "base_price_amount >= 0");
                    table.ForeignKey(
                        name: "fk_product_category",
                        column: x => x.category_id,
                        principalSchema: "catalog",
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delivery_zone",
                schema: "catalog",
                columns: table => new
                {
                    delivery_zone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_delivery_zone", x => x.delivery_zone_id);
                    table.ForeignKey(
                        name: "fk_delivery_zone_store",
                        column: x => x.store_id,
                        principalSchema: "catalog",
                        principalTable: "store",
                        principalColumn: "store_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_delivery_zone_store_id",
                schema: "catalog",
                table: "delivery_zone",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_category_id",
                schema: "catalog",
                table: "product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_is_active",
                schema: "catalog",
                table: "product",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "uq_product_sku",
                schema: "catalog",
                table: "product",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_store_code",
                schema: "catalog",
                table: "store",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_zone",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "store",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "category",
                schema: "catalog");
        }
    }
}
