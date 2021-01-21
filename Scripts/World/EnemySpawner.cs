using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using UnityEngine.AI;

namespace SRPG {
  public class EnemySpawner: NetworkedBehaviour {
    public GameObject toSpawn;
    public int number = 5;
    [HideInInspector] public List<NetworkedObject> enemies;
    [HideInInspector] public Coroutine unspawnRoutine;
    bool spawned = false;
    bool unspawn = false;

    public override void NetworkStart() {
      if (!IsServer) { return; }
      spawned = false;
      unspawn = false;
    }

    private void OnTriggerEnter(Collider other) {
      if (!IsServer) { return; }
      if (other.tag == "Player") {
        unspawn = false;
        if (!spawned) { SpawnEnemies(); }
      }
    }

    private void OnTriggerStay(Collider other) {
      if (!IsServer) { return; }
      if (other.tag == "Player") {
        if (spawned) {
          if (enemies.Count > 0) {
            for (int i = enemies.Count - 1; i >= 0; i--) {
              if (enemies[i].GetComponent<Pawn>().state == (int)pS.Dead) {
                DestroyEnemy(i);
              }
            }
          }
          else { spawned = false; }
        }
      }
    }

    private void OnTriggerExit(Collider other) {
      if (!IsServer) { return; }
      if (other.tag == "Player") {
        if (spawned) {
          unspawn = true;
          Timer.rDelay(this, UnspawnEnemies, 10, unspawnRoutine);
        }
      }
    }

    public void DestroyEnemy(int i) {
      if (!IsServer) { return; }
      NetworkedObject[] equips = enemies[i].GetComponentsInChildren<NetworkedObject>();
      for (int j = 0; j < equips.Length; j++) {
        Destroy(equips[j].gameObject);
      }
      Destroy(enemies[i].gameObject);
      enemies.RemoveAt(i);
    }

    public void UnspawnEnemies() {
      if (!IsServer) { return; }
      if (unspawn) {
        for (int i = enemies.Count - 1; i >= 0; i--) {
          if (enemies[i]) {
            DestroyEnemy(i);
          }
        }
        print("Enemies Unspawned!");
      }
    }

    public void SpawnEnemies() {
      if (!IsServer) { return; }
      for (int i = 0; i < number; i++) {
        Vector3 where = transform.position;
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(where, out myNavHit, 100, -1)) { where = myNavHit.position; }
        GameObject spawn = Instantiate(toSpawn, where, Quaternion.identity);
        spawn.GetComponent<NetworkedObject>().Spawn();
        enemies.Add(spawn.GetComponent<NetworkedObject>());
      }
      spawned = true;
      print("Enemies Spawned!");
    }
  }
}

