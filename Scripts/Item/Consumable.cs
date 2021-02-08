using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [System.Serializable]
  public class Consumable : Item {
    [HideInInspector] public dConsumable dConsumable;

    public override void SetData(dItem data) {
      base.SetData(data);
      dConsumable = data as dConsumable;
    }

    void Awake() {
      dConsumable = dItem as dConsumable;
    }
  }
}