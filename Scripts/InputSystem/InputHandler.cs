using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MLAPI;

namespace SRPG {
  public class InputHandler: NetworkedBehaviour {
    public Player player;
    [HideInInspector] public InputMaster controls;
    [HideInInspector] public Vector2 movement = Vector2.zero;
    [HideInInspector] public Vector2 camvect = Vector2.zero;
    [HideInInspector] public bool firstPerson = false;
    [HideInInspector] public bool jump = false;
    [HideInInspector] public bool dodge = false;
    [HideInInspector] public bool attack = false;
    [HideInInspector] public bool block = false;
    [HideInInspector] public bool menu = false;
    [HideInInspector] public bool inventory = false;
    [HideInInspector] public bool sprint = false;
    [HideInInspector] public bool equip = false;
    [HideInInspector] public bool crouch = false;
    [HideInInspector] public bool interact = false;

    void Update() {
      if (!IsLocalPlayer) { return; }
      movement = controls.Player.Movement.ReadValue<Vector2>();
      camvect = controls.Player.Camera.ReadValue<Vector2>();
      if (controls.Player.Attack.ReadValue<float>() == 0) { attack = false; } else { attack = true; }
      if (controls.Player.Block.ReadValue<float>() == 0) { block = false; } else { block = true; }
      if (controls.Player.Sprint.ReadValue<float>() == 0) { sprint = false; } else { sprint = true; }
      if (controls.Player.Crouch.ReadValue<float>() == 0) { crouch = false; } else { crouch = true; }
    }

    public void OnEnable() {
      if (controls == null) { controls = new InputMaster(); }
      controls.Player.Enable();
    }

    public void OnDisable() {
      controls.Player.Disable();
    }

    void OnChangeView() {
      if (!IsLocalPlayer) { return; }
      if (firstPerson) {
        firstPerson = false;
        player.vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.x = 0.25f;
        player.vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = 4;
      }
      else {
        firstPerson = true;
        player.vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().ShoulderOffset.x = 0;
        player.vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = 0;
      }
    }

    void OnJump() {
      if (!IsLocalPlayer) { return; }
      jump = true;
    }

    void OnDodge() {
      if (!IsLocalPlayer) { return; }
      dodge = true;
    }

    void OnEquip() {
      if (!IsLocalPlayer) { return; }
      equip = true;
    }

    void OnMenu() {
      if (!IsLocalPlayer) { return; }
      menu = true;
    }

    void OnInventory() {
      if (!IsLocalPlayer) { return; }
      inventory = true;
    }

    void OnInteract() {
      if (!IsLocalPlayer) { return; }
      interact = true;
    }
  }
}
