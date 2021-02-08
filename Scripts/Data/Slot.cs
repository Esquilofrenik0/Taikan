using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Serialization;

namespace Postcarbon {
  [System.Serializable]
  public class Slot  {
    public dItem dItem;
    [Range(1, 999)] public int amount;
  }
}