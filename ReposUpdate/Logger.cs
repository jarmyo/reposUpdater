using System;
using System.Diagnostics;
using System.IO;

namespace ReposUpdate
{
    public class Logger
    {
        public static void Write(string logMessage, bool showTimer = true)
        {
            if (!Started)
            {
                timer = Stopwatch.StartNew();
                Started = true;
                Write(DateTime.Now.ToString());
                Write("========================================");
            }

            var timerText = showTimer ? timer.Elapsed.ToString() + " : " : string.Empty;
            var completeString = timerText + logMessage;

            Debug.WriteLine(completeString);
            using (StreamWriter w = File.AppendText(Common.Path_Logs + "Update.txt"))
            {
                w.WriteLine(completeString);
            }
        }

        public static Stopwatch timer;
        private static bool Started = false;
    }
}