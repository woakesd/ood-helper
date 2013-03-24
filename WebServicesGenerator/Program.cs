using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using OodHelper;

namespace WebServicesGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (MySql.Data.MySqlClient.MySqlConnection _conn = new MySql.Data.MySqlClient.MySqlConnection(Settings.Mysql))
            {
                try
                {
                    _conn.Open();

                    DataTable _columns = _conn.GetSchema("Columns", new string[] { "def", "peycrace", "boats" });
                    DisplayData(_columns);

                    MySql.Data.MySqlClient.MySqlCommand _cmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT * FROM boats WHERE 1 = 0", _conn);
                    MySql.Data.MySqlClient.MySqlDataAdapter _adapt = new MySql.Data.MySqlClient.MySqlDataAdapter(_cmd);
                    DataTable _tableShema = new DataTable();
                    _adapt.Fill(_tableShema);
                    foreach (DataColumn _col in _tableShema.Columns)
                    {
                        Console.WriteLine("        [DataMember]");
                        Console.WriteLine("        public {0} {1}  {{ get; set; }}", GetDataMemberType(_col.DataType), _col.ColumnName);
                    }
                }
                finally
                {
                    _conn.Close();
                }
            }
        }

        private static string GetDataMemberType(Type DataType)
        {
            switch (DataType.ToString())
            {
                case "System.Boolean":
                    return "bool?";
                case "System.Byte":
                    return "byte?";
                case "System.Char":
                    return "char?";
                case "System.DateTime":
                    return "DateTime?";
                case "System.Decimal":
                    return "decimal?";
                case "System.Double":
                    return "double?";
                case "System.Guid":
                    return "Guid?";
                case "System.Int16":
                case "System.Int32":
                    return "int?";
                case "System.Int64":
                    return "Int64?";
                case "System.SByte":
                    return "sbyte?";
                case "System.Single":
                    return "Single?";
                case "System.String":
                    return "string";
                case "System.TimeSpan":
                    return "TimeSpan?";
                case "System.UInt16":
                case "System.UInt32":
                    return "uint?";
                case "System.UInt64":
                    return "UInt64?";
                default:
                    return "object";
            }
        }

        private static void DisplayData(System.Data.DataTable table)
        {
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    Console.WriteLine("{0} = {1}", col.ColumnName, row[col]);
                }
                Console.WriteLine("============================");
            }
        }
    }
}
