using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;


namespace SRPG {
  [System.Serializable]
  public class Weapon: Item {
    public GameObject fx;
    public AudioSource audioSource;
    [HideInInspector] public Pawn pawn;
    [HideInInspector] public dWeapon dWeapon;
    [HideInInspector] public List<Pawn> hitPawns;

    public override void NetworkStart() {
      base.NetworkStart();
      if (GetComponent<NetworkedObject>() && GetComponent<NetworkedObject>().IsSceneObject == true) { return; }
      pawn = GetComponentInParent<Pawn>();
      if (GetComponent<Collider>()) {
        GetComponent<Collider>().isTrigger = true;
        Physics.IgnoreCollision(GetComponent<Collider>(), pawn.GetComponent<Collider>());
      }
      gameObject.layer = 2;
    }

    void Awake() {
      dWeapon = dItem as dWeapon;
    }

    private void OnTriggerEnter(Collider other) {
      if (dWeapon.wType != wT.Shield && pawn && pawn.attacking) {
        if (pawn.resetAttack) {
          hitPawns.Clear();
          pawn.resetAttack = false;
        }
        if (other.GetComponent<Pawn>()) {
          Pawn hitPawn = other.GetComponent<Pawn>();
          if (hitPawns.Contains(hitPawn)) { return; }
          else if (hitPawn.state == (int)pS.Block) { pawn.anim.SetTrigger("Impact"); }
          else { hitPawn.TakeDamage(pawn.damage.Value); }
          hitPawns.Add(hitPawn);
        }
        else if (pawn.GetComponent<Hero>()) {
          Hero hero = pawn.GetComponent<Hero>();
          if (other.GetComponent<Node>()) { other.GetComponent<Node>().TakeDamage(hero); }
        }
      }
    }
  }
}