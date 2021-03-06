﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace CollAction.Migrations
{
    public partial class CA591RemoveCategoryFriesland : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"Categories\" WHERE \"Name\" = 'Friesland'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO \"Categories\" (\"Color\", \"Description\", \"File\", \"Name\") VALUES ( 32067, 'Friesland', '', 'Friesland')");
        }
    }
}
