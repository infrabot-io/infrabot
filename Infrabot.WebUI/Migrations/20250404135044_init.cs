using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrabot.WebUI.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    LogAction = table.Column<int>(type: "INTEGER", nullable: false),
                    LogItem = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsADEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ADServer = table.Column<string>(type: "TEXT", nullable: true),
                    ADLogin = table.Column<string>(type: "TEXT", nullable: true),
                    ADPassword = table.Column<string>(type: "TEXT", nullable: true),
                    ADDomainName = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramBotToken = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramEnableEmergency = table.Column<bool>(type: "INTEGER", nullable: false),
                    TelegramEnableShowMyId = table.Column<bool>(type: "INTEGER", nullable: false),
                    TelegramPowerShellPath = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramPowerShellArguments = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramLinuxShellPath = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramLinuxShellArguments = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramPythonPath = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramPythonArguments = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramResultMaxLength = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordPolicyMinLength = table.Column<int>(type: "INTEGER", nullable: false),
                    PasswordPolicyContainSpecialCharacter = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordPolicyContainNumber = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordPolicyContainLowerCase = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordPolicyContainUpperCase = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthChecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plugins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plugins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    TelegramUserUsername = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Surname = table.Column<string>(type: "TEXT", nullable: true),
                    TelegramId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Surname = table.Column<string>(type: "TEXT", nullable: false),
                    Login = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    IsADIntegrated = table.Column<bool>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermissionAssignmentGroups",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionAssignmentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionAssignmentGroups", x => new { x.GroupsId, x.PermissionAssignmentId });
                    table.ForeignKey(
                        name: "FK_PermissionAssignmentGroups_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionAssignmentGroups_PermissionAssignments_PermissionAssignmentId",
                        column: x => x.PermissionAssignmentId,
                        principalTable: "PermissionAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPlugins",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    PluginId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPlugins", x => new { x.GroupId, x.PluginId });
                    table.ForeignKey(
                        name: "FK_GroupPlugins_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPlugins_Plugins_PluginId",
                        column: x => x.PluginId,
                        principalTable: "Plugins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionAssignmentPlugins",
                columns: table => new
                {
                    PermissionAssignmentsId = table.Column<int>(type: "INTEGER", nullable: false),
                    PluginsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionAssignmentPlugins", x => new { x.PermissionAssignmentsId, x.PluginsId });
                    table.ForeignKey(
                        name: "FK_PermissionAssignmentPlugins_PermissionAssignments_PermissionAssignmentsId",
                        column: x => x.PermissionAssignmentsId,
                        principalTable: "PermissionAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionAssignmentPlugins_Plugins_PluginsId",
                        column: x => x.PluginsId,
                        principalTable: "Plugins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermissionAssignmentTelegramUsers",
                columns: table => new
                {
                    PermissionAssignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    TelegramUsersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionAssignmentTelegramUsers", x => new { x.PermissionAssignmentId, x.TelegramUsersId });
                    table.ForeignKey(
                        name: "FK_PermissionAssignmentTelegramUsers_PermissionAssignments_PermissionAssignmentId",
                        column: x => x.PermissionAssignmentId,
                        principalTable: "PermissionAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionAssignmentTelegramUsers_TelegramUsers_TelegramUsersId",
                        column: x => x.TelegramUsersId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    TelegramUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => new { x.GroupId, x.TelegramUserId });
                    table.ForeignKey(
                        name: "FK_UserGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroups_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Configurations",
                columns: new[] { "Id", "ADDomainName", "ADLogin", "ADPassword", "ADServer", "IsADEnabled", "PasswordPolicyContainLowerCase", "PasswordPolicyContainNumber", "PasswordPolicyContainSpecialCharacter", "PasswordPolicyContainUpperCase", "PasswordPolicyMinLength", "TelegramBotToken", "TelegramEnableEmergency", "TelegramEnableShowMyId", "TelegramLinuxShellArguments", "TelegramLinuxShellPath", "TelegramPowerShellArguments", "TelegramPowerShellPath", "TelegramPythonArguments", "TelegramPythonPath", "TelegramResultMaxLength", "UpdatedDate" },
                values: new object[] { 1, "SYsbRFILmcFM0MIyxaZwog==", "2H3bbx3JbmWNiC/x4cIAnw==", "YE0BX7w+ABITvhhnLfljbQ==", "SYsbRFILmcFM0MIyxaZwog==", false, false, false, false, false, 6, "GNtoRPYw2FqDvdNlKDCQaSemluf0E7q0Xq+8YsKIYEm9eSqror56jw+HtgkKV2gU", true, true, "", "/bin/bash", "-ExecutionPolicy Unrestricted -NoProfile", "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe", "", "/usr/bin/python", 12000, new DateTime(2025, 4, 4, 17, 50, 44, 289, DateTimeKind.Local).AddTicks(6046) });

            migrationBuilder.InsertData(
                table: "TelegramUsers",
                columns: new[] { "Id", "CreatedDate", "Name", "Surname", "TelegramId", "UpdatedDate" },
                values: new object[] { 1, new DateTime(2025, 4, 4, 17, 50, 44, 289, DateTimeKind.Local).AddTicks(1097), "Akshin", "Mustafayev", 816058261, new DateTime(2025, 4, 4, 17, 50, 44, 289, DateTimeKind.Local).AddTicks(1103) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Email", "Enabled", "IsADIntegrated", "LastLoginDate", "Login", "Name", "Password", "Phone", "Surname", "UpdatedDate" },
                values: new object[] { 1, new DateTime(2025, 4, 4, 17, 50, 44, 288, DateTimeKind.Local).AddTicks(4214), "admin@aa.com", true, false, new DateTime(2025, 4, 4, 17, 50, 44, 288, DateTimeKind.Local).AddTicks(4569), "admin", "admin", "N+7fqbo7CkC1aLOQIBmu4A==", "12313123", "aa", new DateTime(2025, 4, 4, 17, 50, 44, 288, DateTimeKind.Local).AddTicks(4415) });

            migrationBuilder.CreateIndex(
                name: "IX_GroupPlugins_PluginId",
                table: "GroupPlugins",
                column: "PluginId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionAssignmentGroups_PermissionAssignmentId",
                table: "PermissionAssignmentGroups",
                column: "PermissionAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionAssignmentPlugins_PluginsId",
                table: "PermissionAssignmentPlugins",
                column: "PluginsId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionAssignmentTelegramUsers_TelegramUsersId",
                table: "PermissionAssignmentTelegramUsers",
                column: "TelegramUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_TelegramUserId",
                table: "UserGroups",
                column: "TelegramUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropTable(
                name: "GroupPlugins");

            migrationBuilder.DropTable(
                name: "HealthChecks");

            migrationBuilder.DropTable(
                name: "PermissionAssignmentGroups");

            migrationBuilder.DropTable(
                name: "PermissionAssignmentPlugins");

            migrationBuilder.DropTable(
                name: "PermissionAssignmentTelegramUsers");

            migrationBuilder.DropTable(
                name: "TelegramMessages");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Plugins");

            migrationBuilder.DropTable(
                name: "PermissionAssignments");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "TelegramUsers");
        }
    }
}
