using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace SRPG {
  public class EnemySpawner: NetworkedBehaviour {
    public GameObject toSpawn;
    public int number = 5;
    [HideInInspector] bool spawned = false;
    [HideInInspector] public List<NetworkedObject> enemies;

    public override void NetworkStart() {
      base.NetworkStart();
      if (!IsServer) { return; }
      spawned = false;
    }

    private void OnTriggerStay(Collider other) {
      if (!IsServer) { return; }
      if (other.GetComponent<Player>()) {
        if (!spawned) { SpawnEnemies(); }
        else {
          if (enemies.Count > 0) {
            for (int i = 0; i < enemies.Count; i++) {
              if (enemies[i].GetComponent<Pawn>().state == (int)pS.Dead) {
                enemies.RemoveAt(i);
              }
            }
          }
          else { Timer.Delay(this, ResetSpawn, 10); }
        }
      }
    }

    public void ResetSpawn() {
      spawned = false;
    }

    public void SpawnEnemies() {
      if (!IsServer) { return; }
      for (int i = 0; i < number; i++) {
        NetworkedObject spawn = Instantiate(toSpawn, transform.position, Quaternion.identity).GetComponent<NetworkedObject>();
        if (spawn.GetComponent<Human>()) {
          if (Random.Range(0, 2) < 1) { spawn.GetComponent<Human>().avatar.ChangeRace("HumanMaleDCS"); }
          else { spawn.GetComponent<Human>().avatar.ChangeRace("HumanFemaleDCS"); }
        }
        spawn.transform.localScale = Vector3.one;
        spawn.Spawn();
        enemies.Add(spawn);
      }
      spawned = true;
      print("Enemies Spawned");
    }
  }
}

