using System.Collections;
using System.ComponentModel;

namespace OodHelper.Results
{
    class SternChaseScorer : IRaceScore
    {
        //private Db _racedb;
        //private System.Data.DataTable _racedata;

        public double StandardCorrectedTime
        {
            get { return 0.0; }
        }

        public void Calculate(int rid)
        {
            try
            {
                var p = new Hashtable();
                p["rid"] = rid;
                _racedb = new Db(@"SELECT * FROM races WHERE rid = @rid");
                _racedata = _racedb.GetData(p);

                var c = new Db(@"UPDATE calendar
                        SET result_calculated = GETDATE(),
                        raced = 1
                        WHERE rid = @rid");
                c.ExecuteNonQuery(p);
            }
            catch { }
        }

        //BackgroundWorker _back = null;

        public void Calculate(object sender, DoWorkEventArgs e)
        {
            //_back = sender as BackgroundWorker;
            Calculate((int)e.Argument);
        }

        public int Finishers
        {
            get
            {
                return _racedata != null ? _racedata.Rows.Count : 0;
            }
        }

        public bool Calculated { get; private set; }
    }
}
