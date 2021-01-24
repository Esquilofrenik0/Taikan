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

    public void Store(int iSlotNumber) {
      inventory.StoreStack(interactingHero.inventory.slot[iSlotNumber]);
      interactingHero.inventory.RemoveStack(iSlotNumber);
    }

    public void Retrieve(int cSlotNUmber) {
      interactingHero.inventory.StoreStack(inventory.slot[cSlotNUmber]);
      inventory.RemoveStack(cSlotNUmber);
    }
  }
}