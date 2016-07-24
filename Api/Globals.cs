using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api
{
    class Globals
    {
        static private Dictionary<RequestType, Type> ResponseTypes = new Dictionary<RequestType, Type>()
        {
            { RequestType.AddFortModifier, typeof(AddFortModifierResponse) },
            { RequestType.AttackGym, typeof(AttackGymResponse) },
            { RequestType.CatchPokemon, typeof(CatchPokemonResponse) },
            { RequestType.CheckAwardedBadges, typeof(CheckAwardedBadgesResponse) },
            { RequestType.CheckCodenameAvailable, typeof(CheckCodenameAvailableResponse) },
            { RequestType.ClaimCodename, typeof(ClaimCodenameResponse) },
            { RequestType.CollectDailyBonus, typeof(CollectDailyBonusResponse) },
            { RequestType.CollectDailyDefenderBonus, typeof(CollectDailyDefenderBonusResponse) },
            { RequestType.DiskEncounter, typeof(DiskEncounterResponse) },
            {RequestType.DownloadItemTemplates, typeof(DownloadItemTemplatesResponse) },
            {RequestType.DownloadRemoteConfigVersion, typeof(DownloadRemoteConfigVersionResponse) },
            {RequestType.DownloadSettings, typeof(DownloadSettingsResponse) },
            {RequestType.Echo,typeof(EchoResponse) },
            {RequestType.Encounter,typeof(EncounterResponse) },
            {RequestType.EncounterTutorialComplete,typeof(EncounterTutorialCompleteResponse) },
            {RequestType.EquipBadge,typeof(EquipBadgeResponse) },
            {RequestType.EvolvePokemon,typeof(EvolvePokemonResponse) },
            {RequestType.FortDeployPokemon,typeof(FortDeployPokemonResponse) },
            {RequestType.FortDetails,typeof(FortDetailsResponse) },
            {RequestType.FortRecallPokemon, typeof(FortRecallPokemonResponse) },
            {RequestType.FortSearch, typeof(FortSearchResponse) },
            {RequestType.GetAssetDigest, typeof(GetAssetDigestResponse) },
            {RequestType.GetDownloadUrls, typeof(GetDownloadUrlsResponse) },
            {RequestType.GetGymDetails, typeof(GetGymDetailsResponse) },
            {RequestType.GetHatchedEggs, typeof(GetHatchedEggsResponse) },
            {RequestType.GetIncensePokemon, typeof(GetIncensePokemonResponse) },
            {RequestType.GetInventory, typeof(GetInventoryResponse) },
            {RequestType.GetMapObjects, typeof(GetMapObjectsResponse) },
            {RequestType.GetPlayer, typeof(GetPlayerResponse) },
            {RequestType.GetPlayerProfile, typeof(GetPlayerProfileResponse) },
            {RequestType.GetSuggestedCodenames, typeof(GetSuggestedCodenamesResponse) },
            {RequestType.IncenseEncounter, typeof(IncenseEncounterResponse) },
            {RequestType.LevelUpRewards, typeof(LevelUpRewardsResponse) },
            {RequestType.MarkTutorialComplete, typeof(MarkTutorialCompleteResponse) },
            {RequestType.NicknamePokemon, typeof(NicknamePokemonResponse) },
            {RequestType.PlayerUpdate, typeof(PlayerUpdateResponse) },
            {RequestType.RecycleInventoryItem, typeof(RecycleInventoryItemResponse) },
            {RequestType.ReleasePokemon, typeof(ReleasePokemonResponse) },
            {RequestType.SetAvatar,typeof(SetAvatarResponse) },
            {RequestType.SetContactSettings,typeof(SetContactSettingsResponse) },
            {RequestType.SetFavoritePokemon,typeof(SetFavoritePokemonResponse) },
            {RequestType.SetPlayerTeam,typeof(SetPlayerTeamResponse) },
            {RequestType.StartGymBattle,typeof(StartGymBattleResponse) },
            {RequestType.UpgradePokemon,typeof(UpgradePokemonResponse) },
            {RequestType.UseIncense,typeof(UseIncenseResponse) },
            {RequestType.UseItemCapture,typeof(UseItemCaptureResponse) },
            {RequestType.UseItemEggIncubator,typeof(UseItemEggIncubatorResponse) },
            {RequestType.UseItemGym,typeof(UseItemGymResponse) },
            {RequestType.UseItemPotion,typeof(UseItemPotionResponse) },
            {RequestType.UseItemRevive,typeof(UseItemReviveResponse) },
            {RequestType.UseItemXpBoost,typeof(UseItemXpBoostResponse) }
        };
        static public Type GetResponseTypeForRequestType(RequestType reqType)
        {
            if (ResponseTypes.ContainsKey(reqType)) return ResponseTypes[reqType];
            else return null; 
        }

        public const string RpcUrl = @"https://pgorelease.nianticlabs.com/plfe/rpc";
        public const string NumberedRpcUrl = @"https://pgorelease.nianticlabs.com/plfe/{0}/rpc";
        public const string PtcLoginUrl = "https://sso.pokemon.com/sso/login?service=https%3A%2F%2Fsso.pokemon.com%2Fsso%2Foauth2.0%2FcallbackAuthorize";
        public const string PtcLoginOauth = "https://sso.pokemon.com/sso/oauth2.0/accessToken";
        public const string GoogleGrantRefreshAccessUrl = "https://android.clients.google.com/auth";
        public const double AcceptedRadius = 15; // 25meters.
        public const double WalkingSpeed = 3.5; //in meters/sec
        public const double BicycleSpeed = 7.0;
        public const double CarSpeed = 14.0;

        static public string DbUri => System.Configuration.ConfigurationManager.AppSettings["DbUri"];
            
        
    }
}
