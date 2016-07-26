using MandraSoft.PokemonGo.Api.Extensions;
using MandraSoft.PokemonGo.Api.Logging;
using MandraSoft.PokemonGo.Models.PocoProtos;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGoMapImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));

            Task.Run(() => Execute(args));
            Console.ReadKey();
        }
        static async Task Execute(string[] args)
        {
            List<WildPokemonPoco> pokemons = new List<WildPokemonPoco>();
            var pathToFile = args[0];
            Logger.Write($"Opening database : {pathToFile}");
            using (var conn = new SQLiteConnection($"Data Source={pathToFile}))
            {
                conn.Open();
                Logger.Write("Database opened");
                using (var cmd = conn.CreateCommand())
                {
                    Logger.Write("Retrieving the datas");
                    cmd.CommandText = "SELECT * FROM pokemon";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var enc = (string)reader["encounter_id"];
                            var encId = ulong.Parse(System.Text.UTF8Encoding.UTF8.GetString(Convert.FromBase64String(enc)));
                            var spawnId = (string)reader["spawnpoint_id"];
                            var pokemonId = (int)(long)reader["pokemon_id"];
                            var latitude = (double)reader["latitude"];
                            var longitude = (double)reader["longitude"];
                            var disappear = (DateTime)reader["disappear_time"];
                            pokemons.Add(new WildPokemonPoco() { EncounterId = encId, SpawnPointId = spawnId, Latitude = latitude, Longitude = longitude, PokemonData = new PokemonDataPoco() { PokemonId = (PokemonId)pokemonId }, LastModifiedTimestampMs = disappear.ToUnixTime(), TimeTillHiddenMs = 0 });
                        }
                    }
                    Logger.Write($"Data retrieved : {pokemons.Count} pokemons to export");
                }
            }
            Logger.Write("Sending data to the server");

            var amountToImport = 600;
            var total = 0;
            var index = 1;
            var max = pokemons.Count;
            int batches = (max / amountToImport) +( max % amountToImport == 0 ? 0 : 1);
            while (pokemons.Count > amountToImport)
            {
                Logger.Write($"Starting batch {index}/{batches}");
                var pokeToSync = pokemons.Take(amountToImport);
                var res = await PokemonGo.Communicator.Web.Instance.UpdateResponseToWebsite(pokeToSync.ToList());
                Logger.Write($"Successfully imported: {amountToImport} pokemons in  {res.SpawnPoints.Count} spawn points.");
                pokemons = pokemons.Skip(amountToImport).ToList();
                index++;
                total += amountToImport;
            }

            if (pokemons.Count > 0)
            {
                Logger.Write($"Starting batch {index}/{batches}");
                var res = await PokemonGo.Communicator.Web.Instance.UpdateResponseToWebsite(pokemons);
                Logger.Write($"Successfully imported: {pokemons.Count} pokemons in  {res.SpawnPoints.Count} spawn points.");
                total += pokemons.Count;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Logger.Write($"All done! Imported a total of {total} pokemons!" );
        }
    }
}
