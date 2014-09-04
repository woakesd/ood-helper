using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace OodHelper.Results
{
    class SternChaseScorer : IRaceScore
    {
        private Db racedb;
        private System.Data.DataTable racedata;

        public double StandardCorrectedTime
        {
            get { return 0.0; }
        }

        public void Calculate(int rid)
        {
            try
            {
                Hashtable p = new Hashtable();
                p["rid"] = rid;
                racedb = new Db(@"SELECT * FROM races WHERE rid = @rid");
                racedata = racedb.GetData(p);

                Db c = new Db(@"UPDATE calendar
                        SET result_calculated = GETDATE(),
                        raced = 1
                        WHERE rid = @rid");
                c.ExecuteNonQuery(p);
            }
            catch { }
        }

        BackgroundWorker back = null;

        public void Calculate(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            back = sender as BackgroundWorker;
            Calculate((int)e.Argument);
        }

        public int Finishers
        {
            get 
            {
                if (racedata != null)
                    return racedata.Rows.Count;
                return 0;
            }
        }

        public bool Calculated { get; private set; }
    }
}
