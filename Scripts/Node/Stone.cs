using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class Stone: MonoBehaviour {
    public Collider col;
    public dItem resource;
    public int respawnTime = 120;
    [HideInInspector] public Vector3 pos;
    [HideInInspector] public Vector3 rot;

    public void Start() {
      pos = transform.position;
      rot = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
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
      transform.position = new Vector3(pos.x, pos.y, pos.z);
      transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
      gameObject.SetActive(true);
    }
  }
}
