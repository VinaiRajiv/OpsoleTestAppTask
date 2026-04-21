using System.Diagnostics;
using System.IO;

namespace OpsoleTestApp.Helpers
{
    public class SystemUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="isError"></param>
        public static void LogEvent(string msg, bool isError = false)
        {
            try
            {
                string source = "OpsoleTest";
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }

                EventLog.WriteEntry(source, msg, isError ? EventLogEntryType.Error :
                                                           EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CreateFiles()
        {
            string basePath = @"C:\OpsoleTest\UserProfile";

            try
            {
                Directory.CreateDirectory(basePath);
                Directory.CreateDirectory(basePath + "\\AppData");
                Directory.CreateDirectory(basePath + "\\roamingdata");

                File.WriteAllText(basePath + "\\AppData\\apptest.txt", "test");
                File.WriteAllText(basePath + "\\roamingdata\\roaming.txt", "test");
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CreateTask()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = "/create /sc onlogon /tn OpsoleTest /tr cmd.exe /rl highest /f",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void LogToFile(String message)
        {
            File.AppendAllText("log.txt", $"{DateTime.Now}: {message} {Environment.NewLine}");
        }
    }
}
