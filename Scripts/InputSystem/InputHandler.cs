using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using MLAPI;

namespace Postcarbon {
  public class InputHandler: NetworkedBehaviour {
    [HideInInspector] public InputMaster controls;
    [HideInInspector] public Vector2 movement = Vector2.zero;
    [HideInInspector] public Vector2 camvect = Vector2.zero;
    [HideInInspector] public Vector2 mousePosition = Vector2.zero;
    [HideInInspector] public bool jump = false;
    [HideInInspector] public bool dodge = false;
    [HideInInspector] public bool attack = false;
    [HideInInspector] public bool block = false;
    [HideInInspector] public bool menu = false;
    [HideInInspector] public bool sprint = false;
    [HideInInspector] public bool equip = false;
    [HideInInspector] public bool crouch = false;
    [HideInInspector] public bool interact = false;
    [HideInInspector] public bool inventory = false;
    [HideInInspector] public bool firstPerson = false;

    void Update() {
      if (!IsLocalPlayer) { return; }
      movement = controls.Player.Movement.ReadValue<Vector2>();
      camvect = controls.Player.Camera.ReadValue<Vector2>();
      mousePosition = controls.Player.MousePosition.ReadValue<Vector2>();
      if (controls.Player.Block.ReadValue<float>() == 0) { block = false; } else { block = true; }
      if (controls.Player.Attack.ReadValue<float>() == 0) { attack = false; } else { attack = true; }
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
  }
}
