using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class InfluenceArea: MonoBehaviour {
    public Collider area;

    private void OnTriggerEnter(Collider other) {
      if (other.gameObject.layer == 20) {
        print("Found Vegetation");
      }
    }
  }
}