using Google.Common.Geometry;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Networking.Responses;
using POGOProtos.Inventory;
using MandraSoft.PokemonGo.Api.Extensions;
using POGOProtos.Enums;
using POGOProtos.Data.Capture;
using MandraSoft.PokemonGoApi.ConsoleTest.Models;
using MandraSoft.PokemonGo.Api;
using MandraSoft.PokemonGo.Models.Enums;
using MandraSoft.PokemonGo.Api.Logging;

namespace MandraSoft.PokemonGoApi.ConsoleTest
{
    class Program
    {
        static private List<Tuple<PokemonId, PokemonFamilyId, int>> XpFarmingPokemons = new List<Tuple<PokemonId, PokemonFamilyId, int>>() {
                    new Tuple<PokemonId, PokemonFamilyId,int>(PokemonId.Pidgey,PokemonFamilyId.FamilyPidgey,12),
                    new Tuple<PokemonId, PokemonFamilyId,int>(PokemonId.Rattata, PokemonFamilyId.FamilyRattata,25),
                    new Tuple<PokemonId, PokemonFamilyId,int>(PokemonId.Spearow, PokemonFamilyId.FamilySpearow,50),
                    new Tuple<PokemonId, PokemonFamilyId,int>(PokemonId.Zubat, PokemonFamilyId.FamilyZubat,50) };
        static private Dictionary<ItemId, int> MinimumItems = new Dictionary<ItemId, int>()
        {
            { ItemId.ItemHyperPotion, -1 },
            { ItemId.ItemPotion, -1 },
            { ItemId.ItemSuperPotion, -1 },
            {ItemId.ItemMaxPotion, -1 },
            {ItemId.ItemRevive, -1 },
            {ItemId.ItemMaxRevive,-1 },
            {ItemId.ItemPokeBall, 30 }
        };

        static void Main(string[] args)
        {
            Logger.SetLogger(new ConsoleLogger(LogLevel.Trace));
            Task.Run(() => Execute());
            Console.ReadLine();
        }

        static async void Execute()
        {
            while (true)
            {
                var client = new PokemonGoClient(48.8441589993527, 2.36343582639852);
                try
                {          
                                           
                    await client.Login();
                    await client.SetServer();
                    await client.UpdateInventory();             
                    while (true)
                        await FarmUnknownPokemons(client);
                }
                catch (Exception ex)
                {
                    await Task.Delay(15000);
                }
            }
        }
        static public async Task TestHugeCellIds(PokemonGoClient client)
        {
            var region_rect = S2LatLngRect.FromPointPair(
                S2LatLng.FromDegrees(48.91598573367739, 2.540661974689442),
                S2LatLng.FromDegrees(48.80734476255536, 2.2139903743476452));
            var coverer = new S2RegionCoverer() { MaxLevel = 15, MinLevel = 15, LevelMod = 0, MaxCells = int.MaxValue };
            var covering = new List<S2CellId>();
            coverer.GetCovering(region_rect, covering);
            covering = covering.OrderBy(x => x.Id).ToList();
            while (covering.Count >= 61)
            {
                var cellIds = covering.Take(61).ToList();
                var origin = cellIds[30].ToLatLng();
                await client.UpdateMapObjects(origin.LatDegrees, origin.LngDegrees,cellIds.Select(x => x.Id).ToList());
                covering.RemoveRange(0, 61);
            }
            if (covering.Count % 2 != 1)
                covering.Add(covering.Last().Next);
            var origin2 = covering[(covering.Count - 1) / 2].ToLatLng();
            await client.UpdateMapObjects(origin2.LatDegrees, origin2.LngDegrees,covering.Select(x => x.Id).ToList());

        }
        private static async Task ExecuteFarmingPokestops(PokemonGoClient client,int maxToDo = int.MaxValue)
        {
            await ClearUnwatedItems(client);
            await client.UpdateMapObjects();
            var pokeStops = client.MapManager.AvailablePokestops.Take(maxToDo).ToList();
            var orig = S2LatLng.FromDegrees(client.Latitude, client.Longitude);

            while (pokeStops.Any())
            {
                orig = S2LatLng.FromDegrees(client.Latitude, client.Longitude);
                var pokeStop = pokeStops.OrderBy(x => S2LatLng.FromDegrees(x.Latitude, x.Longitude).GetDistance(orig)).First();
                await client.WalkTo(pokeStop.Latitude, pokeStop.Longitude, TravelingSpeed.Bicycle, CatchAllPokemonAround);
                var fortInfo = await client.GetFortDetailsResponse(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                var fortSearch = await client.GetFortSearchResponse(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                var bag = fortSearch;
                Logger.Write($"Farmed XP: {bag.ExperienceAwarded}, Gems: { bag.GemsAwarded}, Eggs: {bag.PokemonDataEgg} Items: {GetFriendlyItemsString(bag.ItemsAwarded)}");
                if (bag.ExperienceAwarded == 0)
                {
                    await Task.Delay(4000);
                    return;
                }                
                await Task.Delay(2000);
                await CatchAllPokemonAround(client);
                pokeStops.Remove(pokeStop);
            }
            //await FarmUnknownPokemons(client);
        }
        private static async Task FarmUnknownPokemons(PokemonGoClient client)
        {
            await client.UpdateMapObjects();
            var listPokemonsIHave = client.InventoryManager.Items.Where(x => x.InventoryItemData.PokemonData != null && x.InventoryItemData.PokemonData.PokemonId != PokemonId.Missingno).Select(x => (int)x.InventoryItemData.PokemonData.PokemonId).Distinct().ToList();
            using (var comm = new RadarCommunicator())
            {
                var query = new PokemonSpawnQuery()
                {
                    pokemonIds = listPokemonsIHave,
                    neLat = 48.913363085893984,
                    neLng = 2.5323939787726886,
                    swLat = 48.80471641583385,
                    swLng = 2.2057223784308917
                };
                var pokemons = await comm.GetUnknownPokemonsForArea(query);
                //while (pokemons.Any(x => x.ExpirationTime > DateTime.UtcNow))
                //{
                //    var orig = S2LatLng.FromDegrees(client.Latitude, client.Longitude);
                //    var pokemon = pokemons.OrderBy(x => S2LatLng.FromDegrees(x.Latitude.Value, x.Longitude.Value).GetDistance(orig)).First();                    
                //    //Reachable...

                //    await client.GetPlayerUpdateResponse(pokemon.Latitude.Value, pokemon.Longitude.Value);
                //    await client.UpdateMapObjects();                        
                //    var encounter = await client.GetEncounterResponse(pokemon.EncounterId, pokemon.SpawnPointId);
                //    if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                //    {
                //        var result = await CatchPokemon(client, pokemon.EncounterId, pokemon.SpawnPointId, encounter.CaptureProbability ,(PokemonId)pokemon.PokedexNumber);
                //        if (result)
                //        {
                //            Console.ForegroundColor = ConsoleColor.Green;
                //            Logger.Write("Caught an unknown Pokemon!!!");
                //            Console.ResetColor();
                //        }
                //    }                       
                //    pokemons.Remove(pokemon);
                //}
                while (pokemons.Any(x => x.ExpirationTime > DateTime.UtcNow))
                {
                    var orig = S2LatLng.FromDegrees(client.Latitude, client.Longitude);
                    var pokemon = pokemons.OrderBy(x => S2LatLng.FromDegrees(x.Latitude.Value, x.Longitude.Value).GetDistance(orig)).First();
                    var walkingDuration = client.GetWalkingDuration(pokemon.Latitude.Value, pokemon.Longitude.Value,TravelingSpeed.Car);
                    //Reachable...
                    if (DateTime.UtcNow.Add(walkingDuration) < pokemon.ExpirationTime)
                    {
                        await client.WalkTo(pokemon.Latitude.Value, pokemon.Longitude.Value,TravelingSpeed.Car,CatchAllPokemonAround);
                        await client.UpdateMapObjects();
                        if (client.MapManager.GetNearbyCells(client.Latitude,client.Longitude).SelectMany(x => x.CatchablePokemons).Any(x => x.EncounterId == pokemon.EncounterId && x.SpawnPointId == pokemon.SpawnPointId))
                        {
                            var encounter = await client.GetEncounterResponse(pokemon.EncounterId, pokemon.SpawnPointId);
                            if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                            {
                                var result = await CatchPokemon(client, pokemon.EncounterId, pokemon.SpawnPointId, encounter.CaptureProbability, (PokemonId)pokemon.EncounterId);
                                if (result)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Logger.Write("Caught an unknown Pokemon!!!");
                                    Console.ResetColor();
                                }
                            }
                        }
                    }
                    pokemons.Remove(pokemon);
                }
                await ExecuteFarmingPokestops(client, 30);
            }
        }
        private static async Task ClearUnwatedItems(PokemonGoClient client)
        {
            await client.UpdateInventory();
            foreach (var kv in MinimumItems)
            {
                var itemId = kv.Key;
                var qty = client.InventoryManager.Items.Where(a => a.InventoryItemData.Item?.ItemId == itemId).Select(x => x.InventoryItemData.Item.Count).SingleOrDefault();
                if (qty > 0)
                {
                    var qtyToRemove = kv.Value == -1 ? qty : qty - kv.Value;
                    if (qtyToRemove > 0)
                    {
                        var deleteResponse = await client.GetRecycleInventoryItemResponse(itemId, qtyToRemove);
                        await Task.Delay(1000);
                    }
                }
            }
        }
        static async Task<bool> CatchPokemon(PokemonGoClient client, ulong encounterId, string spawnPointId, CaptureProbability proba, PokemonId pokeId)
        {
            var itemToUse = POGOProtos.Inventory.ItemId.ItemPokeBall;
            bool useRasberry = false;
            var pokeIndex = 0;
            var rIndex = 0;
            while (proba.CaptureProbability_[rIndex] < 0.5)
            {
                if (client.InventoryManager.GetCountForItem((ItemId)(pokeIndex + (int)ItemId.ItemPokeBall)) > 0)
                {
                    rIndex = pokeIndex;
                    itemToUse = (ItemId)pokeIndex + (int)ItemId.ItemPokeBall;
                }
                pokeIndex++;
                if (pokeIndex > 2) break;
            }
            if (proba.CaptureProbability_[rIndex] < 0.5 && client.InventoryManager.GetCountForItem(ItemId.ItemRazzBerry) > 0)
                useRasberry = true;
            CatchPokemonResponse caughtPokemonResponse;
            do
            {
                if (useRasberry)
                {
                    var res = await client.GetUseItemCaptureResponse(ItemId.ItemRazzBerry, encounterId, spawnPointId);
                    Logger.Write("Used Rasberry response :" + res.Success);
                    await Task.Delay(1000);
                }
                caughtPokemonResponse = await client.GetCatchPokemonResponse(encounterId, spawnPointId, itemToUse);
                if (caughtPokemonResponse.Status != CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
                    await Task.Delay(1000);
            }
            while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);

            Logger.Write(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"We caught a {pokeId}" : $"{pokeId} got away..");
            if (caughtPokemonResponse.CaptureAward != null)
                Logger.Write("Earned : " + caughtPokemonResponse.CaptureAward.Candy.Sum() + " candies and " + caughtPokemonResponse.CaptureAward.Xp.Sum() + " xp and " + caughtPokemonResponse.CaptureAward.Stardust.Sum() + " stardusts");
            return caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess;
        }
        static async Task CatchAllPokemonAround(PokemonGoClient client)
        {
            await client.UpdateMapObjects();
            var pokemons = client.MapManager.GetNearbyCells(client.Latitude,client.Longitude).SelectMany(i => i.CatchablePokemons).ToList();
            if (client.InventoryManager.Items.Any(a => a.InventoryItemData.AppliedItems?.Item.Any(b => b.ItemType == ItemType.Incense) == true))
            {
                var inc = await client.GetIncensePokemonsResponse();
                if (inc.Result == GetIncensePokemonResponse.Types.Result.IncenseEncounterAvailable)
                {
                    var encounter = await client.GetIncenseEncounterResponse((long)inc.EncounterId, inc.EncounterLocation);
                    if (encounter.Result == IncenseEncounterResponse.Types.Result.IncenseEncounterSuccess)
                    {
                        await CatchPokemon(client, (ulong)inc.EncounterId, inc.EncounterLocation, encounter.CaptureProbability, encounter.PokemonData.PokemonId);
                    }
                }
            }

            var orig = S2LatLng.FromDegrees(client.Latitude, client.Longitude);

            foreach (var pokemon in pokemons)
            {
                var encounterPokemonRespone = await client.GetEncounterResponse(pokemon.EncounterId, pokemon.SpawnPointId);
                if (encounterPokemonRespone.Status != EncounterResponse.Types.Status.EncounterSuccess) continue;
                await CatchPokemon(client, pokemon.EncounterId, pokemon.SpawnPointId, encounterPokemonRespone.CaptureProbability, pokemon.PokemonId);
                await Task.Delay(2000);
                checkForDuplicates++;
                if (checkForDuplicates % 50 == 0)
                {
                    await EarnXpWithEvolveOrPurgeDuplicates(client);
                    checkForDuplicates = 0;
                }
            }
        }
        private static async Task EarnXpWithEvolveOrPurgeDuplicates(PokemonGoClient client)
        {
            await client.UpdateInventory();
            var candies = client.InventoryManager.Items.Select(i => i.InventoryItemData?.PokemonFamily).Where(x => x != null).ToList();
            var allpokemons = client.InventoryManager.Items.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p?.PokemonId != PokemonId.Missingno);

            int potentialXp = 0;
            int realXp = 0;
            var pokemonTypesNeeded = new List<PokemonId>();
            //Rattata
            foreach (var tt in XpFarmingPokemons)
            {
                var pokemonNeeded = 0;
                var candyCount = candies.Where(x => x.FamilyId == tt.Item2).FirstOrDefault()?.Candy;
                var pokemonCurrent = 0;
                if (candyCount.HasValue)
                {
                    pokemonNeeded = (int)(candyCount / tt.Item3);
                    potentialXp += pokemonNeeded * 500;
                    pokemonCurrent = allpokemons.Where(x => x.PokemonId == tt.Item1).Count();
                    realXp += pokemonCurrent > pokemonNeeded ? pokemonNeeded * 500 : pokemonCurrent * 500;
                }
                if (pokemonNeeded > pokemonCurrent) pokemonTypesNeeded.Add(tt.Item1);
            }
            if (realXp > 15000)
                await EvolveUnwantedPokemons(client);

            await TransferDuplicates(pokemonTypesNeeded, client);
        }
        private static int checkForDuplicates = -1;
        private static async Task EvolveUnwantedPokemons(PokemonGoClient client)
        {

            await client.UpdateInventory();
            if (!client.InventoryManager.Items.Any(x => x.InventoryItemData?.AppliedItems?.Item?.Any(a => a.ItemId == ItemId.ItemLuckyEgg) == true) && client.InventoryManager.Items.Where(x => x.InventoryItemData.Item?.ItemId == ItemId.ItemLuckyEgg).Select(x => x.InventoryItemData.Item.Count).FirstOrDefault() > 1)
                await client.GetUseItemXpBoostResponse(ItemId.ItemLuckyEgg);
            var candies = client.InventoryManager.Items.Select(i => i.InventoryItemData?.PokemonFamily).Where(x => x != null).ToList();
            var allpokemons = client.InventoryManager.Items.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p?.PokemonId != PokemonId.Missingno);
            int xpEarned = 0;
            foreach (var tt in XpFarmingPokemons)
            {
                var pokemonNeeded = 0;
                var candyCount = candies.Where(x => x.FamilyId == tt.Item2).FirstOrDefault()?.Candy;
                var pokemonCurrent = 0;
                if (candyCount.HasValue)
                {
                    pokemonNeeded = (int)(candyCount / tt.Item3);
                    pokemonCurrent = allpokemons.Where(x => x.PokemonId == tt.Item1).Count();
                    var pokeToEvolve = Math.Min(pokemonNeeded, pokemonCurrent);
                    for (var i = 0; i < pokeToEvolve; i++)
                    {
                        var res = await client.GetEvolvePokemonResponse(allpokemons.Where(x => x.PokemonId == tt.Item1).Skip(i).FirstOrDefault().Id);
                        xpEarned += res.ExperienceAwarded;
                    }
                }
            }
            Logger.Write("All undesired pokemons evolved : " + xpEarned + "xp received");
        }
        private static async Task TransferDuplicates(List<PokemonId> neededPokemons, PokemonGoClient client)
        {
            Logger.Write($"Check for duplicates");
            await client.UpdateInventory();
            var allpokemons = client.InventoryManager.Items.Select(i => i.InventoryItemData?.PokemonData).Where(p => p != null && p?.PokemonId != PokemonId.Missingno);

            var dupes = allpokemons.OrderBy(x => x.Cp).Select((x, i) => new { index = i, value = x })
              .GroupBy(x => x.value.PokemonId)
              .Where(x => x.Skip(1).Any());

            for (int i = 0; i < dupes.Count(); i++)
            {
                if (neededPokemons.Contains(dupes.ElementAt(i).Key)) continue;
                for (int j = 0; j < dupes.ElementAt(i).Count() - 1; j++)
                {
                    var dubpokemon = dupes.ElementAt(i).ElementAt(j).value;
                    var transfer = await client.GetReleasePokemonResponse(dubpokemon.Id);
                    Logger.Write($"Transfer {dubpokemon.PokemonId} with {dubpokemon.Cp} CP (highest has {dupes.ElementAt(i).Last().value.Cp})");
                }
            }
        }

        private static string GetFriendlyItemsString(IEnumerable<ItemAward> items)
        {
            var enumerable = items as IList<ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                          .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                          .Select(y => $"{y.Amount} x {y.ItemName}")
                          .Aggregate((a, b) => $"{a}, {b}");
        }

    }
}
