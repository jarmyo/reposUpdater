using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace ReposUpdate
{
    public static class Logger
    {
        public static void Write(string logMessage)
        {
            Write(logMessage, true);
        }
        public static void Write(string logMessage, bool showTimer)
        {
            if (!Started)
            {
                timer = Stopwatch.StartNew();
                Started = true;
                Write(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                Write("========================================");
            }

            var timerText = showTimer ? timer.Elapsed.ToString() + " : " : string.Empty;
            var completeString = timerText + logMessage;

            Debug.WriteLine(completeString);
            using (StreamWriter w = File.AppendText(Common.PathLogs + "Update.txt"))
            {
                w.WriteLine(completeString);
            }
        }

        private static Stopwatch timer;
        private static bool Started;
    }
}