using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [CreateAssetMenu(fileName = "Buildable", menuName = "SRPG/Buildable")]
  [System.Serializable]
  public class dBuildable: dItem {
    public GameObject prefab;
    public GameObject preview;
    public bool isFoundation = false;
    public Material canBuild;
    public Material cantBuild;
    public List<string> tags = new List<string>();
  }
}