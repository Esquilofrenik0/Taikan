using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.Serialization;

namespace SRPG {
  [System.Serializable]
  public struct Slot {
    public dItem dItem;
    public int amount;
  }

  [System.Serializable]
  public class Inventory: NetworkedBehaviour {
    public int nSlots = 16;
    public Slot[] slot;

    public void Awake() {
      Slot[] _slot = new Slot[nSlots];
      for (int i = 0; i < nSlots; i++) {
        if (i < slot.Length) {
          _slot[i] = slot[i];
        }
        else {
          Slot spawn = new Slot();
          spawn.amount = 0;
          spawn.dItem = null;
          _slot[i].amount = 0;
          _slot[i].dItem = null;
        }
      }
      slot = _slot;
    }

    public int FreeSlot() {
      for (int i = 0; i < nSlots; i++) {
        if (slot[i].amount == 0) {
          return i;
        }
      }
      return -1;
    }

    public void Store(dItem dItem, int storeAmount) {
      if (dItem.stack > 1) {
        int number = SearchItem(dItem);
        if (number >= 0) {
          if (slot[number].amount + storeAmount <= dItem.stack) {
            slot[number].amount += storeAmount;
            return;
          }
        }
      }
      for (int i = 0; i < nSlots; i++) {
        if (slot[i].amount == 0) {
          slot[i].dItem = dItem;
          slot[i].amount = storeAmount;
          return;
        }
      }
    }

    public void StoreStack(Slot toStore) {
      if (toStore.dItem.stack > 1) {
        int number = SearchItem(toStore.dItem);
        if (number >= 0) {
          if (slot[number].amount + toStore.amount <= toStore.dItem.stack) {
            slot[number].amount += toStore.amount;
            return;
          }
        }
      }
      for (int i = 0; i < nSlots; i++) {
        if (slot[i].amount == 0) {
          slot[i].dItem = toStore.dItem;
          slot[i].amount = toStore.amount;
          return;
        }
      }
    }

    public int SearchItem(dItem dItem) {
      for (int i = 0; i < nSlots; i++) {
        if (slot[i].amount > 0) {
          if (slot[i].dItem == dItem) {
            return i;
          }
        }
      }
      return -1;
    }

    public void Remove(int number, int amount) {
      slot[number].amount -= amount;
      if (slot[number].amount <= 0) {
        RemoveStack(number);
      }
    }

    public void RemoveStack(int number) {
      slot[number].amount = 0;
      slot[number].dItem = null;
    }
  }
}