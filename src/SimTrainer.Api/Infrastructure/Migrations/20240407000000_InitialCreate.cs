using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimTrainer.Api.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Learners",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Learners", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TrainingSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LearnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SessionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                OverallScore = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TrainingSessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_TrainingSessions_Learners_LearnerId",
                    column: x => x.LearnerId,
                    principalTable: "Learners",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CompressionEvents",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                DepthCm = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                RateBpm = table.Column<int>(type: "int", nullable: false),
                FullRecoil = table.Column<bool>(type: "bit", nullable: false),
                QualityScore = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompressionEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_CompressionEvents_TrainingSessions_SessionId",
                    column: x => x.SessionId,
                    principalTable: "TrainingSessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "VitalSnapshots",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                HeartRate = table.Column<int>(type: "int", nullable: false),
                SpO2 = table.Column<int>(type: "int", nullable: false),
                SystolicBp = table.Column<int>(type: "int", nullable: false),
                DiastolicBp = table.Column<int>(type: "int", nullable: false),
                RespiratoryRate = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VitalSnapshots", x => x.Id);
                table.ForeignKey(
                    name: "FK_VitalSnapshots_TrainingSessions_SessionId",
                    column: x => x.SessionId,
                    principalTable: "TrainingSessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ScenarioChanges",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                ScenarioName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PreviousScenario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ScenarioChanges", x => x.Id);
                table.ForeignKey(
                    name: "FK_ScenarioChanges_TrainingSessions_SessionId",
                    column: x => x.SessionId,
                    principalTable: "TrainingSessions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CompressionEvents_SessionId",
            table: "CompressionEvents",
            column: "SessionId");

        migrationBuilder.CreateIndex(
            name: "IX_TrainingSessions_LearnerId",
            table: "TrainingSessions",
            column: "LearnerId");

        migrationBuilder.CreateIndex(
            name: "IX_VitalSnapshots_SessionId",
            table: "VitalSnapshots",
            column: "SessionId");

        migrationBuilder.CreateIndex(
            name: "IX_ScenarioChanges_SessionId",
            table: "ScenarioChanges",
            column: "SessionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ScenarioChanges");
        migrationBuilder.DropTable(name: "VitalSnapshots");
        migrationBuilder.DropTable(name: "CompressionEvents");
        migrationBuilder.DropTable(name: "TrainingSessions");
        migrationBuilder.DropTable(name: "Learners");
    }
}
