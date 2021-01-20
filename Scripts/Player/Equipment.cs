using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Spawning;
using MLAPI.Connection;

namespace SRPG {
  public class Equipment: NetworkedBehaviour {

    public Humanoid humanoid;
    private Database data;
    public NetworkedVarBool holstered = new NetworkedVarBool(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, false);
    public NetworkedVar<GameObject> weapon1 = new NetworkedVar<GameObject>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, null);
    public NetworkedVar<GameObject> weapon2 = new NetworkedVar<GameObject>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, null);
    public List<Transform> weaponSlot = new List<Transform>() { null, null, null, null };
    public List<dItem> equip = new List<dItem>() { null, null, null, null, null, null, null };
    [HideInInspector] public Coroutine holsterRoutine;
    [HideInInspector] public Coroutine refreshRoutine;
    // public NetworkedList<ulong> item = new NetworkedList<ulong>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<ulong> { 0, 0, 0, 0, 0, 0, 0 });
    // public NetworkedList<string> item = new NetworkedList<string>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.Everyone, ReadPermission = NetworkedVarPermission.Everyone, SendTickrate = 0f }, new List<string> { null, null, null, null, null, null, null });


    void Awake() {
      data = GameObject.Find("Database").GetComponent<Database>();
    }

    public override void NetworkStart() {
      base.NetworkStart();
      Awake();
      Dress();
      Timer.rDelay(this, dHolster, 0.05f, holsterRoutine);
      Timer.rDelay(this, humanoid.RefreshStats, 0.1f, refreshRoutine);
    }

    public void dHolster() {
      Holster(holstered.Value);
    }

    public void Holster(bool equipped) {
      nHolster(equipped);
    }

    public void nHolster(bool equipped) {
      if (equipped) {
        if (weapon1.Value) {
          weapon1.Value.transform.SetParent(weaponSlot[0]);
          weapon1.Value.transform.localPosition = Vector3.zero;
          weapon1.Value.transform.localRotation = Quaternion.identity;
        }
        if (weapon2.Value) {
          weapon2.Value.transform.SetParent(weaponSlot[1]);
          weapon2.Value.transform.localPosition = Vector3.zero;
          weapon2.Value.transform.localRotation = Quaternion.identity;
        }
      }
      else {
        if (weapon1.Value) {
          weapon1.Value.transform.SetParent(weaponSlot[2]);
          weapon1.Value.transform.localPosition = Vector3.zero;
          weapon1.Value.transform.localRotation = Quaternion.identity;
        }
        if (weapon2.Value) {
          weapon2.Value.transform.SetParent(weaponSlot[3]);
          weapon2.Value.transform.localPosition = Vector3.zero;
          weapon2.Value.transform.localRotation = Quaternion.identity;
        }
      }
    }

    public void Dress() {
      bool build = false;
      for (int i = 0; i < 7; i++) {
        if (i == 0) { if (equip[0] && weapon1.Value == null) { EquipItem(equip[0]); } }
        else if (i == 1) { if (equip[1] && weapon2.Value == null) { EquipItem(equip[1]); } }
        else if (i > 1) {
          if (equip[i]) {
            dArmor dArmor = equip[i] as dArmor;
            humanoid.avatar.SetSlot(dArmor.armorSlot.ToString(), dArmor.Name);
            build = true;
          }
        }
      }
      if (build) { humanoid.avatar.BuildCharacter(); }
    }

    public void EquipItem(dItem dItem) {
      int slot = GetSlot(dItem);
      ClearSlot(slot, dItem);
      UpdateSlot(slot, dItem);
      if (slot < 2) {
        if (IsServer) { SpawnEquip(slot, dItem.name, OwnerClientId); }
        else { InvokeServerRpc(SpawnEquip, slot, dItem.name, OwnerClientId); }
      }
      Timer.rDelay(this, dHolster, 0.05f, holsterRoutine);
      Timer.rDelay(this, humanoid.RefreshStats, 0.1f, refreshRoutine);
    }

    public void ClearSlot(int slot, dItem dItem) {
      UnequipItem(slot);
      if (slot < 2) {
        dWeapon dWeapon = dItem as dWeapon;
        if (slot == 0) {
          if (dWeapon.weaponSlot == wS.TwoHand) {
            UnequipItem(WeaponSlot(1));
          }
        }
        else if (slot == 1) {
          if (weapon1.Value != null && weapon1.Value.GetComponent<Weapon>().dWeapon.weaponSlot == wS.TwoHand) { UnequipItem(WeaponSlot(0)); }
        }
      }
    }

    public void UpdateSlot(int slot, dItem dItem) {
      if (slot == 0) {
        dWeapon dWeapon = dItem as dWeapon;
        humanoid.anim.SetInteger("Weapon", (int)dWeapon.wType);
      }
      else if (slot == 1) {
        dWeapon dWeapon = dItem as dWeapon;
        if (dWeapon.wType == wT.Shield) { humanoid.anim.SetBool("Shield", true); }
      }
      else {
        dArmor dArmor = dItem as dArmor;
        humanoid.avatar.SetSlot(dArmor.armorSlot.ToString(), dArmor.Name);
        humanoid.avatar.BuildCharacter();
      }
      equip[slot] = dItem;
    }

    [ServerRPC(RequireOwnership = false)]
    public void SpawnEquip(int slot, string name, ulong clientID) {
      dItem dItem = data.GetItem(name);
      NetworkedObject spawn = Instantiate(dItem.resource, weaponSlot[slot]).GetComponent<NetworkedObject>();
      spawn.SpawnWithOwnership(clientID);
      if (slot == 0) { weapon1.Value = GetNetworkedObject(spawn.NetworkId).gameObject; }
      else if (slot == 1) { weapon2.Value = GetNetworkedObject(spawn.NetworkId).gameObject; }
    }

    public void UnequipItem(int slot) {
      if (!equip[slot]) { return; }
      InvokeServerRpc(sUnequipItem, slot);
    }

    [ServerRPC(RequireOwnership = false)]
    public void sUnequipItem(int slot) {
      if (!equip[slot]) { return; }
      if (GetComponent<Inventory>()) { GetComponent<Inventory>().Store(equip[slot], 1); }
      if (slot > 1) {
        dArmor dArmor = equip[slot] as dArmor;
        humanoid.avatar.ClearSlot(dArmor.armorSlot.ToString());
        humanoid.avatar.BuildCharacter();
      }
      else {
        if (slot == 0 && weapon1.Value) {
          humanoid.anim.SetInteger("Weapon", 0);
          weapon1.Value.GetComponent<NetworkedObject>().UnSpawn();
          Destroy(weapon1.Value);
          weapon1.Value = null;
        }
        else if (slot == 1 && weapon2.Value) {
          humanoid.anim.SetBool("Shield", false);
          weapon2.Value.GetComponent<NetworkedObject>().UnSpawn();
          Destroy(weapon2.Value);
          weapon2.Value = null;
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
