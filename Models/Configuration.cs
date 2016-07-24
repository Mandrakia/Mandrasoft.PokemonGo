using MandraSoft.PokemonGo.Models.Enums;

namespace MandraSoft.PokemonGo.Models
{
    static public class Configuration
    {
        public static int WebDelay => int.Parse(System.Configuration.ConfigurationManager.AppSettings["WebDelay"]);
        public static int DbDelay => int.Parse(System.Configuration.ConfigurationManager.AppSettings["DbDelay"]);
        public static string WebUri  => System.Configuration.ConfigurationManager.AppSettings["WebUri"];
        static public string Login => System.Configuration.ConfigurationManager.AppSettings["Login"];
        static public string Password => System.Configuration.ConfigurationManager.AppSettings["Password"];
        static public AuthType AuthType => System.Configuration.ConfigurationManager.AppSettings["AuthType"] == "PTC" ? AuthType.PTC : AuthType.Google;
    }
}
