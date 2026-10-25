using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace WhatTheFish {
 [Serializable,Preserve] public sealed class CosmeticItem {public string id,slot,name;public int index,price;}
 [Serializable,Preserve] sealed class CosmeticCatalog {public CosmeticItem[] items;}
 public static class CosmeticStore {
  static CosmeticItem[] items;
  public static CosmeticItem[] Items=>items??=JsonUtility.FromJson<CosmeticCatalog>(Resources.Load<TextAsset>("EconomyCosmetics").text).items;
  public static CosmeticItem Item(string slot,int index)=>Items.FirstOrDefault(i=>i.slot==slot&&i.index==index);
  public static bool Owned(CosmeticItem item)=>item!=null&&(item.price==0||PlayerEconomy.Wallet?.storeVersion==2&&PlayerEconomy.Wallet.playerId==PlayerEconomy.Owner&&PlayerEconomy.Wallet.owned.Contains(item.id));
  public static bool Owned(string slot,int index)=>Owned(Item(slot,index));
  public static int Allowed(string slot,int index)=>Owned(slot,index)?index:0;
  public static string VenueKey(SportId sport)=>"economy.venue."+PlayerIdentity.ProfileStorageKey+"."+sport;
  public static StadiumAppearance Allowed(SportId sport,StadiumAppearance value){
   value.Clamp();string prefix=sport.ToString().ToLowerInvariant();value.palette=Allowed(prefix+"Palette",value.palette);
   if(sport==SportId.Basketball)value.logo=Allowed("basketballLogo",value.logo);
   else value.design=Allowed(prefix+(sport==SportId.Football?"Message":"Finish"),value.design);
   return value;
  }
  [Serializable,Preserve] sealed class PurchaseRequest {public string id;}
  public static async Task<string> Purchase(CosmeticItem item){
   if(item==null)return "This item is unavailable.";
   if(PlayerEconomy.Busy||!PlayerEconomy.Enabled||!PlayerEconomy.Confirmed)return "Connect to sync your coins.";
   string expected=PlayerEconomy.Scope;
   try{
    var wallet=await PlayerEconomy.Request<CoinWallet>("purchase","POST",new PurchaseRequest{id=item.id});
    if(expected!=PlayerEconomy.Scope)return "Account changed. Open the item again.";
    PlayerEconomy.Receive(wallet,expected);return Owned(item)?"Unlocked. Saved to your account.":"Purchase unavailable. Please retry online.";
   }catch(ProfileException e)when(e.Code=="insufficient_coins"){PlayerEconomy.RefreshSoon();return "Not enough coins. Earn more by signing in and playing matches.";}
   catch(Exception){PlayerEconomy.RefreshSoon();return "Purchase unavailable. Please retry online.";}
  }
 }
}
