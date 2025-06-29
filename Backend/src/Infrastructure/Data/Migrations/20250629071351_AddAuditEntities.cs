using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthenticationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AuthenticationMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdditionalData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthenticationAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FailedLoginAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdditionalData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedLoginAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FailedLoginAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdditionalData = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Investigated = table.Column<bool>(type: "boolean", nullable: false),
                    InvestigationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OldValues = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NewValues = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_EventType",
                table: "AuthenticationAuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_IpAddress",
                table: "AuthenticationAuditLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_Result",
                table: "AuthenticationAuditLogs",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_Timestamp",
                table: "AuthenticationAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_UserId",
                table: "AuthenticationAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FailedLoginAttempts_AttemptTime",
                table: "FailedLoginAttempts",
                column: "AttemptTime");

            migrationBuilder.CreateIndex(
                name: "IX_FailedLoginAttempts_IpAddress",
                table: "FailedLoginAttempts",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_FailedLoginAttempts_IpAddress_AttemptTime",
                table: "FailedLoginAttempts",
                columns: new[] { "IpAddress", "AttemptTime" });

            migrationBuilder.CreateIndex(
                name: "IX_FailedLoginAttempts_UserId",
                table: "FailedLoginAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_EventType",
                table: "SecurityAuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Investigated",
                table: "SecurityAuditLogs",
                column: "Investigated");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_IpAddress",
                table: "SecurityAuditLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Severity",
                table: "SecurityAuditLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Timestamp",
                table: "SecurityAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_UserId",
                table: "SecurityAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityAuditLogs_Action",
                table: "UserActivityAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityAuditLogs_Resource",
                table: "UserActivityAuditLogs",
                column: "Resource");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityAuditLogs_Resource_ResourceId",
                table: "UserActivityAuditLogs",
                columns: new[] { "Resource", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityAuditLogs_ResourceId",
                table: "UserActivityAuditLogs",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityAuditLogs_Timestamp",
                table: "UserActivityAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityAuditLogs_UserId",
                table: "UserActivityAuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationAuditLogs");

            migrationBuilder.DropTable(
                name: "FailedLoginAttempts");

            migrationBuilder.DropTable(
                name: "SecurityAuditLogs");

            migrationBuilder.DropTable(
                name: "UserActivityAuditLogs");
        }
    }
}
