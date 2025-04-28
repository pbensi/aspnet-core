using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.migrator.Migrations
{
    /// <inheritdoc />
    public partial class Add_OpenResourcePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "OpenResourcePaths",
            columns: new[] { "Id", "RequestMethod", "RequestPath", "Description", "AllowedRole" },
            values: new object[,]
            {
                { 1,"GET", "/","The server root URL is used to restrict direct access to Swagger","Server" },
                { 2,"GET", "/Home/SwaggerSignIn","The server view signin is used to restrict direct access to Swagger","Server" },
                { 3,"POST", "/Home/SwaggerSignIn","The server action signin are allowed to make requests without needing a user role","Server" },
                { 4,"GET", "/api/Presentation/Localization/GetXmlLocalization","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 5,"GET", "/api/Presentation/Localization/GetJsonLocalization","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 6,"GET", "/api/Presentation/Localization/GetResources","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 7,"GET", "/api/Presentation/Account/GetPublicEncryptionByUserName","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 8,"POST", "/api/Presentation/PersonalDetail/CreatePersonalDetailForAccountAsync","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 9,"GET", "/api/Presentation/Account/SignInAccountAsync","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 10,"POST", "/api/Presentation/Asymmetric/GenerateKeyPairAsync","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 11,"POST", "/api/Presentation/Asymmetric/EncryptDataAsync","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 12,"POST", "/api/Presentation/Asymmetric/SignDataAsync","ApiEndPoint are allowed to make requests without needing a user role","Client" },
                { 13,"POST", "/api/Presentation/Asymmetric/GenerateKeyPairAsync","ApiEndPoint are allowed to make requests without needing a user role","Server" },
                { 14,"POST", "/api/Presentation/Asymmetric/EncryptDataAsync","ApiEndPoint are allowed to make requests without needing a user role","Server" },
                { 15,"POST", "/api/Presentation/Asymmetric/SignDataAsync","ApiEndPoint are allowed to make requests without needing a user role","Server" },
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
            table: "OpenResourcePaths",
            keyColumn: "Id",
            keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15 });
        }
    }
}
