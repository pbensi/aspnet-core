using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.migrator.Migrations
{
    /// <inheritdoc />
    public partial class Add_Account : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Guid AdminUserGuid = Guid.NewGuid();

            migrationBuilder.InsertData(
                table: "PersonalDetails",
                columns: new[] {
                    "Id",
                    "UserGuid",
                    "FirstName",
                    "LastName",
                    "MiddleName",
                    "Gender",
                    "BirthDate",
                    "Email",
                    "IsDelete",
                    "CreatedBy",
                    "CreatedAt",
                    "LastModifiedBy",
                    "LastModifiedAt"
                },
                values: new object[] {
                    1,
                    AdminUserGuid,
                    "Admin",
                    "Admin",
                    "Admin",
                    "Male",
                    DateTime.UtcNow.Date,
                    "Email@Email.com",
                    false,
                    "Admin",
                    DateTime.UtcNow,
                    "Admin",
                    DateTime.UtcNow
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] {
                    "UserGuid",
                    "UserName",
                    "Password",
                    "IsActive",
                    "IsAdmin",
                    "CreatedBy",
                    "CreatedAt",
                    "LastModifiedBy",
                    "LastModifiedAt"
                },
                values: new object[] {
                    AdminUserGuid,
                    "Admin",
                    "LluV+z/hDV5GROphcvQfhvDbZkpcr8ViluLdLVGOK4g=:100000:0+gLO4Aq6IaIEHdw0f7ajcGSGGp7HSDQ4sbiM7RzAiU0UUE7n4ms8lBRuxwwrXGVVre2rkM4c0Ra/3kPPb2s3Q==",
                    true,
                    true,
                    "Admin",
                    DateTime.UtcNow,
                    "Admin",
                    DateTime.UtcNow
                });

            migrationBuilder.InsertData(
                table: "AccountRoles",
                columns: new[] {
                    "Id",
                    "UserGuid",
                    "PageName",
                    "LabelName",
                    "IsPost",
                    "IsPut",
                    "IsDelete",
                    "IsGet",
                    "IsOptions"
                },
                values: new object[,]
                {
                    { 1, AdminUserGuid, "Pages.Users", "Users", false, true, false, true, false },
                    { 2, AdminUserGuid, "Pages.Swagger", "Swagger", false, true, false, true, false }
                });

            migrationBuilder.InsertData(
                table: "AccountSecurities",
                columns: new[] {
                    "UserGuid",
                    "PublicKey",
                    "PublicIV",
                    "PrivateKey",
                    "PrivateIV",
                    "DeviceName",
                    "Ipv4Address",
                    "Ipv6Address",
                    "OperatingSystem"
                },
                values: new object[] {
                    AdminUserGuid,
                    $"sV3QvGAslEOC7VwiLFL97f1hyWg0u3WXKiAY6gWv6Vc=",
                    $"O1blOZzN+W7v15VjX7wNQQ==",
                    $"LQKQbdbbYz9/1Wvq+3y0UcZzWneEASRq1pT6RgzxAhI=",
                    $"HTthV95OZR8NjNXVkevLig==",
                    NetworkProvider.DeviceName(),
                    NetworkProvider.Ipv4Address(),
                    NetworkProvider.Ipv6Address(),
                    NetworkProvider.OperatingSystem()
                });

            migrationBuilder.InsertData(
                table: "AccountSecurityLogs",
                columns: new[] {
                    "Id",
                    "UserGuid",
                    "OldPublicKey",
                    "OldPublicIV",
                    "OldPrivateKey",
                    "OldPrivateIV",
                    "DeviceName",
                    "Ipv4Address",
                    "Ipv6Address",
                    "OperatingSystem",
                    "CreatedBy",
                    "CreatedAt",
                    "LastModifiedBy",
                    "LastModifiedAt"
                },
                values: new object[] {
                    1,
                    AdminUserGuid,
                    $"sV3QvGAslEOC7VwiLFL97f1hyWg0u3WXKiAY6gWv6Vc=",
                    $"O1blOZzN+W7v15VjX7wNQQ==",
                    $"LQKQbdbbYz9/1Wvq+3y0UcZzWneEASRq1pT6RgzxAhI=",
                    $"HTthV95OZR8NjNXVkevLig==",
                    NetworkProvider.DeviceName(),
                    NetworkProvider.Ipv4Address(),
                    NetworkProvider.Ipv6Address(),
                    NetworkProvider.OperatingSystem(),
                    "Admin",
                    DateTime.UtcNow,
                    "Admin",
                    DateTime.UtcNow
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
