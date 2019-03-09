using System;
using System.IO;
using System.Reflection;

namespace TrackFolderChange.Support
{
    public class LogWriter
    {
        private string _logFilePath;

        public LogWriter(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Write(string logMessage)
        {
            try
            {
                using (var w = File.AppendText(_logFilePath))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception)
            {
                // Ignored.
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception)
            {
                // Ignored.
            }
        }
    }
}
