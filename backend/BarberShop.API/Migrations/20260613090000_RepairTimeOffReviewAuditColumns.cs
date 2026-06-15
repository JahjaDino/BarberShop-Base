using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShop.API.Migrations
{
    /// <inheritdoc />
    public partial class RepairTimeOffReviewAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "TimeOffs"
                ADD COLUMN IF NOT EXISTS "ReviewNote" text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "TimeOffs"
                ADD COLUMN IF NOT EXISTS "ReviewedAt" timestamp with time zone;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "TimeOffs"
                ADD COLUMN IF NOT EXISTS "ReviewedByUserId" integer;
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_TimeOffs_ReviewedByUserId"
                ON "TimeOffs" ("ReviewedByUserId");
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_TimeOffs_Users_ReviewedByUserId'
                    ) THEN
                        ALTER TABLE "TimeOffs"
                        ADD CONSTRAINT "FK_TimeOffs_Users_ReviewedByUserId"
                        FOREIGN KEY ("ReviewedByUserId")
                        REFERENCES "Users" ("Id");
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "TimeOffs"
                DROP CONSTRAINT IF EXISTS "FK_TimeOffs_Users_ReviewedByUserId";
                """);

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_TimeOffs_ReviewedByUserId";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "TimeOffs"
                DROP COLUMN IF EXISTS "ReviewedByUserId";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "TimeOffs"
                DROP COLUMN IF EXISTS "ReviewedAt";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "TimeOffs"
                DROP COLUMN IF EXISTS "ReviewNote";
                """);
        }
    }
}
