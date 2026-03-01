using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlCenter.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpaceHierarchyRelationsDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Space hierarchy: add ParentSpaceId column
            migrationBuilder.AddColumn<Guid>(
                name: "ParentSpaceId",
                table: "spaces",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_spaces_ParentSpaceId",
                table: "spaces",
                column: "ParentSpaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_spaces_spaces_ParentSpaceId",
                table: "spaces",
                column: "ParentSpaceId",
                principalTable: "spaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // SpaceRelations table
            migrationBuilder.CreateTable(
                name: "space_relations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SourceSpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetSpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_space_relations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_space_relations_spaces_SourceSpaceId",
                        column: x => x.SourceSpaceId,
                        principalTable: "spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_space_relations_spaces_TargetSpaceId",
                        column: x => x.TargetSpaceId,
                        principalTable: "spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_space_relations_SourceSpaceId_TargetSpaceId_RelationType",
                table: "space_relations",
                columns: new[] { "SourceSpaceId", "TargetSpaceId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_space_relations_TargetSpaceId",
                table: "space_relations",
                column: "TargetSpaceId");

            // Documents table
            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_documents_spaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documents_SpaceId_Slug",
                table: "documents",
                columns: new[] { "SpaceId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documents_SpaceId_SortOrder",
                table: "documents",
                columns: new[] { "SpaceId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "documents");
            migrationBuilder.DropTable(name: "space_relations");

            migrationBuilder.DropForeignKey(
                name: "FK_spaces_spaces_ParentSpaceId",
                table: "spaces");

            migrationBuilder.DropIndex(
                name: "IX_spaces_ParentSpaceId",
                table: "spaces");

            migrationBuilder.DropColumn(
                name: "ParentSpaceId",
                table: "spaces");
        }
    }
}
