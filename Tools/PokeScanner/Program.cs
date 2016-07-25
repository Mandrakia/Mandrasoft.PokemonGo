using Google.Common.Geometry;
using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using MandraSoft.PokemonGo.Communicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PortablePokeRadar
{
    class Program
    {
        static private bool Exit;
        static void Main(string[] args)
        {
            Task.Run(() => Execute());
            System.Console.ReadLine();
            Exit = true;
        }

        private static async Task Execute()
        {
            var client = new PokemonGoClient();
            client.MapObjectsHandler = Web.Instance.UpdateResponseToWebsite;
            bool success = false;
            while (!success)
            {
                try
                {
                    Console.WriteLine("Trying to authenticate");
                    await client.Login();
                    Console.WriteLine("Success !");
                    Console.WriteLine("Asking niantic servers which server I should connect to (The equivalent of the loading screen)");
                    await client.SetServer();
                    Console.WriteLine("Success!");
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error :(");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Retrying in 5 seconds");
                    await Task.Delay(5000);
                }
            }
            Console.WriteLine("Launching workers");
            var bounds = System.Configuration.ConfigurationManager.AppSettings["BoundsToScan"].Split(',').Select(x => double.Parse(x.Trim(), System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            var jobs = int.Parse(System.Configuration.ConfigurationManager.AppSettings["JobsToLaunch"]);
            var tasks = new List<Task>();
            for (var i = 0; i < jobs; i++)
            {
                tasks.Add(ScanAllArea(bounds[0], bounds[1], bounds[2], bounds[3], jobs, i, client));                
                
            }            
            Task.WaitAll(tasks.ToArray());
            client.Dispose();
        }

        private static async Task ScanAllArea(double lat1, double lng1, double lat2, double lng2, int splittedIn, int indexNumber, PokemonGoClient client)
        {
            while (!Exit)
            {
                try
                {
                    Console.WriteLine("Worker : " + indexNumber + " started !");
                    var region_rect = S2LatLngRect.FromPointPair(
                    S2LatLng.FromDegrees(lat1, lng1),
                    S2LatLng.FromDegrees(lat2, lng2));
                    var coverer = new S2RegionCoverer() { MaxLevel = 15, MinLevel = 15, LevelMod = 0, MaxCells = int.MaxValue };
                    var covering = new List<S2CellId>();
                    coverer.GetCovering(region_rect, covering);
                    covering = covering.OrderBy(x => x.Id).ToList();
                    covering = covering.Skip((covering.Count / splittedIn) * indexNumber).Take(covering.Count / splittedIn).ToList();
                    foreach (var cell in covering)
                    {
                        for (S2CellId c = cell.ChildBegin; c != cell.ChildEnd; c = c.Next)
                        {
                            var point2 = new S2LatLng(new S2Cell(c).Center);
                            client.SetCoordinates(point2.LatDegrees, point2.LngDegrees);
                            await client.UpdateMapObjects();
                        }
                    }
                    Console.WriteLine("Worker : " + indexNumber + " finished! Looping now.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Worker : " + indexNumber + " crashed !");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Retrying in 5 seconds");
                    await Task.Delay(5000);
                }
            }
        }
    }
}
