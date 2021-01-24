using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;

namespace Postcarbon {
  public class Node: NetworkedBehaviour {
    public Collider col;
    public dItem resource;
    public int maxHealth = 5;
    public int respawnTime = 120;
    [HideInInspector] public int health;
    [HideInInspector] public Vector3 pos;
    [HideInInspector] public Vector3 rot;

    void Start() {
      pos = transform.position;
      rot = new Vector3(transform.rotation.x, transform.rotation.y, transform.rotation.z);
      Spawn();
    }

    public void TakeDamage(Hero hero) {
      Harvest(hero);
      hero.hud.RefreshInventory();
    }

    public void Harvest(Hero hero) {
      if (health > 0) {
        health -= 1;
        hero.inventory.Store(resource, 1);
        if (health <= 0) { StartCoroutine(Respawn()); }
      }
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
      health = maxHealth;
      transform.position = new Vector3(pos.x, pos.y, pos.z);
      transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
      gameObject.SetActive(true);
    }
  }
}