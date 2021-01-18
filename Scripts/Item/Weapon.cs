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
    public NetworkedVarULong owner = new NetworkedVarULong(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);
    [HideInInspector] public Pawn pawn;
    [HideInInspector] public dWeapon dWeapon;
    [HideInInspector] public List<Pawn> hitPawns;

    public override void NetworkStart() {
      base.NetworkStart();
      NetworkedObject nObject = GetComponent<NetworkedObject>();
      if (nObject.IsSceneObject == true) { return; }
      if (owner.Value != 0) {
        NetworkedObject actor = GetNetworkedObject(owner.Value);
        pawn = actor.GetComponent<Pawn>();
        if (nObject.GetComponent<Collider>()) {
          nObject.GetComponent<Collider>().isTrigger = true;
          if (actor.GetComponent<Collider>()) {
            Physics.IgnoreCollision(nObject.GetComponent<Collider>(), GetNetworkedObject(owner.Value).GetComponent<Collider>());
          }
        }
      }
    }

    void Awake() {
      dWeapon = dItem as dWeapon;
    }

    private void OnTriggerEnter(Collider other) {
      if (pawn.attacking) {
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
          if (other.GetComponent<Node>()) {
            other.GetComponent<Node>().TakeDamage(hero);
          }
        }
      }
    }
  }
}