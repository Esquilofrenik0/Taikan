using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [System.Serializable]
  public class dItem : ScriptableObject {
    public iT type = iT.Scrap;
    public string Name = null;
    public string description = null;
    public Sprite icon = null;
    public float value = 0.01f;
    public float weight = 0.01f;
    public int stack = 1;
  }
}