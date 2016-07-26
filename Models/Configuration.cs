using MandraSoft.PokemonGo.Models.Enums;
using MandraSoft.PokemonGo.Models.WebModels.Responses;
using MandraSoft.PokemonGo.Models.WPFViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MandraSoft.PokemonGo.Models
{
    static public class Configuration
    {
        static public ScannerSettingsViewModel Settings { get; set; }

        static Configuration()
        {
            if (File.Exists("Settings.json"))
            {
                Settings = JsonConvert.DeserializeObject<ScannerSettingsViewModel>(File.ReadAllText("Settings.json"));
            }
            else
            {
                Settings = new ScannerSettingsViewModel()
                {
                    WebDelay = System.Configuration.ConfigurationManager.AppSettings["WebDelay"] == null ? 30 : int.Parse(System.Configuration.ConfigurationManager.AppSettings["WebDelay"]),
                    Accounts = new ObservableCollection<AccountsViewModel>() { new AccountsViewModel() { Name = "Default", Login = System.Configuration.ConfigurationManager.AppSettings["Login"], Password = System.Configuration.ConfigurationManager.AppSettings["Password"], AuthType = System.Configuration.ConfigurationManager.AppSettings["AuthType"]?.ToLowerInvariant() == "ptc" ? AuthType.PTC : AuthType.Google } },
                    ScannedAreas = System.Configuration.ConfigurationManager.AppSettings["Bounds"] == null ? new ObservableCollection<ScannedAreaViewModel>() : new ObservableCollection<ScannedAreaViewModel>() { new ScannedAreaViewModel() { AccountNames = new ObservableCollection<string>() { "None" }, Name = "None", Coordinates = ParseConfigBounds()[0] } },
                    SaveLocalDb = System.Configuration.ConfigurationManager.AppSettings["SaveLocalDb"] == null ? false : bool.Parse(System.Configuration.ConfigurationManager.AppSettings["SaveLocalDb"])
                };
            }
        }
        static private LatLngViewModel[] ParseConfigBounds()
        {
            List<LatLngViewModel> result = new List<LatLngViewModel>();
            string boundsConf = System.Configuration.ConfigurationManager.AppSettings["Bounds"];
            var splitted = boundsConf.Split(',');
            for (var i = 0; i < splitted.Length / 2; i += 2)
            {
                result.Add(new LatLngViewModel() { Latitude = double.Parse(splitted[i], System.Globalization.CultureInfo.InvariantCulture), Longitude = double.Parse(splitted[i + 1], System.Globalization.CultureInfo.InvariantCulture) });
            }
            return result.ToArray();
        }

    }
}
