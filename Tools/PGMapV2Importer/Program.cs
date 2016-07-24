using MandraSoft.PokemonGo.Api.Extensions;
using MandraSoft.PokemonGo.Models.PocoProtos;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGoMapImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => Execute(args));
            Console.ReadKey();
        }
        static async Task Execute(string[] args)
        {
            List<WildPokemonPoco> pokemons = new List<WildPokemonPoco>();
            var pathToFile = args[0];
            Console.WriteLine("Opening database : " + pathToFile);
            using (var conn = new SQLiteConnection("Data Source=" + pathToFile))
            {
                conn.Open();
                Console.WriteLine("Database opened");
                using (var cmd = conn.CreateCommand())
                {
                    Console.WriteLine("Retrieving the datas");
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
                    Console.WriteLine("Data retrieved : " + pokemons.Count + " pokemons to export");
                }
            }
            Console.WriteLine("Sending data to the server");
            var total = 0;
            var index = 1;
            var max = pokemons.Count;
            int batches = (max / 600) +( max % 600 == 0 ? 0 : 1);
            while (pokemons.Count > 600)
            {
                Console.WriteLine("Starting batch " + index +"/" + batches);
                var pokeToSync = pokemons.Take(600);
                var res = await PokemonGo.Communicator.Web.Instance.UpdateResponseToWebsite(pokeToSync.ToList());                
                Console.WriteLine("Successfully imported :" + 600 + "pokemons in " + res.SpawnPoints.Count + " spawn points.");
                pokemons = pokemons.Skip(600).ToList();
                index++;
                total += 600;
            }
            if (pokemons.Count > 0)
            {
                Console.WriteLine("Starting batch " + index + "/" + batches);
                var res = await PokemonGo.Communicator.Web.Instance.UpdateResponseToWebsite(pokemons);
                Console.WriteLine("Successfully imported : "+ pokemons.Count + " pokemons in " + res.SpawnPoints.Count + " spawn points.");
                total += pokemons.Count;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All done! Imported a total of " + total + " pokemons!" );
        }
    }
}
