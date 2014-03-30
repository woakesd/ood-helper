using MySql.Data.MySqlClient;
using OodHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            
            string mysql = "server=peycrace.info;User Id=peycrace;" +
                "password=5c8gbE7stjH4;database=peycrace;Use Compression=True;" +
                "port=7506;Ssl Mode=VerifyFull";
            MySqlConnectionStringBuilder mcsb = new MySqlConnectionStringBuilder(mysql);
            mysql = mcsb.ConnectionString;
            mcon = new MySqlConnection(mysql);
            mcon.Open();
            mtrn = mcon.BeginTransaction();

            SqlConnectionStringBuilder conb = new SqlConnectionStringBuilder();
            conb.DataSource = "(localdb)\\v11.0";
            conb.InitialCatalog = "raceresults";
            SqlConnection scon = new SqlConnection(conb.ConnectionString);
            scon.Open();

            SqlCommand scom = new SqlCommand(@"SELECT date, height, [current] FROM tidedata WHERE date >= @start AND date < @end");
            scom.Connection = scon;
            scom.Parameters.Add("start", new DateTime(2014, 1, 1));
            scom.Parameters.Add("end", new DateTime(2015, 1, 1));
            SqlDataAdapter sdt = new SqlDataAdapter(scom);
            DataTable Tide = new DataTable();
            sdt.Fill(Tide);

            scon.Close();
            scon.Dispose();

            MySqlCommand mcom = new MySqlCommand();
            mcom.Connection = mcon;

            mcom.CommandText = "DELETE FROM `tidedata` WHERE date >= @sdate AND date <= @edate";
            mcom.Parameters.AddWithValue("sdate", Tide.Rows[0]["date"]);
            mcom.Parameters.AddWithValue("edate", Tide.Rows[Tide.Rows.Count - 1]["date"]);

            mcom.ExecuteNonQuery();

            StringBuilder msql = new StringBuilder("INSERT INTO `tidedata` (`date`,`height`,`current`) VALUES ");

            BuildInsertData(Tide, msql);

            mcom.CommandText = msql.ToString();
            mcom.ExecuteNonQuery();

            mtrn.Commit();

            mcon.Close();
            mcon.Dispose();
        }

        protected static void BuildInsertData(DataTable d, StringBuilder msql)
        {
            for (int i = 0; i < d.Rows.Count; i++)
            {
                DataRow dr = d.Rows[i];

                msql.Append("(");
                for (int j = 0; j < d.Columns.Count; j++)
                {
                    string _colType = d.Columns[j].DataType.ToString();
                    switch (_colType)
                    {
                        case "System.String":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("'{0}'", dr[j].ToString().Replace("'", "''"));
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Int32":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("{0}", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Double":
                            if (dr[j] != DBNull.Value && !Double.IsNaN((double)dr[j]))
                                msql.AppendFormat("{0}", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.DateTime":
                            if (dr[j] != DBNull.Value)
                                msql.AppendFormat("'{0:yyyy-MM-dd HH:mm:ss}'", dr[j]);
                            else
                                msql.Append("NULL");
                            break;
                        case "System.Boolean":
                            if (dr[j] == DBNull.Value)
                                msql.Append("NULL");
                            else if ((bool)dr[j])
                                msql.Append("1");
                            else
                                msql.Append("0");
                            break;
                        case "System.Decimal":
                            if (dr[j] == DBNull.Value)
                                msql.Append("NULL");
                            else
                                msql.AppendFormat("{0}", dr[j]);
                            break;
                        case "System.Guid":
                            if (dr[j] == DBNull.Value)
                                msql.Append("NULL");
                            else
                                msql.AppendFormat("'{{{0}}}'", dr[j]);
                            break;
                    }
                    if (j < d.Columns.Count - 1) msql.Append(",");
                }
                msql.Append(")");
                if (i < d.Rows.Count - 1) msql.Append(",");
            }
        }
    }

}
