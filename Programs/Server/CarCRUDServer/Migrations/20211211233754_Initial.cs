using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CarCRUD.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarBrands",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarBrands", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fullname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    passwordAttempts = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    accountDeleteRequested = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CarType",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand = table.Column<int>(type: "int", nullable: false),
                    brandDataID = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarType", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CarType_CarBrands_brandDataID",
                        column: x => x.brandDataID,
                        principalTable: "CarBrands",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRequests",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    user = table.Column<int>(type: "int", nullable: false),
                    userDataID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequests", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserRequests_Users_userDataID",
                        column: x => x.userDataID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FavouriteCars",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cartype = table.Column<int>(type: "int", nullable: false),
                    carTypeDataID = table.Column<int>(type: "int", nullable: true),
                    user = table.Column<int>(type: "int", nullable: false),
                    userDataID = table.Column<int>(type: "int", nullable: true),
                    year = table.Column<int>(type: "int", nullable: false),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fuel = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteCars", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FavouriteCars_CarType_carTypeDataID",
                        column: x => x.carTypeDataID,
                        principalTable: "CarType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FavouriteCars_Users_userDataID",
                        column: x => x.userDataID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarImages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    favouriteCar = table.Column<int>(type: "int", nullable: false),
                    favouriteCarDataID = table.Column<int>(type: "int", nullable: true),
                    image = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarImages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CarImages_FavouriteCars_favouriteCarDataID",
                        column: x => x.favouriteCarDataID,
                        principalTable: "FavouriteCars",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarImages_favouriteCarDataID",
                table: "CarImages",
                column: "favouriteCarDataID");

            migrationBuilder.CreateIndex(
                name: "IX_CarType_brandDataID",
                table: "CarType",
                column: "brandDataID");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteCars_carTypeDataID",
                table: "FavouriteCars",
                column: "carTypeDataID");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteCars_userDataID",
                table: "FavouriteCars",
                column: "userDataID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequests_userDataID",
                table: "UserRequests",
                column: "userDataID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarImages");

            migrationBuilder.DropTable(
                name: "UserRequests");

            migrationBuilder.DropTable(
                name: "FavouriteCars");

            migrationBuilder.DropTable(
                name: "CarType");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CarBrands");
        }
    }
}
