using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SampleShopApi.App.Data.migrations
{
    /// <inheritdoc />
    public partial class AddCoverUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArtistPhotoUrl",
                table: "Artists",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AlbumCoverUrl",
                table: "Albums",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArtistPhotoUrl",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "AlbumCoverUrl",
                table: "Albums");
        }
    }
}
