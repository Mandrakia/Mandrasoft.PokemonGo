using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandraSoft.PokemonGo.Api.Managers
{
     /// <summary>
     /// It's the class that keeps the state of your playing Inventory and handles DeltaResponses from the server so that you always have Up to date state of your inventory
     /// without asking for it at everycall.
     /// </summary>
    public class InventoryManager
    {
        //TODO : Rewrite new POCOs for Inventory Items, but low priority because only Updated in Bot Usage. (Current Implementation can throw exception on RepeatedFields.
        private object selfLock = new object();
        public long LastTimestamp { get; set; }
        public List<InventoryItem> Items { get; set; }

        public InventoryManager()
        {
            Items = new List<InventoryItem>();
        }
        /// <summary>
        /// Called by the genericResponseHandler each time a response of Type GetInventoryResponse is received.
        /// </summary>
        /// <param name="msg"></param>
        public void UpdateItems(GetInventoryResponse msg)
        {
            if(LastTimestamp == 0)
            {
                Items = msg.InventoryDelta.InventoryItems.ToList();
                LastTimestamp = msg.InventoryDelta.NewTimestampMs;
            }
            else
            {
                LastTimestamp = msg.InventoryDelta.NewTimestampMs;
                UpdateItemCollection(msg.InventoryDelta.InventoryItems);
            }
        }
        private void UpdateItemCollection(IList<InventoryItem> items)
        {
            lock (selfLock)
            {
                foreach (var item in items.ToList())
                {
                    if (item.DeletedItem != null)
                    {
                        var id = item.DeletedItem.ReleasedPokemonId;
                        var oPokemon = Items.Where(x => x.InventoryItemData.PokemonData?.Id == id).SingleOrDefault();
                        if (oPokemon != null)
                            Items.Remove(oPokemon);
                    }
                    //Should be unique hopefully...
                    else if (item.InventoryItemData.PlayerStats != null)
                    {
                        var oItem = Items.Where(x => x.InventoryItemData.PlayerStats != null).SingleOrDefault();
                        if (oItem.ModifiedTimestampMs < item.ModifiedTimestampMs)
                            Items[Items.IndexOf(oItem)] = item;
                    }
                    else if (item.InventoryItemData.Item != null)
                    {
                        var oItem = Items.Where(x => x.InventoryItemData.Item?.ItemId == item.InventoryItemData.Item.ItemId).FirstOrDefault();
                        if (oItem == null) Items.Add(item);
                        else if (oItem.ModifiedTimestampMs < item.ModifiedTimestampMs)
                            Items[Items.IndexOf(oItem)] = item;
                    }
                    else if (item.InventoryItemData.PokemonFamily != null)
                    {
                        var oItem = Items.Where(x => x.InventoryItemData.PokemonFamily?.FamilyId == item.InventoryItemData.PokemonFamily.FamilyId).FirstOrDefault();
                        if (oItem == null) Items.Add(item);
                        else if (oItem.ModifiedTimestampMs < item.ModifiedTimestampMs)
                            Items[Items.IndexOf(oItem)] = item;
                    }
                    else if (item.InventoryItemData.PokedexEntry != null)
                    {
                        var oItem = Items.Where(x => x.InventoryItemData.PokedexEntry?.PokedexEntryNumber == item.InventoryItemData.PokedexEntry.PokedexEntryNumber).FirstOrDefault();
                        if (oItem == null) Items.Add(item);
                        else if (oItem.ModifiedTimestampMs < item.ModifiedTimestampMs)
                            Items[Items.IndexOf(oItem)] = item;
                    }
                    else if (item.InventoryItemData.PokemonData != null)
                    {
                        var oItem = Items.Where(x => x.InventoryItemData.PokemonData?.Id == item.InventoryItemData.PokemonData.Id).FirstOrDefault();
                        if (oItem == null) Items.Add(item);
                        else if (oItem.ModifiedTimestampMs < item.ModifiedTimestampMs)
                            Items[Items.IndexOf(oItem)] = item;
                    }
                    else
                    {
                        //TODO: Handle Incubators etc...
                    }
                }
            }
        }

        public int GetCountForItem(ItemId item)
        {
            return Items.Where(x => x.InventoryItemData?.Item?.ItemId == item).Select(x => x.InventoryItemData?.Item?.Count).FirstOrDefault().GetValueOrDefault();
        }
    }
}
