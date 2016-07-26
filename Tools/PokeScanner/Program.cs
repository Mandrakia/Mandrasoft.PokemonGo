using Google.Common.Geometry;
using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using MandraSoft.PokemonGo.Api.Helpers;
using MandraSoft.PokemonGo.Api.Logging;
using MandraSoft.PokemonGo.Communicator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MandraSoft.PortablePokeRadar
{
    class Program
    {
        static private bool Exit;
        static void Main(string[] args)
        {
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));

            Console.ForegroundColor = ConsoleColor.White;
            Task.Run(() => Execute());
            Console.ReadLine();
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
                    Logger.Write("Trying to authenticate");
                    await client.Login();
                    Logger.Write("Success !");
                    Logger.Write("Asking niantic servers which server I should connect to (The equivalent of the loading screen)");
                    await client.SetServer();
                    Logger.Write("Success!");
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Write("Error :(");
                    Logger.Write(ex.Message);
                    Logger.Write("Retrying in 5 seconds");
                    await Task.Delay(5000);
                }
            }
            Logger.Write("Launching workers");
            var bounds = System.Configuration.ConfigurationManager.AppSettings["BoundsToScan"].Split(',').Select(x => double.Parse(x.Trim(), System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            var area = (S2LatLng.FromDegrees(bounds[0], bounds[1]).GetEarthDistance(S2LatLng.FromDegrees(bounds[0], bounds[3]))/1000)
                * (S2LatLng.FromDegrees(bounds[0], bounds[1]).GetEarthDistance(S2LatLng.FromDegrees(bounds[2], bounds[1]))/1000);
            
            var jobs = int.Parse(System.Configuration.ConfigurationManager.AppSettings["JobsToLaunch"]);
            areaPerJob = area / jobs;


            var region_rect = S2LatLngRect.FromPointPair(
                    S2LatLng.FromDegrees(bounds[0], bounds[1]),
                    S2LatLng.FromDegrees(bounds[2], bounds[3]));
            var coverer = new S2RegionCoverer() { MaxLevel = 16, MinLevel = 16, LevelMod = 0, MaxCells = int.MaxValue };
            var covering = new List<S2CellId>();
            coverer.GetCovering(region_rect, covering);

            //Seemed like a good idea a the time but geometry of the Cells changes depending on where you are on the Globe
            //In the end with a 100m detection radius there's no choice but to scan each cell.
            //CellsToAnalyze = S2Helper.GetListOfCellsToCheck(covering); 
            //CellsToAnalyzeOdd = covering.Where(x => !CellsToAnalyze.Any(a => a.Id == x.Id)).ToList();


            var tasks = new List<Task>();
            for (var i = 0; i < jobs; i++)
            {
                tasks.Add(ScanAllArea(jobs, i, client));                
                
            }
            sw = new Stopwatch();
            sw.Start();
            Task.WaitAll(tasks.ToArray());
            client.Dispose();
        }
        static private List<S2CellId> CellsToAnalyze;
        static private List<S2CellId> CellsToAnalyzeOdd;
        static private double areaPerJob;
        static private int counterCompleted = 0;
        static private Stopwatch sw;
        private static async Task ScanAllArea(int splittedIn, int indexNumber, PokemonGoClient client)
        {
            int failCounter = 0;
            Logger.Write("Worker : " + indexNumber + " started !");
            while (!Exit)
            {
                try
                {              
                    var tmp = CellsToAnalyze.Skip((CellsToAnalyze.Count / splittedIn) * indexNumber).Take(CellsToAnalyze.Count / splittedIn).ToList();
                    foreach (var cell in tmp)
                    {
                        //for (S2CellId c = cell.ChildBegin; c != cell.ChildEnd; c = c.Next)
                        //{
                        //    var point2 = new S2LatLng(new S2Cell(c).Center);
                        //    client.SetCoordinates(point2.LatDegrees, point2.LngDegrees);
                        //    await client.UpdateMapObjects();
                        //}
                        //Logger.Write("Max range so far : " + maxRange + "meters");
                        await client.UpdateMapObjects(cell.Center().LatDegrees, cell.Center().LngDegrees);                        
                    }                    
                    counterCompleted++;                    
                    if (counterCompleted % 10 == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Write("Total pokemons sent so far : " + Web.Instance._EncountersAlreadySent.Count);
                        Logger.Write("Average scanning speed : " + ((double)(counterCompleted * areaPerJob) / sw.ElapsedMilliseconds) * 1000 + "km²/sec");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    failCounter = 0;
                }
                catch (Exception ex)
                {
                    failCounter++;
                    //Logger.Write("Worker : " + indexNumber + " crashed !");
                    //Logger.Write(ex.Message);
                    //Logger.Write("Retrying in 5 seconds");
                    await Task.Delay(5000);
                    if (failCounter > 5)
                    {
                        Logger.Write($"Something wrong with worker {indexNumber}");
                        failCounter = 0;
                    }
                }
            }
        }
        static void PrintRow(int tableWidth,params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Logger.Write(row);
        }

        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
    }
}
