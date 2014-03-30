using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTide
{
    class Program
    {
        protected static MySqlConnection mcon;
        protected static MySqlTransaction mtrn;
        
        static void Main(string[] args)
        {
            string mysql = "server=peycrace.info;User Id=peycrace;password=5c8gbE7stjH4;database=peycrace;Use Compression=True;port=7506;Ssl Mode=VerifyFull";
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
            mysql = mcsb.ConnectionString;
            mcon = new MySqlConnection(mysql);
            mcon.Open();
            mtrn = mcon.BeginTransaction();
            
            MySqlCommand mcom = new MySqlCommand("DELETE FROM boats");
            mcom.Connection = mcon;
            mcom.ExecuteNonQuery();

            StringBuilder msql = new StringBuilder("INSERT INTO `boats` (`boatname`,`boatclass`,`sailno`,`dngy`,`h`,`bid`,");
            msql.Append("`distance`,`crewname`,`ohp`,`ohstat`,`rhp`,`csf`,`eng`,`kl`,`deviations`,`subscriptn`,`p`,`s`,");
            msql.Append("`boatmemo`,`id`,`beaten`,`berth`,`hired`,`uid`) VALUES ");

            Db c = new Db(@"SELECT boatname, boatclass, sailno, dinghy dngy, hulltype h, bid,
                    distance, crewname, open_handicap ohp, handicap_status ohstat, rolling_handicap rhp, crew_skill_factor csf, engine_propeller eng, keel kl,
                    deviations, subscription subscriptn, p, s, boatmemo, id, beaten, berth, hired, uid
                    FROM boats");
            DataTable d = c.GetData(null);
            c.Dispose();

            BuildInsertData(d, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mtrn.Commit();

            mcon.Close();
            mcon.Dispose();
        }
    }
}
