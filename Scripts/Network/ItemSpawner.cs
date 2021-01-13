using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;

namespace SRPG {
  public class ItemSpawner: NetworkedBehaviour {
    public List<dItem> database;
    
    public dItem GetItem(string name) {
      dItem dItem = null;
      for (int i = 0; i < database.Count; i++) {
        if (database[i].name == name) {
          dItem = database[i];
          break;
        }
      }
      return dItem;
    }
  }
}
