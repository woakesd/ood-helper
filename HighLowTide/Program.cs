using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace HighLowTide
{
    class Program
    {
        static void Main(string[] args)
        {
            MySqlConnectionStringBuilder msb = new MySqlConnectionStringBuilder();
            msb.Password = "";
            msb.UserID = "peycrace";
            msb.Server = "imap.mitredata.co.uk";
            msb.SslMode = MySqlSslMode.Required;
            MySqlConnection mcon = new MySqlConnection();

            MySqlCommand mcom = new MySqlCommand(@"select *, (a.current + b.current + c.current + d.current + e.current)/5
from tidedata a
inner join tidedata b
on b.date = date_add(a.date, interval -10 minute)
inner join tidedata c
on c.date = date_add(a.date, interval 10 minute)
inner join tidedata d
on d.date = date_add(a.date, interval -20 minute)
inner join tidedata e
on e.date = date_add(a.date, interval 20 minute)
where a.date >= '2012-01-01'
and a.height >= b.height
and a.height >= c.height
and a.height >= d.height
and a.height >= e.height

union all

select *, (a.current + b.current + c.current + d.current + e.current)/5
from tidedata a
inner join tidedata b
on b.date = date_add(a.date, interval -10 minute)
inner join tidedata c
on c.date = date_add(a.date, interval 10 minute)
inner join tidedata d
on d.date = date_add(a.date, interval -20 minute)
inner join tidedata e
on e.date = date_add(a.date, interval 20 minute)
where a.date >= '2012-01-01'
and a.height <= b.height
and a.height <= c.height
and a.height <= d.height
and a.height <= e.height

order by 1", mcon);

            MySqlDataAdapter mad = new MySqlDataAdapter(mcom);

            DataTable d = new DataTable();
            mad.Fill(d);

            for (int i = 0; i < d.Rows.Count; i++)
            {

            }
        }
    }
}
