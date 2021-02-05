using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  public class FieldOfView: MonoBehaviour {
    public NPC npc;
    private void OnTriggerEnter(Collider other) {
      if (other.GetComponent<Pawn>() && other.GetComponent<Pawn>().faction != npc.pawn.faction) {
        npc.enemy.Add(other.GetComponent<Pawn>());
      }
    }

    private void OnTriggerExit(Collider other) {
      if (other.GetComponent<Pawn>() && other.GetComponent<Pawn>().faction != npc.pawn.faction) {
        npc.enemy.Remove(other.GetComponent<Pawn>());
      }
    }
  }
}