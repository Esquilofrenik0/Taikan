using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Postcarbon {
  [System.Serializable]
  public class Melee: Weapon {
    [HideInInspector] public dMelee dMelee;
    [HideInInspector] public List<Collider> hits;

    void Awake() {
      dWeapon = dItem as dWeapon;
      dMelee = dItem as dMelee;
    }

    private void OnTriggerEnter(Collider other) {
      if (pawn && pawn.attacking && pawn.state.Value == (int)pS.Attack) {
        if (pawn.resetAttack) {
          hits.Clear();
          pawn.resetAttack = false;
        }
        if (hits.Contains(other)) { return; }
        if (other.GetComponent<Pawn>()) {
          Pawn hitPawn = other.GetComponent<Pawn>();
          if (hitPawn.state.Value == (int)pS.Block) { pawn.AniTrig("Impact"); }
          else {
            hitPawn.TakeDamage(pawn.damage.Value);
            hitPawn.AniTrig("Impact");
            if (hitPawn.GetComponent<NPC>() && hitPawn.faction != pawn.faction) {
              NPC npc = hitPawn.GetComponent<NPC>();
              npc.enemy.Add(pawn);
            }
          }
        }
        else if (pawn is Hero) {
          Hero hero = pawn as Hero;
          if (other.GetComponent<Stone>()) { other.GetComponent<Stone>().Pickup(hero); }
          else if (other.GetComponent<Node>()) { other.GetComponent<Node>().TakeDamage(hero); }
          else if (other.GetComponent<TreeNode>()) { other.GetComponent<TreeNode>().TakeDamage(hero); }
        }
        hits.Add(other);
      }
    }
  }
}