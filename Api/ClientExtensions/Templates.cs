using Google.Protobuf;
using MandraSoft.PokemonGo.Api.ClientExtensions;
using MandraSoft.PokemonGo.Api.Extensions;
using Newtonsoft.Json;
using POGOProtos.Enums;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api
{
    static class Templates
    {
        public static GetAssetDigestResponse AssetDigest { get; set; }

        public static DownloadItemTemplatesResponse ItemTemplates { get; set; }

        public static IEnumerable<PokemonSettings> GetPokemonSettings()
        {
            return
                ItemTemplates.ItemTemplates.Select(i => i.PokemonSettings)
                    .Where(p => p != null && p?.FamilyId != PokemonFamilyId.FamilyUnset);
        }

        public static async Task Initialise(this PokemonGoClient client)
        {
            var response = (DownloadRemoteConfigVersionResponse)(await client._httpClient.GetResponses(
                client, true, client._apiUrl, null, null, client.GetDownloadRemoteConfigVersionRequest()))[0];
            // Currently there seems to be a bug since AssetDigest timestamp is 1467338276561000 which is way in the future
            // That's why the client (it seems) is pulling AssetDigest every time and itemTemplates only after fresh install
            AssetDigest = LoadAssetDigest();
            if (AssetDigest == null || response.AssetDigestTimestampMs > (ulong)DateTime.UtcNow.ToUnixTime())
            {
                AssetDigest = (GetAssetDigestResponse)(await client._httpClient.GetResponses(
                    client, true, client._apiUrl, null, null, client.GetAssetDigestRequest()))[0];
                if (AssetDigest != null)
                {
                    SaveAssetDigest(AssetDigest);
                }
            }
            ItemTemplates = LoadItemTemplates();
            if (ItemTemplates == null || response.ItemTemplatesTimestampMs > (ulong)DateTime.UtcNow.ToUnixTime())
            {
                ItemTemplates = (DownloadItemTemplatesResponse)(await client._httpClient.GetResponses(
                    client, true, client._apiUrl, null, null, client.GetDownloadItemTemplatesRequest()))[0];
                if (ItemTemplates.Success)
                {
                    SaveItemTemplates(ItemTemplates);
                }
            }
        }

        private static void SaveItemTemplates(DownloadItemTemplatesResponse itemTemplates)
        {
            var fileName = Path.Combine(Environment.CurrentDirectory, "cache", "itemTemplates");

            File.WriteAllText($"{fileName}.json", JsonConvert.SerializeObject(itemTemplates, Formatting.Indented));
            File.WriteAllBytes($"{fileName}.dat", itemTemplates.ToByteString().ToByteArray());
        }

        internal static DownloadItemTemplatesResponse LoadItemTemplates()
        {
            var cacheDir = Path.Combine(Environment.CurrentDirectory, "cache");
            var fileName = Path.Combine(cacheDir, "itemTemplates.dat");

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            if (File.Exists(fileName))
            {
                return DownloadItemTemplatesResponse.Parser.ParseFrom(File.ReadAllBytes(fileName));
            }
            return null;
        }

        private static void SaveAssetDigest(GetAssetDigestResponse assetDigest)
        {
            var fileName = Path.Combine(Environment.CurrentDirectory, "cache", "assetDigest");

            File.WriteAllText($"{fileName}.json", JsonConvert.SerializeObject(assetDigest, Formatting.Indented));
            File.WriteAllBytes($"{fileName}.dat", assetDigest.ToByteString().ToByteArray());
        }

        internal static GetAssetDigestResponse LoadAssetDigest()
        {
            var cacheDir = Path.Combine(Environment.CurrentDirectory, "cache");
            var fileName = Path.Combine(cacheDir, "assetDigest.dat");

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            if (File.Exists(fileName))
            {
                return GetAssetDigestResponse.Parser.ParseFrom(File.ReadAllBytes(fileName));
            }
            return null;
        }
    }
}