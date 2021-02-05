using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Spawning;
using MLAPI.Connection;
using MLAPI.Serialization;
using System.IO;

namespace Postcarbon {
  [System.Serializable]
  public class Equipment: NetworkedBehaviour {
    public Humanoid humanoid;
    public List<Transform> weaponSlot = new List<Transform>() { null, null, null, null };
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    public NetworkedList<ulong> weapon = new NetworkedList<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<ulong> { 0, 0 });
    public NetworkedList<dArmor> armor = new NetworkedList<dArmor>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<dArmor> { null, null, null, null, null });
    public NetworkedVar<dItem> testVar = new NetworkedVar<dItem>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, null);


    public List<dItem> initItems;
    [HideInInspector] public Coroutine holsterRoutine;
    [HideInInspector] public Coroutine refreshRoutine;
    [HideInInspector] public Database data;

    public override void NetworkStart() {
      base.NetworkStart();
      data = GameObject.FindObjectOfType<Database>();
      weapon.OnListChanged += weaponChanged;
      armor.OnListChanged += armorChanged;
      if (IsServer) { for (int i = 0; i < initItems.Count; i++) { EquipItem(initItems[i]); } }
      init();
      print(armor.Count);
    }

    void weaponChanged(NetworkedListEvent<ulong> changeEvent) {
      Holster(holstered.Value);
    }

    void armorChanged(NetworkedListEvent<dArmor> changeEvent) {
      if (armor[changeEvent.index] == null) { Undress(changeEvent.index); }
      else { DressUp(armor[changeEvent.index]); }
      humanoid.RefreshStats();
      if (IsLocalPlayer && GetComponent<HUD>() && GetComponent<HUD>().hero) {
        GetComponent<HUD>().Refresh();
        GetComponent<HUD>().DisplayStats();
      }
    }

    public void init() {
      for (int i = 0; i < armor.Count; i++) {
        if (i > 1) { if (armor[i] != null) { DressUp(armor[i]); } }
      }
      Timer.rDelay(this, dHolster, 0.05f, holsterRoutine);
      Timer.rDelay(this, humanoid.RefreshStats, 0.1f, refreshRoutine);
    }

    public void dHolster() { Holster(holstered.Value); }

    [ServerRPC(RequireOwnership = false)]
    public void Holster(bool equipped) {
      if (IsServer) { InvokeClientRpcOnEveryone(nHolster, equipped); }
      else { InvokeServerRpc(Holster, equipped); }
    }

    [ClientRPC]
    public void nHolster(bool equipped) {
      for (int i = 0; i < weapon.Count; i++) {
        if (weapon[i] != 0) {
          NetworkedObject w = GetNetworkedObject(weapon[i]);
          if (equipped) { w.transform.SetParent(weaponSlot[i]); }
          else { w.transform.SetParent(weaponSlot[i + 2]); }
          w.transform.localPosition = Vector3.zero;
          w.transform.localRotation = Quaternion.identity;
        }
      }
    }

    public void ClearSlot(dItem dItem) {
      if (dItem is dArmor) {
        armor[ArmorSlot((dArmor)dItem)] = null;
      }
      else if (dItem is dWeapon) {
        dWeapon dWeapon = (dWeapon)dItem;
        UnequipWeapon(WeaponSlot(dWeapon));
        if (WeaponSlot(dWeapon) == 0) {
          if (dWeapon.weaponSlot == wS.TwoHand) { UnequipWeapon(1); }
          humanoid.anim.SetInteger("Weapon", (int)dWeapon.wType);
        }
        else if (WeaponSlot(dWeapon) == 1) {
          if (weapon[0] != 0 && GetNetworkedObject(weapon[0]).GetComponent<Weapon>().dWeapon.weaponSlot == wS.TwoHand) { UnequipWeapon(0); }
          if (dWeapon is dShield) { humanoid.anim.SetBool("Shield", true); }
        }
      }
    }

    public void EquipItem(dItem dItem) {
      ClearSlot(dItem);
      if (dItem is dArmor) {
        dArmor dArmor = (dArmor)dItem;
        armor[ArmorSlot(dArmor)] = dArmor;
      }
      else if (dItem is dWeapon) {
        dWeapon dWeapon = (dWeapon)dItem;
        if (IsServer) { SpawnWeapon(dWeapon); }
        else { InvokeServerRpc(SpawnWeapon, dWeapon); }
      }
    }

    [ServerRPC(RequireOwnership = false)]
    public void SpawnWeapon(dWeapon dWeapon) {
      NetworkedObject spawn = Instantiate(dWeapon.resource).GetComponent<NetworkedObject>();
      spawn.GetComponent<Weapon>().ownerID = NetworkId;
      spawn.SpawnWithOwnership(OwnerClientId);
      weapon[WeaponSlot(dWeapon)] = spawn.NetworkId;
    }

    public void UnequipWeapon(int slot) {
      InvokeServerRpc(sUnequipWeapon, slot);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUnequipWeapon(int slot) {
      if (weapon[slot] == 0) { return; }
      dWeapon dWeapon = GetNetworkedObject(weapon[slot]).GetComponent<Weapon>().dWeapon;
      if (GetComponent<Inventory>()) { GetComponent<Inventory>().Store(dWeapon, 1); }
      if (slot == 0) { humanoid.anim.SetInteger("Weapon", 0); }
      else if (slot == 1) { humanoid.anim.SetBool("Shield", false); }
      GameObject toDestroy = GetNetworkedObject(weapon[slot]).gameObject;
      Destroy(toDestroy);
      weapon[slot] = 0;
    }

    [ServerRPC(RequireOwnership = false)]
    public void DressUp(dArmor dArmor) {
      if (IsServer) { InvokeClientRpcOnEveryone(nDressUp, dArmor); }
      else { InvokeServerRpc(DressUp, dArmor); }
    }

    [ClientRPC]
    public void nDressUp(dArmor dArmor) {
      humanoid.avatar.SetSlot(dArmor.armorSlot.ToString(), dArmor.name);
      humanoid.avatar.BuildCharacter();
    }

    [ServerRPC(RequireOwnership = false)]
    public void Undress(int slot) {
      if (IsServer) { InvokeClientRpcOnEveryone(nUndress, slot); }
      else { InvokeServerRpc(Undress, slot); }
    }

    [ClientRPC]
    public void nUndress(int slot) {
      aS armorSlot = (aS)slot;
      humanoid.avatar.ClearSlot(armorSlot.ToString());
      humanoid.avatar.BuildCharacter();
    }

    public int WeaponSlot(dWeapon dWeapon) {
      int slot = (int)dWeapon.weaponSlot;
      if (slot == 2 || slot == 3) { slot = 0; }
      return slot;
    }

    public int ArmorSlot(dArmor dArmor) {
      int slot = (int)dArmor.armorSlot;
      return slot;
    }
  }
}
