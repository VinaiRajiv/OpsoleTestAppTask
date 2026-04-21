using System.IO;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace OpsoleTestApp.Helpers
{
    public class AppConfig
    {
        /// <summary>
        /// API end point
        /// </summary>
        public string? ApiEndPoint { get; set; }

        /// <summary>
        /// Bearer Token value
        /// </summary>
        public string? BearerToken { get; set; }

        /// <summary>
        /// AESKey value
        /// </summary>
        public string? AESKey { get; set; }
    }

    public static class ConfigLoader
    {
        /// <summary>
        /// To load the configuration values from config.xml file.
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns></returns>
        public static AppConfig? Load(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    SystemUtility.LogToFile($"Configuration file ({path}) doesnot exist");
                    return null;
                }

                var configValues = XElement.Load(path);

                return new AppConfig
                {
                    ApiEndPoint = configValues.Element("ApiEndPoint")?.Value,
                    BearerToken = configValues.Element("BearerToken")?.Value,
                    AESKey = configValues.Element("AESKey")?.Value
                };
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
                return null;
            }
        }
    }
}
