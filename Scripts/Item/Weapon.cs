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
    [HideInInspector] public dWeapon dWeapon;
    [HideInInspector] public bool attacking = false;
    [HideInInspector] public bool resetAttack = false;
    [HideInInspector] public List<Pawn> hitPawns;

    void Awake() {
      dWeapon = dItem as dWeapon;
      attacking = false;
      resetAttack = true;
    }

    private void OnTriggerEnter(Collider other) {
      if (attacking) {
        if (resetAttack) { hitPawns.Clear(); resetAttack = false; }
        if (other.GetComponent<Pawn>()) {
          Pawn pawn = other.GetComponent<Pawn>();
          if (hitPawns.Contains(pawn)) { return; }
          pawn.TakeDamage(GetNetworkedObject(owner.Value).GetComponent<Pawn>().damage.Value);
          hitPawns.Add(pawn);
        }
        else if (GetNetworkedObject(owner.Value).GetComponent<Hero>()) {
          Hero hero = GetNetworkedObject(owner.Value).GetComponent<Hero>();
          if (other.GetComponent<Node>()) {
            other.GetComponent<Node>().TakeDamage(hero);
          }
        }
      }
    }

    public override void NetworkStart() {
      base.NetworkStart();
      NetworkedObject nObject = GetComponent<NetworkedObject>();
      if (nObject.IsSceneObject == true) { return; }
      if (nObject.GetComponent<Collider>()) { nObject.GetComponent<Collider>().isTrigger = true; }
      if (IsServer) {
        NetworkedObject player = NetworkingManager.Singleton.ConnectedClients[OwnerClientId].PlayerObject;
        owner.Value = player.NetworkId;
      }
      if (owner.Value != 0) {
        NetworkedObject actor = GetNetworkedObject(owner.Value);
        if (nObject.GetComponent<Collider>() && actor.GetComponent<Collider>()) {
          Physics.IgnoreCollision(nObject.GetComponent<Collider>(), GetNetworkedObject(owner.Value).GetComponent<Collider>());
        }
      }
    }
  }
}