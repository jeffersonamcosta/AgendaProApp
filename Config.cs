using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgendaProApp
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; }
        public string BearerToken { get; set; }
    }

    public static class Config
    {
        public static ApiSettings Settings { get; private set; }

        public static void Load()
        {
            var json = File.ReadAllText("appsettings.json", Encoding.UTF8);
            var doc = JsonDocument.Parse(json);
            var apiSettings = doc.RootElement.GetProperty("ApiSettings");

            Settings = new ApiSettings
            {
                BaseUrl = apiSettings.GetProperty("BaseUrl").GetString(),
                BearerToken = apiSettings.GetProperty("BearerToken").GetString()
            };
        }

        public static void Save()
        {
            var json = JsonSerializer.Serialize(new { ApiSettings = Settings }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", json);
        }
    }
}
