using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkedVar;

namespace Postcarbon {
  [System.Serializable]
  public class Weapon: Item {
    public GameObject fx;
    public AudioSource audioSource;
    [HideInInspector] public Pawn pawn;
    [HideInInspector] public NetworkedVar<ulong> owner = new NetworkedVar<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);
    [HideInInspector] public dWeapon dWeapon;
    [HideInInspector] public List<Collider> hits;

    public override void NetworkStart() {
      base.NetworkStart();
      if (GetComponent<NetworkedObject>() && GetComponent<NetworkedObject>().IsSceneObject == true) { return; }
      if (IsServer) { owner.Value = NetworkingManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject.NetworkId; }
      if (owner.Value != 0) {
        pawn = GetNetworkedObject(owner.Value).GetComponent<Pawn>();
        if (GetComponent<Collider>()) {
          GetComponent<Collider>().isTrigger = true;
          Physics.IgnoreCollision(GetComponent<Collider>(), pawn.GetComponent<Collider>());
        }
      }
      gameObject.layer = 2;
    }

    void Awake() { dWeapon = dItem as dWeapon; }

    private void OnTriggerEnter(Collider other) {
      if (dWeapon.wType != wT.Shield) {
        if (pawn && pawn.attacking) {
          if (pawn.resetAttack) {
            hits.Clear();
            pawn.resetAttack = false;
          }
          if (hits.Contains(other)) { return; }
          if (other.GetComponent<Pawn>()) {
            Pawn hitPawn = other.GetComponent<Pawn>();
            if (hitPawn.state == (int)pS.Block) { pawn.anim.SetTrigger("Impact"); }
            else { hitPawn.TakeDamage(pawn.damage.Value); }
          }
          else if (pawn.GetComponent<Hero>()) {
            Hero hero = pawn.GetComponent<Hero>();
            if (other.GetComponent<Stone>()) { other.GetComponent<Stone>().Pickup(hero); }
            else if (other.GetComponent<Node>()) { other.GetComponent<Node>().TakeDamage(hero); }
            else if (other.GetComponent<TreeNode>()) { other.GetComponent<TreeNode>().TakeDamage(hero); }
          }
          hits.Add(other);
        }
      }
    }
  }
}