using System;

namespace OodHelper.Results.Model
{
    public interface IEntry
    {
        string a { get; set; }
        int? achieved_handicap { get; set; }
        int bid { get; set; }
        string boatclass { get; set; }
        string boatname { get; set; }
        string c { get; set; }
        double? corrected { get; set; }
        int? elapsed { get; set; }
        string finish_code { get; set; }
        DateTime? finish_date { get; set; }
        string handicap_status { get; set; }
        DateTime? interim_date { get; set; }
        int? laps { get; set; }
        int? new_rolling_handicap { get; set; }
        int? open_handicap { get; set; }
        double? override_points { get; set; }
        int? place { get; set; }
        double? points { get; set; }
        int rid { get; set; }
        int? rolling_handicap { get; set; }
        string sailno { get; set; }
        double? standard_corrected { get; set; }
        DateTime? start_date { get; set; }
        int? performance_index { get; set; }
        void SaveChanges();
    }
}
