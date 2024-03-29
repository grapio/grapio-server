using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grapio.Server.Migrations
{
    /// <inheritdoc />
    public partial class Consumer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FeatureFlags",
                table: "FeatureFlags");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "FeatureFlags",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "FeatureFlags",
                type: "VARCHAR(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Consumer",
                table: "FeatureFlags",
                type: "VARCHAR(150)",
                nullable: false,
                defaultValue: "*");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeatureFlags",
                table: "FeatureFlags",
                columns: new[] { "Key", "Consumer" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FeatureFlags",
                table: "FeatureFlags");

            migrationBuilder.DropColumn(
                name: "Consumer",
                table: "FeatureFlags");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "FeatureFlags",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "FeatureFlags",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeatureFlags",
                table: "FeatureFlags",
                column: "Key");
        }
    }
}
