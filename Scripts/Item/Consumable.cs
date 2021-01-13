using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  [System.Serializable]
  public class Consumable : Item {
    [HideInInspector] public dConsumable dConsumable;

    void Awake() {
      dConsumable = dItem as dConsumable;
    }
  }
}