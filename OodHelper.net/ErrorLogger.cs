using System;
using System.IO;
using System.Reflection;

namespace OodHelper
{
    class ErrorLogger
    {
        private static readonly string FileFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        private static readonly string FileName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + "-Errors.log";

        private static readonly object Locker = new object();

        public static void LogException(Exception ex)
        {
            lock (Locker)
            {
                using (var sw = new StreamWriter(FileFolder + Path.DirectorySeparatorChar + FileName, true))
                {
                    sw.WriteLine(@"{0:yyyy-MM-ddTHH:mm:ss} {1}", new object[] { DateTime.Now, ex.Message });
                    if (ex.InnerException != null)
                    {
                        sw.WriteLine(@"{0:yyyy-MM-ddTHH:mm:ss} {1}", new object[] { DateTime.Now, ex.InnerException.Message });
                    }
                    sw.WriteLine(ex.StackTrace);
                    sw.WriteLine();
                    sw.Close();
                }
            }
        }
    }
}
