using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using UnityEngine.AI;

namespace Postcarbon {
  public class EnemySpawner: NetworkedBehaviour {
    public GameObject toSpawn;
    public int number = 5;
    [HideInInspector] public List<ulong> players;
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
        players.Add(other.GetComponent<Player>().OwnerClientId);
        SpawnEnemies();
      }
    }

    private void OnTriggerExit(Collider other) {
      if (!IsServer) { return; }
      if (other.tag == "Player") {
        players.Remove(other.GetComponent<Player>().OwnerClientId);
        if (players.Count == 0) { UnspawnEnemies(); }
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
      for (int i = enemies.Count - 1; i >= 0; i--) {
        if (enemies[i]) {
          DestroyEnemy(i);
        }
        spawned = false;
        print("Enemies Unspawned!");
      }
    }

    public void SpawnEnemies() {
      if (!IsServer) { return; }
      for (int i = 0; i < number; i++) {
        Vector3 where = transform.position;
        where.x += Random.Range(-transform.localScale.x / 2, transform.localScale.x / 2);
        where.y += Random.Range(-transform.localScale.y / 2, transform.localScale.y / 2);
        where.z += Random.Range(-transform.localScale.z / 2, transform.localScale.z / 2);
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(where, out myNavHit, 100, -1)) { where = myNavHit.position; }
        GameObject spawn = Instantiate(toSpawn, where, Quaternion.identity);
        spawn.GetComponent<NetworkedObject>().Spawn();
        Pawn pawn = spawn.GetComponent<Pawn>();
        pawn.spawnPoint = transform.position;
        // spawn.transform.SetParent(transform);
        enemies.Add(spawn.GetComponent<NetworkedObject>());
      }
      spawned = true;
      print("Enemies Spawned!");
    }
  }
}

