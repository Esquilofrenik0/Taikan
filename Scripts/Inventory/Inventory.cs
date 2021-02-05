using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Serialization;

namespace Postcarbon {
  [System.Serializable]
  public class Inventory: NetworkedBehaviour {
    public int nSlots = 16;
    public List<Slot> initItems;
    public NetworkedList<int> amount = new NetworkedList<int>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<int>());
    public NetworkedList<dItem> item = new NetworkedList<dItem>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<dItem>());

    void inventoryItemChanged(NetworkedListEvent<dItem> changeEvent) {
      if (IsLocalPlayer && GetComponent<HUD>() && GetComponent<HUD>().hero) {
        GetComponent<HUD>().Refresh();
      }
      else if (GetComponent<Container>() && GetComponent<Container>().interactingHero) {
        if (GetComponent<Container>().interactingHero.IsLocalPlayer) {
          GetComponent<Container>().interactingHero.hud.Refresh();
        }
      }
    }

    void inventoryAmountChanged(NetworkedListEvent<int> changeEvent) {
      if (IsLocalPlayer && GetComponent<HUD>() && GetComponent<HUD>().hero) {
        GetComponent<HUD>().Refresh();
      }
      else if (GetComponent<Container>() && GetComponent<Container>().interactingHero) {
        if (GetComponent<Container>().interactingHero.IsLocalPlayer) {
          GetComponent<Container>().interactingHero.hud.Refresh();
        }
      }
    }


    public override void NetworkStart() {
      if (IsServer || IsLocalPlayer) {
        for (int i = 0; i < nSlots; i++) {
          item.Add(null);
          amount.Add(0);
          if (initItems.Count > i && initItems[i].dItem != null) {
            item[i] = initItems[i].dItem;
            if (initItems[i].amount > initItems[i].dItem.stack) { initItems[i].amount = initItems[i].dItem.stack; }
            amount[i] = initItems[i].amount;
          }
        }
      }
      item.OnListChanged += inventoryItemChanged;
      amount.OnListChanged += inventoryAmountChanged;
    }

    public int FreeSlot() {
      for (int i = 0; i < nSlots; i++) {
        if (item[i] == null) {
          return i;
        }
      }
      return -1;
    }

    public void Store(dItem dItem, int storeAmount) {
      if (dItem.stack > 1) {
        int number = SearchItem(dItem);
        if (number >= 0) {
          if (amount[number] + storeAmount <= dItem.stack) {
            amount[number] += storeAmount;
            return;
          }
        }
      }
      for (int i = 0; i < nSlots; i++) {
        if (item[i] == null) {
          item[i] = dItem;
          amount[i] = storeAmount;
          return;
        }
      }
    }

    public void StoreStack(Slot toStore) {
      if (toStore.dItem.stack > 1) {
        int number = SearchItem(toStore.dItem);
        if (number >= 0) {
          if (amount[number] + toStore.amount <= toStore.dItem.stack) {
            amount[number] += toStore.amount;
            return;
          }
        }
      }
      for (int i = 0; i < nSlots; i++) {
        if (item[i] == null) {
          item[i] = toStore.dItem;
          amount[i] = toStore.amount;
          return;
        }
      }
    }

    public int SearchItem(dItem dItem) {
      for (int i = 0; i < nSlots; i++) {
        if (item[i] != null) {
          if (item[i] == dItem) {
            return i;
          }
        }
      }
      return -1;
    }

    public void Remove(int number, int removeAmount) {
      amount[number] -= removeAmount;
      if (amount[number] <= 0) {
        RemoveStack(number);
      }
    }

    public void RemoveStack(int number) {
      amount[number] = 0;
      item[number] = null;
    }
  }
}