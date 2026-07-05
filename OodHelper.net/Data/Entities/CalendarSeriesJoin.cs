using System;

namespace OodHelper.Data.Entities;

// Explicit join entity for the calendar_series_join many-to-many between Series and Calendar.
// The relationship is still exposed through the Series.Rids / Calendar.Sids skip navigations
// (configured in OodHelperContext.UsingEntity), but giving the join row a real type lets it go
// through DbSet/EF operations (e.g. ExecuteDeleteAsync, BulkInsertAsync) instead of raw SQL.
public partial class CalendarSeriesJoin
{
    public int Sid { get; set; }

    public int Rid { get; set; }
}
