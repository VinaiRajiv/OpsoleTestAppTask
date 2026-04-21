using OpsoleTestApp.Helpers;
using System.Diagnostics;
using System.Management;
using System.Security.Principal;

namespace OpsoleTestApp.Services
{
    public class SystemInfoService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DeviceName() => Environment.MachineName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Username() => Environment.UserName;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string? GetSID() => WindowsIdentity.GetCurrent()?.User?.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetSerial()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
                foreach (var obj in searcher.Get())
                {
                    return obj["SerialNumber"]?.ToString() ?? "";
                }

                return "Unknown";
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DsReg()
        {
            try
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/c dsregcmd /status",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                string azure = output.Contains("AzureAdJoined : YES") ? "YES" : "NO";
                string domain = output.Contains("DomainJoined : YES") ? "YES" : "NO";

                return $"AzureAdJoined: {azure}, DomainJoined: {domain}";
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
