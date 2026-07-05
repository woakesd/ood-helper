using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OodHelper.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "boats",
                columns: table => new
                {
                    bid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    boatname = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    boatclass = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    sailno = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    dinghy = table.Column<bool>(type: "INTEGER", nullable: true),
                    hulltype = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    distance = table.Column<int>(type: "INTEGER", nullable: true),
                    open_handicap = table.Column<int>(type: "INTEGER", nullable: true),
                    handicap_status = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    rolling_handicap = table.Column<int>(type: "INTEGER", nullable: true),
                    crew_skill_factor = table.Column<int>(type: "INTEGER", nullable: true),
                    small_cat_handicap_rating = table.Column<decimal>(type: "TEXT", nullable: true),
                    engine_propeller = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    keel = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    deviations = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    subscription = table.Column<string>(type: "TEXT", maxLength: 26, nullable: true),
                    boatmemo = table.Column<string>(type: "TEXT", nullable: true),
                    berth = table.Column<string>(type: "TEXT", maxLength: 6, nullable: true),
                    hired = table.Column<bool>(type: "INTEGER", nullable: true),
                    p = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    s = table.Column<bool>(type: "INTEGER", nullable: true),
                    beaten = table.Column<int>(type: "INTEGER", nullable: true),
                    uid = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boats", x => x.bid);
                });

            migrationBuilder.CreateTable(
                name: "calendar",
                columns: table => new
                {
                    rid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    time_limit_type = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    time_limit_fixed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    time_limit_delta = table.Column<int>(type: "INTEGER", nullable: true),
                    extension = table.Column<int>(type: "INTEGER", nullable: true),
                    @class = table.Column<string>(name: "class", type: "TEXT", maxLength: 20, nullable: true),
                    @event = table.Column<string>(name: "event", type: "TEXT", maxLength: 34, nullable: true),
                    price_code = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    course = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true),
                    ood = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    venue = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    racetype = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    handicapping = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    visitors = table.Column<int>(type: "INTEGER", nullable: true),
                    flag = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    memo = table.Column<string>(type: "TEXT", nullable: true),
                    is_race = table.Column<bool>(type: "INTEGER", nullable: true),
                    raced = table.Column<bool>(type: "INTEGER", nullable: true),
                    approved = table.Column<bool>(type: "INTEGER", nullable: true),
                    course_choice = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    laps_completed = table.Column<int>(type: "INTEGER", nullable: true),
                    wind_speed = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    wind_direction = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    standard_corrected_time = table.Column<double>(type: "REAL", nullable: true),
                    result_calculated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar", x => x.rid);
                });

            migrationBuilder.CreateTable(
                name: "portsmouth_numbers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    class_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    no_of_crew = table.Column<int>(type: "INTEGER", nullable: true),
                    rig = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    spinnaker = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    engine = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    keel = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    number = table.Column<int>(type: "INTEGER", nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portsmouth_numbers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "select_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    parent = table.Column<Guid>(type: "TEXT", nullable: true),
                    application = table.Column<int>(type: "INTEGER", nullable: true),
                    field = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    condition = table.Column<int>(type: "INTEGER", nullable: true),
                    string_value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    number_bound1 = table.Column<decimal>(type: "TEXT", nullable: true),
                    number_bound2 = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_select_rule", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "series",
                columns: table => new
                {
                    sid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    sname = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    discards = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_series", x => x.sid);
                });

            migrationBuilder.CreateTable(
                name: "series_results",
                columns: table => new
                {
                    sid = table.Column<int>(type: "INTEGER", nullable: false),
                    bid = table.Column<int>(type: "INTEGER", nullable: false),
                    division = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    entered = table.Column<int>(type: "INTEGER", nullable: true),
                    gross = table.Column<double>(type: "REAL", nullable: true),
                    nett = table.Column<double>(type: "REAL", nullable: true),
                    place = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_series_results", x => new { x.sid, x.division, x.bid });
                });

            migrationBuilder.CreateTable(
                name: "updates",
                columns: table => new
                {
                    dummy = table.Column<int>(type: "INTEGER", nullable: true),
                    upload = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "races",
                columns: table => new
                {
                    rid = table.Column<int>(type: "INTEGER", nullable: false),
                    bid = table.Column<int>(type: "INTEGER", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    finish_code = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    finish_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    interim_date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    restricted_sail = table.Column<bool>(type: "INTEGER", nullable: true),
                    last_edit = table.Column<DateTime>(type: "TEXT", nullable: true),
                    laps = table.Column<int>(type: "INTEGER", nullable: true),
                    place = table.Column<int>(type: "INTEGER", nullable: true),
                    points = table.Column<double>(type: "REAL", nullable: true),
                    override_points = table.Column<double>(type: "REAL", nullable: true),
                    elapsed = table.Column<int>(type: "INTEGER", nullable: true),
                    corrected = table.Column<double>(type: "REAL", nullable: true),
                    standard_corrected = table.Column<double>(type: "REAL", nullable: true),
                    handicap_status = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    open_handicap = table.Column<int>(type: "INTEGER", nullable: true),
                    rolling_handicap = table.Column<int>(type: "INTEGER", nullable: true),
                    achieved_handicap = table.Column<int>(type: "INTEGER", nullable: true),
                    new_rolling_handicap = table.Column<int>(type: "INTEGER", nullable: true),
                    performance_index = table.Column<int>(type: "INTEGER", nullable: true),
                    a = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    c = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_races", x => new { x.rid, x.bid });
                    table.ForeignKey(
                        name: "FK_races_boats",
                        column: x => x.bid,
                        principalTable: "boats",
                        principalColumn: "bid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_races_calendar",
                        column: x => x.rid,
                        principalTable: "calendar",
                        principalColumn: "rid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "calendar_series_join",
                columns: table => new
                {
                    sid = table.Column<int>(type: "INTEGER", nullable: false),
                    rid = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_series_join", x => new { x.sid, x.rid });
                    table.ForeignKey(
                        name: "FK_calendar_series_join_calendar",
                        column: x => x.rid,
                        principalTable: "calendar",
                        principalColumn: "rid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_calendar_series_join_series",
                        column: x => x.sid,
                        principalTable: "series",
                        principalColumn: "sid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_series_join_rid",
                table: "calendar_series_join",
                column: "rid");

            migrationBuilder.CreateIndex(
                name: "IX_races_bid",
                table: "races",
                column: "bid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_series_join");

            migrationBuilder.DropTable(
                name: "portsmouth_numbers");

            migrationBuilder.DropTable(
                name: "races");

            migrationBuilder.DropTable(
                name: "select_rules");

            migrationBuilder.DropTable(
                name: "series_results");

            migrationBuilder.DropTable(
                name: "updates");

            migrationBuilder.DropTable(
                name: "series");

            migrationBuilder.DropTable(
                name: "boats");

            migrationBuilder.DropTable(
                name: "calendar");
        }
    }
}
