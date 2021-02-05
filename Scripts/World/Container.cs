using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [System.Serializable]
  public class Container: MonoBehaviour {
    public Inventory inventory;
    [HideInInspector] public bool open = false;
    [HideInInspector] public Hero interactingHero;

    public void CreateInventory(int _nSlots) {
      open = false;
    }

    public void Open(Hero hero) {
      open = true;
      hero.container = this;
      interactingHero = hero;
    }

    public void Close() {
      open = false;
      interactingHero = null;
    }

    public void Store(Hero hero, int iSlotNumber) {
      Slot slot = new Slot();
      slot.amount = interactingHero.inventory.amount[iSlotNumber];
      slot.dItem = interactingHero.inventory.item[iSlotNumber];
      inventory.StoreStack(slot);
      hero.inventory.RemoveStack(iSlotNumber);
    }

    public void Retrieve(Hero hero, int cSlotNumber) {
      Slot slot = new Slot();
      slot.amount = inventory.amount[cSlotNumber];
      slot.dItem = inventory.item[cSlotNumber];
      hero.inventory.StoreStack(slot);
      inventory.RemoveStack(cSlotNumber);
    }
  }
}