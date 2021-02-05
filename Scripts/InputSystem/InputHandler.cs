using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using static UnityEngine.InputSystem.InputAction;

namespace Postcarbon {
  public class InputHandler: NetworkedBehaviour {
    [HideInInspector] public InputMaster controls;
    [HideInInspector] public Vector2 movement = Vector2.zero;
    [HideInInspector] public Vector2 camvect = Vector2.zero;
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

    public void OnEnable() {
      if (controls == null) { controls = new InputMaster(); }
      controls.Player.Enable();
    }

    public void OnDisable() {
      controls.Player.Disable();
    }

    void OnMovement() {
      if (!IsLocalPlayer) { return; }
      movement = controls.Player.Movement.ReadValue<Vector2>();
    }

    void OnCamera() {
      if (!IsLocalPlayer) { return; }
      camvect = controls.Player.Camera.ReadValue<Vector2>();
    }

    void OnBlock() {
      if (!IsLocalPlayer) { return; }
      block = !block;
    }

    void OnAttack() {
      if (!IsLocalPlayer) { return; }
      attack = !attack;
    }

    void OnSprint() {
      if (!IsLocalPlayer) { return; }
      sprint = !sprint;
    }

    void OnCrouch() {
      if (!IsLocalPlayer) { return; }
      crouch = !crouch;
    }
  }
}
