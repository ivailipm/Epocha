using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Epocha.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceSystem = table.Column<int>(type: "integer", nullable: false),
                    SourceExternalId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BirthYear = table.Column<int>(type: "integer", nullable: true),
                    DeathYear = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Artworks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceSystem = table.Column<int>(type: "integer", nullable: false),
                    SourceExternalId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SourceUrl = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MediumDisplay = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    MediumCategory = table.Column<int>(type: "integer", nullable: false),
                    Department = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Dimensions = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreditLine = table.Column<string>(type: "text", nullable: true),
                    DateDisplay = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DateStartYear = table.Column<int>(type: "integer", nullable: true),
                    DateEndYear = table.Column<int>(type: "integer", nullable: true),
                    Era = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    IsPublicDomain = table.Column<bool>(type: "boolean", nullable: false),
                    ArtistId = table.Column<int>(type: "integer", nullable: true),
                    LastIndexedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artworks_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ArtworkMovement",
                columns: table => new
                {
                    ArtworksId = table.Column<int>(type: "integer", nullable: false),
                    MovementsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtworkMovement", x => new { x.ArtworksId, x.MovementsId });
                    table.ForeignKey(
                        name: "FK_ArtworkMovement_Artworks_ArtworksId",
                        column: x => x.ArtworksId,
                        principalTable: "Artworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtworkMovement_Movements_MovementsId",
                        column: x => x.MovementsId,
                        principalTable: "Movements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artists_SourceSystem_SourceExternalId",
                table: "Artists",
                columns: new[] { "SourceSystem", "SourceExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtworkMovement_MovementsId",
                table: "ArtworkMovement",
                column: "MovementsId");

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_ArtistId",
                table: "Artworks",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_Era",
                table: "Artworks",
                column: "Era");

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_LastIndexedAt",
                table: "Artworks",
                column: "LastIndexedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_SourceSystem_SourceExternalId",
                table: "Artworks",
                columns: new[] { "SourceSystem", "SourceExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movements_Slug",
                table: "Movements",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtworkMovement");

            migrationBuilder.DropTable(
                name: "Artworks");

            migrationBuilder.DropTable(
                name: "Movements");

            migrationBuilder.DropTable(
                name: "Artists");
        }
    }
}
