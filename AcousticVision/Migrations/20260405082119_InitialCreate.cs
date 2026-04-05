using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcousticVision.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    NoiseCancelation = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Length = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoundReceivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Properties = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundReceivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SoundSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Volume = table.Column<double>(type: "REAL", nullable: false),
                    Article = table.Column<double>(type: "REAL", nullable: true),
                    Properties = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoundSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Textures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    NoiseCancelation = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Textures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiverId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestModels_RoomModels_RoomId",
                        column: x => x.RoomId,
                        principalTable: "RoomModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestModels_SoundReceivers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "SoundReceivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestModels_SoundSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "SoundSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Walls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    TextureId = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Walls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Walls_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Walls_Textures_TextureId",
                        column: x => x.TextureId,
                        principalTable: "Textures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomWalls",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "INTEGER", nullable: false),
                    WallId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomWalls", x => new { x.RoomId, x.WallId });
                    table.ForeignKey(
                        name: "FK_RoomWalls_RoomModels_RoomId",
                        column: x => x.RoomId,
                        principalTable: "RoomModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomWalls_Walls_WallId",
                        column: x => x.WallId,
                        principalTable: "Walls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name",
                table: "Materials",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomModels_Name",
                table: "RoomModels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomWalls_RoomId_Position",
                table: "RoomWalls",
                columns: new[] { "RoomId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomWalls_WallId",
                table: "RoomWalls",
                column: "WallId");

            migrationBuilder.CreateIndex(
                name: "IX_SoundReceivers_Name",
                table: "SoundReceivers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoundSources_Name",
                table: "SoundSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestModels_ReceiverId",
                table: "TestModels",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_TestModels_RoomId_SourceId_ReceiverId",
                table: "TestModels",
                columns: new[] { "RoomId", "SourceId", "ReceiverId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestModels_SourceId",
                table: "TestModels",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Textures_Name",
                table: "Textures",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Walls_MaterialId",
                table: "Walls",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Walls_TextureId",
                table: "Walls",
                column: "TextureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomWalls");

            migrationBuilder.DropTable(
                name: "TestModels");

            migrationBuilder.DropTable(
                name: "Walls");

            migrationBuilder.DropTable(
                name: "RoomModels");

            migrationBuilder.DropTable(
                name: "SoundReceivers");

            migrationBuilder.DropTable(
                name: "SoundSources");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "Textures");
        }
    }
}
