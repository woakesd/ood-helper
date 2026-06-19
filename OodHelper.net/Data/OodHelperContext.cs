using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OodHelper.Data.Entities;

namespace OodHelper.Data;

// Disambiguate from the legacy OodHelper.SeriesResult domain class, which is visible here
// via the parent namespace and would otherwise win over the imported entity type. This alias
// must live inside the namespace scope to take precedence over the enclosing namespace.
// Re-apply this alias if the context is re-scaffolded.
using SeriesResult = OodHelper.Data.Entities.SeriesResult;

public partial class OodHelperContext : DbContext
{
    public OodHelperContext(DbContextOptions<OodHelperContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Boat> Boats { get; set; }

    public virtual DbSet<BoatCrew> BoatCrews { get; set; }

    public virtual DbSet<Calendar> Calendars { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<PortsmouthNumber> PortsmouthNumbers { get; set; }

    public virtual DbSet<Race> Races { get; set; }

    public virtual DbSet<SelectRule> SelectRules { get; set; }

    public virtual DbSet<Series> Series { get; set; }

    public virtual DbSet<SeriesResult> SeriesResults { get; set; }

    public virtual DbSet<Update> Updates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boat>(entity =>
        {
            entity.HasKey(e => e.Bid);

            entity.ToTable("boats");

            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Beaten).HasColumnName("beaten");
            entity.Property(e => e.Berth)
                .HasMaxLength(6)
                .HasColumnName("berth");
            entity.Property(e => e.Boatclass)
                .HasMaxLength(20)
                .HasColumnName("boatclass");
            entity.Property(e => e.Boatmemo)
                .HasColumnType("ntext")
                .HasColumnName("boatmemo");
            entity.Property(e => e.Boatname)
                .HasMaxLength(20)
                .HasColumnName("boatname");
            entity.Property(e => e.CrewSkillFactor).HasColumnName("crew_skill_factor");
            entity.Property(e => e.Crewname)
                .HasMaxLength(30)
                .HasColumnName("crewname");
            entity.Property(e => e.Deviations)
                .HasMaxLength(30)
                .HasColumnName("deviations");
            entity.Property(e => e.Dinghy).HasColumnName("dinghy");
            entity.Property(e => e.Distance).HasColumnName("distance");
            entity.Property(e => e.EnginePropeller)
                .HasMaxLength(3)
                .HasColumnName("engine_propeller");
            entity.Property(e => e.HandicapStatus)
                .HasMaxLength(2)
                .HasColumnName("handicap_status");
            entity.Property(e => e.Hired).HasColumnName("hired");
            entity.Property(e => e.Hulltype)
                .HasMaxLength(1)
                .HasColumnName("hulltype");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Keel)
                .HasMaxLength(2)
                .HasColumnName("keel");
            entity.Property(e => e.OpenHandicap).HasColumnName("open_handicap");
            entity.Property(e => e.P)
                .HasMaxLength(1)
                .HasColumnName("p");
            entity.Property(e => e.RollingHandicap).HasColumnName("rolling_handicap");
            entity.Property(e => e.S).HasColumnName("s");
            entity.Property(e => e.Sailno)
                .HasMaxLength(8)
                .HasColumnName("sailno");
            entity.Property(e => e.SmallCatHandicapRating)
                .HasColumnType("numeric(4, 3)")
                .HasColumnName("small_cat_handicap_rating");
            entity.Property(e => e.Subscription)
                .HasMaxLength(26)
                .HasColumnName("subscription");
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Boats)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_boats_people");
        });

        modelBuilder.Entity<BoatCrew>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Bid });

            entity.ToTable("boat_crew");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bid).HasColumnName("bid");
        });

        modelBuilder.Entity<Calendar>(entity =>
        {
            entity.HasKey(e => e.Rid);

            entity.ToTable("calendar");

            entity.Property(e => e.Rid).HasColumnName("rid");
            entity.Property(e => e.Approved).HasColumnName("approved");
            entity.Property(e => e.Class)
                .HasMaxLength(20)
                .HasColumnName("class");
            entity.Property(e => e.Course)
                .HasMaxLength(15)
                .HasColumnName("course");
            entity.Property(e => e.CourseChoice)
                .HasMaxLength(10)
                .HasColumnName("course_choice");
            entity.Property(e => e.Event)
                .HasMaxLength(34)
                .HasColumnName("event");
            entity.Property(e => e.Extension).HasColumnName("extension");
            entity.Property(e => e.Flag)
                .HasMaxLength(20)
                .HasColumnName("flag");
            entity.Property(e => e.Handicapping)
                .HasMaxLength(1)
                .HasColumnName("handicapping");
            entity.Property(e => e.IsRace).HasColumnName("is_race");
            entity.Property(e => e.LapsCompleted).HasColumnName("laps_completed");
            entity.Property(e => e.Memo)
                .HasColumnType("ntext")
                .HasColumnName("memo");
            entity.Property(e => e.Ood)
                .HasMaxLength(30)
                .HasColumnName("ood");
            entity.Property(e => e.PriceCode)
                .HasMaxLength(1)
                .HasColumnName("price_code");
            entity.Property(e => e.Raced).HasColumnName("raced");
            entity.Property(e => e.Racetype)
                .HasMaxLength(20)
                .HasColumnName("racetype");
            entity.Property(e => e.ResultCalculated)
                .HasColumnType("datetime")
                .HasColumnName("result_calculated");
            entity.Property(e => e.StandardCorrectedTime).HasColumnName("standard_corrected_time");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.TimeLimitDelta).HasColumnName("time_limit_delta");
            entity.Property(e => e.TimeLimitFixed)
                .HasColumnType("datetime")
                .HasColumnName("time_limit_fixed");
            entity.Property(e => e.TimeLimitType)
                .HasMaxLength(1)
                .HasColumnName("time_limit_type");
            entity.Property(e => e.Venue)
                .HasMaxLength(11)
                .HasColumnName("venue");
            entity.Property(e => e.Visitors).HasColumnName("visitors");
            entity.Property(e => e.WindDirection)
                .HasMaxLength(10)
                .HasColumnName("wind_direction");
            entity.Property(e => e.WindSpeed)
                .HasMaxLength(10)
                .HasColumnName("wind_speed");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("people");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address1)
                .HasMaxLength(30)
                .HasColumnName("address1");
            entity.Property(e => e.Address2)
                .HasMaxLength(30)
                .HasColumnName("address2");
            entity.Property(e => e.Address3)
                .HasMaxLength(30)
                .HasColumnName("address3");
            entity.Property(e => e.Address4)
                .HasMaxLength(30)
                .HasColumnName("address4");
            entity.Property(e => e.Club)
                .HasMaxLength(10)
                .HasColumnName("club");
            entity.Property(e => e.Cp).HasColumnName("cp");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Firstname)
                .HasMaxLength(20)
                .HasColumnName("firstname");
            entity.Property(e => e.Handbookexclude).HasColumnName("handbookexclude");
            entity.Property(e => e.Hometel)
                .HasMaxLength(20)
                .HasColumnName("hometel");
            entity.Property(e => e.MainId).HasColumnName("main_id");
            entity.Property(e => e.Manmemo)
                .HasColumnType("ntext")
                .HasColumnName("manmemo");
            entity.Property(e => e.Member)
                .HasMaxLength(6)
                .HasColumnName("member");
            entity.Property(e => e.Mobile)
                .HasMaxLength(20)
                .HasColumnName("mobile");
            entity.Property(e => e.Novice).HasColumnName("novice");
            entity.Property(e => e.Papernewsletter).HasColumnName("papernewsletter");
            entity.Property(e => e.Postcode)
                .HasMaxLength(9)
                .HasColumnName("postcode");
            entity.Property(e => e.S).HasColumnName("s");
            entity.Property(e => e.Surname)
                .HasMaxLength(28)
                .HasColumnName("surname");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Worktel)
                .HasMaxLength(20)
                .HasColumnName("worktel");
        });

        modelBuilder.Entity<PortsmouthNumber>(entity =>
        {
            entity.ToTable("portsmouth_numbers");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ClassName)
                .HasMaxLength(100)
                .HasColumnName("class_name");
            entity.Property(e => e.Engine)
                .HasMaxLength(3)
                .HasColumnName("engine");
            entity.Property(e => e.Keel)
                .HasMaxLength(1)
                .HasColumnName("keel");
            entity.Property(e => e.NoOfCrew).HasColumnName("no_of_crew");
            entity.Property(e => e.Notes)
                .HasColumnType("ntext")
                .HasColumnName("notes");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Rig)
                .HasMaxLength(1)
                .HasColumnName("rig");
            entity.Property(e => e.Spinnaker)
                .HasMaxLength(1)
                .HasColumnName("spinnaker");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Race>(entity =>
        {
            entity.HasKey(e => new { e.Rid, e.Bid }).IsClustered(false);

            entity.ToTable("races");

            entity.Property(e => e.Rid).HasColumnName("rid");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.A)
                .HasMaxLength(1)
                .HasColumnName("a");
            entity.Property(e => e.AchievedHandicap).HasColumnName("achieved_handicap");
            entity.Property(e => e.C)
                .HasMaxLength(1)
                .HasColumnName("c");
            entity.Property(e => e.Corrected).HasColumnName("corrected");
            entity.Property(e => e.Elapsed).HasColumnName("elapsed");
            entity.Property(e => e.FinishCode)
                .HasMaxLength(5)
                .HasColumnName("finish_code");
            entity.Property(e => e.FinishDate)
                .HasColumnType("datetime")
                .HasColumnName("finish_date");
            entity.Property(e => e.HandicapStatus)
                .HasMaxLength(2)
                .HasColumnName("handicap_status");
            entity.Property(e => e.InterimDate)
                .HasColumnType("datetime")
                .HasColumnName("interim_date");
            entity.Property(e => e.Laps).HasColumnName("laps");
            entity.Property(e => e.LastEdit)
                .HasColumnType("datetime")
                .HasColumnName("last_edit");
            entity.Property(e => e.NewRollingHandicap).HasColumnName("new_rolling_handicap");
            entity.Property(e => e.OpenHandicap).HasColumnName("open_handicap");
            entity.Property(e => e.OverridePoints).HasColumnName("override_points");
            entity.Property(e => e.PerformanceIndex).HasColumnName("performance_index");
            entity.Property(e => e.Place).HasColumnName("place");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.RestrictedSail).HasColumnName("restricted_sail");
            entity.Property(e => e.RollingHandicap).HasColumnName("rolling_handicap");
            entity.Property(e => e.StandardCorrected).HasColumnName("standard_corrected");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.Races)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("FK_races_boats");

            entity.HasOne(d => d.RidNavigation).WithMany(p => p.Races)
                .HasForeignKey(d => d.Rid)
                .HasConstraintName("FK_races_calendar");
        });

        modelBuilder.Entity<SelectRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_select_rule");

            entity.ToTable("select_rules");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Application).HasColumnName("application");
            entity.Property(e => e.Condition).HasColumnName("condition");
            entity.Property(e => e.Field)
                .HasMaxLength(255)
                .HasColumnName("field");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NumberBound1)
                .HasColumnType("numeric(18, 4)")
                .HasColumnName("number_bound1");
            entity.Property(e => e.NumberBound2)
                .HasColumnType("numeric(18, 4)")
                .HasColumnName("number_bound2");
            entity.Property(e => e.Parent).HasColumnName("parent");
            entity.Property(e => e.StringValue)
                .HasMaxLength(255)
                .HasColumnName("string_value");
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.Sid);

            entity.ToTable("series");

            entity.Property(e => e.Sid).HasColumnName("sid");
            entity.Property(e => e.Discards)
                .HasMaxLength(255)
                .HasColumnName("discards");
            entity.Property(e => e.Sname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("sname");

            entity.HasMany(d => d.Rids).WithMany(p => p.Sids)
                .UsingEntity<Dictionary<string, object>>(
                    "CalendarSeriesJoin",
                    r => r.HasOne<Calendar>().WithMany()
                        .HasForeignKey("Rid")
                        .HasConstraintName("FK_calendar_series_join_calendar"),
                    l => l.HasOne<Series>().WithMany()
                        .HasForeignKey("Sid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_calendar_series_join_series"),
                    j =>
                    {
                        j.HasKey("Sid", "Rid");
                        j.ToTable("calendar_series_join");
                        j.IndexerProperty<int>("Sid").HasColumnName("sid");
                        j.IndexerProperty<int>("Rid").HasColumnName("rid");
                    });
        });

        modelBuilder.Entity<SeriesResult>(entity =>
        {
            entity.HasKey(e => new { e.Sid, e.Division, e.Bid });

            entity.ToTable("series_results");

            entity.Property(e => e.Sid).HasColumnName("sid");
            entity.Property(e => e.Division)
                .HasMaxLength(20)
                .HasColumnName("division");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Entered).HasColumnName("entered");
            entity.Property(e => e.Gross).HasColumnName("gross");
            entity.Property(e => e.Nett).HasColumnName("nett");
            entity.Property(e => e.Place).HasColumnName("place");
        });

        modelBuilder.Entity<Update>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("updates");

            entity.Property(e => e.Dummy).HasColumnName("dummy");
            entity.Property(e => e.Upload)
                .HasColumnType("datetime")
                .HasColumnName("upload");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
