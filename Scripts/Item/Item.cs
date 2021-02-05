using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

namespace Postcarbon {
  [System.Serializable]
  public class Item: NetworkedBehaviour {
    public dItem dItem;
    [HideInInspector] public bool spawned = true;

    public void Despawn() {
      InvokeServerRpc(sUnspawn);
      StartCoroutine(Respawn(10));
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUnspawn() {
      InvokeClientRpcOnEveryone(cUnspawn);
    }

    [ClientRPC]
    public void cUnspawn() {
      spawned = false;
      GetComponent<Collider>().enabled = false;
      transform.GetChild(0).gameObject.SetActive(false);
    }

    IEnumerator Respawn(int time) {
      yield return new WaitForSeconds(time);
      InvokeServerRpc(Spawn);
    }

    [ServerRPC(RequireOwnership = false)]
    public void Spawn() {
      InvokeClientRpcOnEveryone(cSpawn);
    }

    [ClientRPC]
    public void cSpawn() {
      spawned = true;
      GetComponent<Collider>().enabled = true;
      transform.GetChild(0).gameObject.SetActive(true);
    }
  }
}