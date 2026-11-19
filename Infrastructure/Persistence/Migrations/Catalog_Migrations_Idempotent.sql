DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'catalog') THEN
        CREATE SCHEMA catalog;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS catalog.__ef_migrations_history_catalog (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___ef_migrations_history_catalog" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'catalog') THEN
            CREATE SCHEMA catalog;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE TABLE catalog.category (
        category_id uuid NOT NULL,
        name character varying(200) NOT NULL,
        description character varying(1000),
        is_active boolean NOT NULL,
        created_at timestamptz NOT NULL,
        updated_at timestamptz,
        CONSTRAINT pk_category PRIMARY KEY (category_id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE TABLE catalog.store (
        store_id uuid NOT NULL,
        code character varying(30) NOT NULL,
        name character varying(200) NOT NULL,
        address character varying(500) NOT NULL,
        latitude numeric(9,6) NOT NULL,
        longitude numeric(9,6) NOT NULL,
        pickup_enabled boolean NOT NULL,
        delivery_enabled boolean NOT NULL,
        is_active boolean NOT NULL,
        created_at timestamptz NOT NULL,
        updated_at timestamptz,
        CONSTRAINT pk_store PRIMARY KEY (store_id),
        CONSTRAINT ck_store_latitude CHECK (latitude >= -90 AND latitude <= 90),
        CONSTRAINT ck_store_longitude CHECK (longitude >= -180 AND longitude <= 180)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE TABLE catalog.product (
        product_id uuid NOT NULL,
        sku character varying(50) NOT NULL,
        name character varying(200) NOT NULL,
        description character varying(1000),
        category_id uuid NOT NULL,
        base_price_amount numeric(12,2) NOT NULL,
        base_price_currency character varying(3) NOT NULL,
        is_active boolean NOT NULL,
        created_at timestamptz NOT NULL,
        updated_at timestamptz,
        CONSTRAINT pk_product PRIMARY KEY (product_id),
        CONSTRAINT ck_product_price_nonnegative CHECK (base_price_amount >= 0),
        CONSTRAINT fk_product_category FOREIGN KEY (category_id) REFERENCES catalog.category (category_id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE TABLE catalog.delivery_zone (
        delivery_zone_id uuid NOT NULL,
        store_id uuid NOT NULL,
        name character varying(200) NOT NULL,
        is_active boolean NOT NULL,
        created_at timestamptz NOT NULL,
        updated_at timestamptz,
        CONSTRAINT pk_delivery_zone PRIMARY KEY (delivery_zone_id),
        CONSTRAINT fk_delivery_zone_store FOREIGN KEY (store_id) REFERENCES catalog.store (store_id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE INDEX ix_delivery_zone_store_id ON catalog.delivery_zone (store_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE INDEX ix_product_category_id ON catalog.product (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE INDEX ix_product_is_active ON catalog.product (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE UNIQUE INDEX uq_product_sku ON catalog.product (sku);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    CREATE UNIQUE INDEX uq_store_code ON catalog.store (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231742_Catalog_InitialCreate') THEN
    INSERT INTO catalog.__ef_migrations_history_catalog ("MigrationId", "ProductVersion")
    VALUES ('20261113231742_Catalog_InitialCreate', '10.0.14');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    ALTER TABLE catalog.store ALTER COLUMN longitude DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    ALTER TABLE catalog.store ALTER COLUMN latitude DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    CREATE TABLE catalog.store_assortment (
        store_assortment_id uuid NOT NULL,
        store_id uuid NOT NULL,
        product_id uuid NOT NULL,
        is_listed boolean NOT NULL,
        created_at timestamptz NOT NULL,
        updated_at timestamptz,
        CONSTRAINT pk_store_assortment PRIMARY KEY (store_assortment_id),
        CONSTRAINT fk_store_assortment_product FOREIGN KEY (product_id) REFERENCES catalog.product (product_id) ON DELETE RESTRICT,
        CONSTRAINT fk_store_assortment_store FOREIGN KEY (store_id) REFERENCES catalog.store (store_id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    ALTER TABLE catalog.store ADD CONSTRAINT ck_store_code_format CHECK (code ~ '^[A-Z0-9][A-Z0-9._-]{0,29}$');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    ALTER TABLE catalog.store ADD CONSTRAINT ck_store_location_complete CHECK ((latitude IS NULL) = (longitude IS NULL));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    ALTER TABLE catalog.product ADD CONSTRAINT ck_product_base_price_currency CHECK (base_price_currency ~ '^[A-Z]{3}$');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    ALTER TABLE catalog.product ADD CONSTRAINT ck_product_sku_format CHECK (sku ~ '^[A-Z0-9][A-Z0-9._-]{0,49}$');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    CREATE INDEX ix_store_assortment_product_id ON catalog.store_assortment (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    CREATE UNIQUE INDEX uq_store_assortment_store_id_product_id ON catalog.store_assortment (store_id, product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM catalog.__ef_migrations_history_catalog WHERE "MigrationId" = '20261113231851_Catalog_AssortmentAndCatalogConstraints') THEN
    INSERT INTO catalog.__ef_migrations_history_catalog ("MigrationId", "ProductVersion")
    VALUES ('20261113231851_Catalog_AssortmentAndCatalogConstraints', '10.0.14');
    END IF;
END $EF$;
COMMIT;

