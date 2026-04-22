using Microsoft.Win32;
using OpsoleTestApp.Helpers;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace OpsoleTestApp.Services
{
    public class ExecutionService
    {
        private readonly Action<string> ui; 
        private readonly SystemInfoService sysService = new SystemInfoService(); 
        private readonly APIService apiService;
        private readonly SecurityService secService;

        private string serial = "UNKNOWN";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="output"></param>
        public ExecutionService(Action<string> output)
        {
            ui = output;

            var config = ConfigLoader.Load("Config.xml");

            if (null == config || string.IsNullOrEmpty(config.AESKey) ||
                string.IsNullOrEmpty(config.BearerToken) ||
                string.IsNullOrEmpty(config.ApiEndPoint))
            {
                SystemUtility.LogToFile($"The configuration file loading failed");
                throw new InvalidOperationException("Configuration is Invalid");
            }
            secService = new SecurityService(config.AESKey);

            // Encrypt config and store
            string raw = config.ApiEndPoint + "|" + config.BearerToken;
            secService.SaveToRegistry(secService.Encrypt(raw));

            string value = secService.ReadFromRegistry();
            string decryptedValue = secService.Decrypt(value);
            var splittedValues = decryptedValue.Split('|');

            apiService = new APIService(splittedValues[0], splittedValues[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RunAll()
        {
            string deviceName = "";
            string username = "";
            string? sid = "";
            string ds = "";

            try
            {
                await Run("Device Name", () => deviceName = sysService.DeviceName());
                await Run("Serial", () => serial = sysService.GetSerial());
                await Run("", () => ds = sysService.DsReg());
                await Run("Username", () => username = sysService.Username());
                await Run("SID", () => sid = sysService.GetSID() ?? "");

                await Run("Registry Write", () =>
                {
                    using var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\OpsoleTest\DeviceInfo");
                    key.SetValue("DeviceName", deviceName);
                    key.SetValue("Serial", serial);
                    key.SetValue("Username", username);
                    key.SetValue("SID", sid);
                    key.SetValue("JoinStatus", ds);
                    return "Registry written: OK";
                });

                await Run("Scheduled Task", () =>
                {
                    SystemUtility.CreateTask();
                    return "Task created: OK";
                });

                await Run("File + ACL", () =>
                {
                    string basePath = @"C:\OpsoleTest\UserProfile";
                    SystemUtility.CreateFiles();

                    var dirInfo = new DirectoryInfo(basePath);
                    var security = dirInfo.GetAccessControl();

                    security.AddAccessRule(new FileSystemAccessRule(
                        "Everyone",
                        FileSystemRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));

                    dirInfo.SetAccessControl(security);
                    return "File ACL: OK";
                });

                await Run("Registry ACL", () =>
                {
                    var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\OpsoleTest\UserProfile\AppSettings");
                    var security = key.GetAccessControl();

                    security.AddAccessRule(new RegistryAccessRule(
                        "Everyone",
                        RegistryRights.FullControl,
                        InheritanceFlags.ContainerInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));

                    key.SetAccessControl(security);
                    return "Registry ACL: OK";
                });

                await Run("Execution Context", () =>
                {
                    bool isSystem = WindowsIdentity.GetCurrent().IsSystem;
                    return isSystem ? "Running as SYSTEM" : "Running as " + Environment.UserName;
                });

                await Run("AES Test", () =>
                {
                    var e = secService.Encrypt("test");
                    var d = secService.Decrypt(e);
                    return d == "test" ? "Encrypt/Decrypt: OK" : "Encrypt/Decrypt: FAIL";
                });
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
            }
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="step"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private async Task Run(string step, Func<string> action)
        {
            try
            {
                var result = action();
                string msg = $"[{step}] SUCCESS - {result}";

                ui(msg);
                SystemUtility.LogEvent(msg);
                await apiService.Send(serial, msg, false);
            }
            catch (Exception ex)
            {
                string msg = $"[{step}] FAILURE - {ex.Message}";

                ui(msg);
                SystemUtility.LogEvent(msg, true);
                await apiService.Send(serial, msg, true);
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
            }
        }
    }
}
