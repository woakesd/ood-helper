using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace OodHelper
{
    class ErrorLogger
    {
        private static string FileName = "Errors.log";
        private static object _locker = new object();
        public static void LogException(Exception ex)
        {
            lock (_locker)
            {
                using (StreamWriter sw = new StreamWriter(FileName, true))
                {
                    sw.WriteLine(string.Format("{0:yyyy-MM-ddTHH:mm:ss} \"{1}\"", new object[] { DateTime.Now, ex.Message }));

                    sw.WriteLine(ex.StackTrace);
                    sw.WriteLine();
                    sw.Close();
                }
            }
        }
    }
}
