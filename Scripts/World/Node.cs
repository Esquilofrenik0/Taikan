﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  public class Node : MonoBehaviour {
    public Rigidbody rb;
    public Collider col;
    public dItem resource;
    public int maxHealth = 5;
    public int unspawnTime = 20;
    public int respawnTime = 10;
    [HideInInspector] public int health;
    [HideInInspector] public Vector3 pos;
    [HideInInspector] public Vector3 rot;

    public void Start(){
      pos = transform.position;
      rot = new Vector3(transform.rotation.x,transform.rotation.y,transform.rotation.z);
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
        if (health <= 0) {
          rb.isKinematic = false;
          Vector3 force = hero.transform.forward * 100 * rb.mass;
          rb.AddForce(force, ForceMode.Force);
          ResetNode();
        }
      }
    }
    
    public void ResetNode() {
      StartCoroutine(Respawn());
    }

    IEnumerator Respawn() {
      yield return new WaitForSeconds(unspawnTime);
      Unspawn();
      yield return new WaitForSeconds(respawnTime);
      Spawn();
    }

    public void Unspawn() {
      rb.gameObject.SetActive(false);
    }

    public void Spawn() {
      rb.transform.position = new Vector3(pos.x, pos.y, pos.z);
      rb.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
      rb.isKinematic = true;
      health = maxHealth;
      rb.gameObject.SetActive(true);
    }
  }
}