using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Infrastructure.Outbox.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxNotifyTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccurredOnUtc",
                table: "OutboxMessages",
                column: "OccurredOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION notify_outbox_change()
                RETURNS trigger AS $$
                BEGIN
                  PERFORM pg_notify('outbox_channel', row_to_json(NEW)::text);
                  RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                DROP TRIGGER IF EXISTS outbox_change_trigger ON "OutboxMessages";

                CREATE TRIGGER outbox_change_trigger
                AFTER INSERT ON "OutboxMessages"
                FOR EACH ROW EXECUTE FUNCTION notify_outbox_change();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS outbox_change_trigger ON "OutboxMessages";
                DROP FUNCTION IF EXISTS notify_outbox_change();
                """);

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_OccurredOnUtc",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "OutboxMessages");
        }
    }
}
