using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OodHelper.net
{
    [Svn("$Id$")]
    public class RaceSeriesResult
    {
        //
        // Series date indexed by class
        //
        public Dictionary<string, SeriesResult> SeriesResults;
        public RaceSeriesResult(int SeriesId, Delegate d)
        {
            Working w = new Working(App.Current.MainWindow);

            Task t = Task.Factory.StartNew(() =>
            {
                Hashtable p = new Hashtable();
                p["sid"] = SeriesId;
                Db c = new Db(@"SELECT sname FROM series WHERE sid = @sid");

                string SeriesName = c.GetScalar(p) as string;

                c = new Db(@"SELECT c.event, c.class, c.handicapping, c.rid, c.start_date, c.result_calculated
                    FROM calendar c 
                    INNER JOIN calendar_series_join j ON c.rid = j.rid
                    INNER JOIN races r on r.rid = c.rid
                    WHERE j.sid = @sid
                    AND c.raced = 1 
                    GROUP BY c.event, c.class, c.handicapping, c.rid, c.start_date, c.result_calculated
                    ORDER BY c.start_date");

                DataTable races = c.GetData(p);

                w.SetRange(0, races.Rows.Count);

                foreach (DataRow race in races.Rows)
                {
                    w.SetProgress("Calculating " + race["event"] + " - " + race["class"], races.Rows.IndexOf(race));
                    IRaceScore scorer;
                    switch (race["handicapping"].ToString().ToUpper())
                    {
                        case "R":
                            scorer = new RollingHandicap();
                            break;
                        default:
                            scorer = new OpenHandicap();
                            break;
                    }
                    scorer.Calculate((int)race["rid"]);
                }

                c = new Db(@"SELECT c.class, r.rid, c.start_date, bid, points, override_points, finish_code
                    FROM races r
                    LEFT JOIN calendar_series_join j ON r.rid = j.rid
                    LEFT JOIN calendar c ON c.rid = j.rid
                    WHERE j.sid = @sid
                    AND raced = 1
                    AND (finish_code IS NOT NULL OR finish_date IS NOT NULL)
                    ORDER BY c.class");

                DataTable rd = c.GetData(p);

                Dictionary<string, Dictionary<int, SeriesEvent>> SeriesData = new Dictionary<string, Dictionary<int, SeriesEvent>>();

                foreach (DataRow re in rd.Rows)
                {
                    if (!SeriesData.ContainsKey(re["class"].ToString()))
                    {
                        SeriesData.Add(re["class"].ToString(), new Dictionary<int, SeriesEvent>());
                    }
                    Dictionary<int, SeriesEvent> sData = SeriesData[re["class"].ToString()];
                    if (!sData.ContainsKey((int)re["rid"]))
                        sData[(int)re["rid"]] = new SeriesEvent((int)re["rid"], (DateTime)re["start_date"]);
                    SeriesEvent Event = sData[(int)re["rid"]];
                    SeriesEntry se = new SeriesEntry();
                    se.bid = (int)re["bid"];
                    se.rid = Event.Rid;
                    se.code = re["finish_code"].ToString().ToUpper();
                    se.date = (DateTime)re["start_date"];
                    if (re["points"] != DBNull.Value)
                        se.points = (double)re["points"];
                    if (re["override_points"] != DBNull.Value)
                        se.override_points = (double)re["override_points"];
                    Event.AddEntry(se);
                }

                SeriesResults = new Dictionary<string, SeriesResult>();
                foreach (string className in SeriesData.Keys)
                {
                    ArrayList rem = new ArrayList();
                    foreach (int k in SeriesData[className].Keys)
                    {
                        if (SeriesData[className][k].NumberOfFinishers == 0)
                            rem.Add(k);
                    }
                    foreach (int k in rem)
                        SeriesData[className].Remove(k);

                    string defaultDiscards = (string)DbSettings.GetSetting(DbSettings.settDefaultDiscardProfile);
                    if (defaultDiscards == string.Empty || defaultDiscards == null)
                        defaultDiscards = "0,1";

                    string[] DiscardParts = defaultDiscards.Split(new char[] { ',' });
                    int[] discardProfile = new int[DiscardParts.Length];
                    try
                    {
                        for (int s = 0; s < DiscardParts.Length; s++)
                            discardProfile[s] = Int32.Parse(DiscardParts[s]);
                    }
                    catch
                    {
                        discardProfile = new int[] { 0, 1 };
                    }

                    SeriesResult sr = new SeriesResult(SeriesData[className], discardProfile);
                    w.SetProgress("Calculating series " + className, races.Rows.Count);
                    sr.Score();
                    sr.SeriesName = SeriesName + " - " + className;
                    SeriesResults.Add(className, sr);
                }
                w.CloseWindow();
                w.Dispatcher.Invoke(d, null);
            });
        }
    }
}
