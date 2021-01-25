using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Spawning;
using MLAPI.Connection;

namespace Postcarbon {
  public class Equipment: NetworkedBehaviour {
    public Humanoid humanoid;
    public List<Transform> weaponSlot = new List<Transform>() { null, null, null, null };
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    public NetworkedVar<ulong> weapon1 = new NetworkedVar<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);
    public NetworkedVar<ulong> weapon2 = new NetworkedVar<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, 0);
    public NetworkedList<string> equip = new NetworkedList<string>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<string> { null, null, null, null, null, null, null });
    [HideInInspector] public Database data;
    [HideInInspector] public Coroutine holsterRoutine;
    [HideInInspector] public Coroutine refreshRoutine;

    void Awake() { data = GameObject.Find("Database").GetComponent<Database>(); }

    public override void NetworkStart() {
      base.NetworkStart();
      Awake();
      init();
      Timer.rDelay(this, dHolster, 0.05f, holsterRoutine);
      Timer.rDelay(this, humanoid.RefreshStats, 0.1f, refreshRoutine);
    }

    public void init() {
      for (int i = 0; i < 7; i++) {
        if (i > 1) { if (equip[i] != null) { DressUp(equip[i]); } }
        else if (i == 0) { if (equip[0] != null && weapon1.Value == 0) { EquipItem(data.GetItem(equip[0])); } }
        else if (i == 1) { if (equip[1] != null && weapon2.Value == 0) { EquipItem(data.GetItem(equip[1])); } }
      }
    }

    public void dHolster() { Holster(holstered.Value); }

    [ServerRPC(RequireOwnership = false)]
    public void Holster(bool equipped) {
      if (IsServer) {
        nHolster(equipped);
        InvokeClientRpcOnEveryone(nHolster, equipped);
      }
      else { InvokeServerRpc(Holster, equipped); }
    }

    [ClientRPC]
    public void nHolster(bool equipped) {
      if (weapon1.Value != 0) {
        NetworkedObject w1 = GetNetworkedObject(weapon1.Value);
        if (equipped) {
          w1.transform.SetParent(weaponSlot[0]);
          w1.transform.localPosition = Vector3.zero;
          w1.transform.localRotation = Quaternion.identity;
        }
        else {
          w1.transform.SetParent(weaponSlot[2]);
          w1.transform.localPosition = Vector3.zero;
          w1.transform.localRotation = Quaternion.identity;
        }
      }
      if (weapon2.Value != 0) {
        NetworkedObject w2 = GetNetworkedObject(weapon2.Value);
        if (equipped) {
          w2.transform.SetParent(weaponSlot[1]);
          w2.transform.localPosition = Vector3.zero;
          w2.transform.localRotation = Quaternion.identity;
        }
        else {
          w2.transform.SetParent(weaponSlot[3]);
          w2.transform.localPosition = Vector3.zero;
          w2.transform.localRotation = Quaternion.identity;
        }
      }
    }

    public void EquipItem(dItem dItem) {
      int slot = GetSlot(dItem);
      ClearSlot(slot, dItem);
      if (slot < 2) {
        if (slot == 0) {
          dWeapon dWeapon = dItem as dWeapon;
          humanoid.anim.SetInteger("Weapon", (int)dWeapon.wType);
        }
        else if (slot == 1) {
          dWeapon dWeapon = dItem as dWeapon;
          if (dWeapon.wType == wT.Shield) { humanoid.anim.SetBool("Shield", true); }
        }
        if (IsServer) { SpawnWeapon(slot, dItem.name, OwnerClientId); }
        else { InvokeServerRpc(SpawnWeapon, slot, dItem.name, OwnerClientId); }
      }
      else { DressUp(dItem.name); }
      equip[slot] = dItem.name;
      Timer.rDelay(this, dHolster, 0.05f, holsterRoutine);
      Timer.rDelay(this, humanoid.RefreshStats, 0.1f, refreshRoutine);
    }

    [ServerRPC(RequireOwnership = false)]
    public void SpawnWeapon(int slot, string name, ulong clientID) {
      dWeapon dWeapon = data.GetItem(name) as dWeapon;
      NetworkedObject spawn = Instantiate(dWeapon.resource, weaponSlot[slot]).GetComponent<NetworkedObject>();
      spawn.SpawnWithOwnership(clientID);
      if (slot == 0) { weapon1.Value = spawn.NetworkId; }
      else if (slot == 1) { weapon2.Value = spawn.NetworkId; }
    }

    [ServerRPC(RequireOwnership = false)]
    public void DressUp(string name) {
      if (IsServer) { InvokeClientRpcOnEveryone(nDressUp, name); }
      else { InvokeServerRpc(DressUp, name); }
    }

    [ClientRPC]
    public void nDressUp(string name) {
      dArmor dArmor = data.GetItem(name) as dArmor;
      humanoid.avatar.SetSlot(dArmor.armorSlot.ToString(), dArmor.Name);
      humanoid.avatar.BuildCharacter();
    }

    [ServerRPC(RequireOwnership = false)]
    public void Undress(int slot) {
      if (IsServer) { InvokeClientRpcOnEveryone(nUndress, slot); }
      else { InvokeServerRpc(Undress, slot); }
    }

    [ClientRPC]
    public void nUndress(int slot) {
      aS armorSlot = (aS)(slot - 2);
      humanoid.avatar.ClearSlot(armorSlot.ToString());
      humanoid.avatar.BuildCharacter();
    }

    public void ClearSlot(int slot, dItem dItem) {
      UnequipItem(slot);
      if (slot < 2) {
        dWeapon dWeapon = dItem as dWeapon;
        if (slot == 0) {
          if (dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(WeaponSlot(1)); }
        }
        else if (slot == 1) {
          if (weapon1.Value != 0 && GetNetworkedObject(weapon1.Value).GetComponent<Weapon>().dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(WeaponSlot(0)); }
        }
      }
    }

    public void UnequipItem(int slot) {
      if (equip[slot] == null) { return; }
      InvokeServerRpc(sUnequipItem, slot);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUnequipItem(int slot) {
      if (equip[slot] == null) { return; }
      dItem dItem = data.GetItem(equip[slot]);
      if (GetComponent<Inventory>()) { GetComponent<Inventory>().Store(dItem, 1); }
      if (slot > 1) { Undress(slot); }
      else {
        if (slot == 0 && weapon1.Value != 0) {
          humanoid.anim.SetInteger("Weapon", 0);
          GameObject toDestroy = GetNetworkedObject(weapon1.Value).gameObject;
          toDestroy.GetComponent<NetworkedObject>().UnSpawn();
          Destroy(toDestroy);
          weapon1.Value = 0;
        }
        else if (slot == 1 && weapon2.Value != 0) {
          humanoid.anim.SetBool("Shield", false);
          GameObject toDestroy = GetNetworkedObject(weapon2.Value).gameObject;
          toDestroy.GetComponent<NetworkedObject>().UnSpawn();
          Destroy(toDestroy);
          weapon2.Value = 0;
        }
      }
      equip[slot] = null;
      Timer.rDelay(this, humanoid.RefreshStats, 0.1f, refreshRoutine);
    }

    public int WeaponSlot(int weaponSlot) {
      int slot = weaponSlot;
      if (weaponSlot == 2 || weaponSlot == 3) { slot = 0; }
      return slot;
    }

    public int ArmorSlot(int armorSlot) {
      int slot = armorSlot + 2;
      return slot;
    }

    public int GetSlot(dItem dItem) {
      int slot = 0;
      if (dItem.type == iT.Weapon) {
        dWeapon weapon = dItem as dWeapon;
        slot = WeaponSlot((int)weapon.weaponSlot);
      }
      else if (dItem.type == iT.Armor) {
        dArmor armor = dItem as dArmor;
        slot = ArmorSlot((int)armor.armorSlot);
      }
      return slot;
    }
  }
}
