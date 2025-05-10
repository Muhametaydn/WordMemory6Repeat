using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordMemoryApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) OwnerId sütununu önce NULL kabul eder şekilde ekle
            migrationBuilder.AddColumn<int?>(
                name: "OwnerId",
                table: "Words",
                nullable: true);

            // 2) Var olan kelimelere geçerli bir kullanıcı ata.
            //    Burada UserID’si 1 olan bir kullanıcıya gönderiyoruz.
            //    Eğer Users tablosunda ID=1 yoksa, aşağıdaki SQL o kullanıcıyı oluşturup kullanır.
            migrationBuilder.Sql(@"
        IF EXISTS (SELECT 1 FROM [Users] WHERE [UserID] = 1)
        BEGIN
            UPDATE [Words] SET [OwnerId] = 1;
        END
        ELSE
        BEGIN
            INSERT INTO [Users] ([UserName],[Email],[PasswordHash])
            VALUES ('default','default@example.com','');
            DECLARE @newId INT = SCOPE_IDENTITY();
            UPDATE [Words] SET [OwnerId] = @newId;
        END
    ");

            // 3) Şimdi sütunu NOT NULL yap
            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Words",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            // 4) Index ve Foreign Key ekle
            migrationBuilder.CreateIndex(
                name: "IX_Words_OwnerId",
                table: "Words",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Words_Users_OwnerId",
                table: "Words",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Words_Users_OwnerId",
                table: "Words");

            migrationBuilder.DropIndex(
                name: "IX_Words_OwnerId",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Words");
        }
    }
}
