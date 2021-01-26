using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;

namespace Postcarbon {
  public class Stone: MonoBehaviour {
    public Collider col;
    public dItem resource;
    public int respawnTime = 120;
    
    public void Start() { 
      Spawn(); 
    }

    public void Pickup(Hero hero) {
      hero.inventory.Store(resource, 1);
      StartCoroutine(Respawn());
    }

    IEnumerator Respawn() {
      Unspawn();
      yield return new WaitForSeconds(respawnTime);
      Spawn();
    }

    public void Unspawn() {
      gameObject.SetActive(false);
    }

    public void Spawn() {
      gameObject.SetActive(true);
    }
  }
}
