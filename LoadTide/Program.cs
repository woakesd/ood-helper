using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using MySql.Data.MySqlClient;

namespace LoadTide
{
    internal class Program
    {
        protected static MySqlConnection Mcon;

        private static void Main()
        {
            var tide = new DataTable();

            string mysql = "server=peycrace.info;User Id=peycrace;" +
                           "password=5c8gbE7stjH4;database=peycrace;Use Compression=True;" +
                           "port=7506;Ssl Mode=VerifyFull";
            var mcsb = new MySqlConnectionStringBuilder(mysql);
            mysql = mcsb.ConnectionString;
            Mcon = new MySqlConnection(mysql);
            Mcon.Open();
            //mtrn = mcon.BeginTransaction();

            var conb = new SqlConnectionStringBuilder {DataSource = "(localdb)\\v11.0", InitialCatalog = "raceresults"};
            var scon = new SqlConnection(conb.ConnectionString);
            scon.Open();
            SqlTransaction stran = scon.BeginTransaction();

            //SqlCommand scom = new SqlCommand(@"SELECT date, height, [current] FROM tidedata WHERE date >= @start AND date < @end");
            //scom.Connection = scon;
            //scom.Parameters.AddWithValue("start", new DateTime(2014, 1, 1));
            //scom.Parameters.AddWithValue("end", new DateTime(2015, 1, 1));
            //SqlDataAdapter sdt = new SqlDataAdapter(scom);
            //sdt.Fill(Tide);
            var msel = new MySqlCommand("SELECT `date`, `height`, `current` FROM `tidedata` WHERE date >= '2015-01-01'")
            {
                Connection = Mcon
            };
            var mdt = new MySqlDataAdapter(msel);
            mdt.Fill(tide);

            //scon.Close();
            //scon.Dispose();

            var scom = new SqlCommand
            {
                Connection = scon,
                Transaction = stran,
                CommandText = "DELETE FROM [tidedata] WHERE [date] >= '01 Jan 2015'"
            };

            scom.ExecuteNonQuery();

            DataTable transition = tide.Clone();

            var blocks = (int) Math.Ceiling(tide.Rows.Count/1000.0);

            for (int i = 0; i < blocks; i++)
            {
                transition.Rows.Clear();

                for (int j = i*1000; j < Math.Min((i + 1)*1000, tide.Rows.Count); j++)
                    transition.ImportRow(tide.Rows[j]);

                var msql = new StringBuilder("INSERT INTO [tidedata] ([date],[height],[current]) VALUES ");

                BuildInsertData(transition, msql);

                scom.CommandText = msql.ToString();
                scom.ExecuteNonQuery();
            }

            stran.Commit();

            scon.Close();
            scon.Dispose();
        }

        protected static void BuildInsertData(DataTable d, StringBuilder msql)
        {
            for (int i = 0; i < d.Rows.Count; i++)
            {
                DataRow dr = d.Rows[i];

                msql.Append("(");
                for (int j = 0; j < d.Columns.Count; j++)
                {
                    string colType = d.Columns[j].DataType.ToString();
                    switch (colType)
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
                            if (dr[j] != DBNull.Value && !Double.IsNaN((double) dr[j]))
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
                            else if ((bool) dr[j])
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