using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MLAPI;
using MLAPI.Messaging;

namespace SRPG {
  [System.Serializable]
  public class Hero: Human {
    [Header("Components")]
    public Player player;
    public HUD hud;
    public CharacterController cc;
    [HideInInspector] public string recipeAvatar;

    [Header("Camera")]
    public GameObject camTarget;
    public float rotationSpeed = 1;
    [HideInInspector] public float mouseX = 0;
    [HideInInspector] public float mouseY = 0;

    [Header("Stats")]
    public float maxMana = 100;
    public float maxStamina = 100;
    public float manaRegen = 1;
    public float staminaRegen = 1;
    public float jumpCost = 0;
    public float blockCost = 5;
    public float dodgeCost = 5;
    public float attackCost = 5;
    public float sprintCost = 5;
    [HideInInspector] public float mana = 100;
    [HideInInspector] public float stamina = 100;

    [Header("Inventory")]
    [HideInInspector] public Container container;
    [HideInInspector] public bool containerOpen = false;
    [HideInInspector] public bool inventoryOpen = false;
    [HideInInspector] public GameObject[] iSlot;

    #region Init
    public void LoadAvatar() {
      if (File.Exists(Application.persistentDataPath + "/Avatar.txt")) {
        recipeAvatar = File.ReadAllText(Application.persistentDataPath + "/Avatar.txt");
        avatar.ClearSlots();
        avatar.LoadFromRecipeString(recipeAvatar);
        avatar.name = "Player";
        avatar.BuildCharacter();
      }
    }

    public void Teleport(Vector3 pos) {
      cc.enabled = false;
      transform.position = pos;
      cc.enabled = true;
    }

    public override void Respawn() {
      spawnPoint = GameObject.Find("PlayerSpawner").GetComponent<PlayerSpawner>().spawnPoints[0].position;
      Teleport(spawnPoint);
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
      avatar.ClearSlots();
      if (avatar.activeRace.name == "HumanMaleDCS") {
        avatar.SetSlot("Underwear", "MaleUnderwear");
      }
      else if (avatar.activeRace.name == "HumanFemaleDCS") {
        avatar.SetSlot("Underwear", "FemaleUndies2");
      }
      avatar.BuildCharacter();
      health.Value = maxHealth / 2;
      stamina = maxStamina / 2;
      mana = maxMana / 2;
      grounded = true;
      crouching = false;
      state = pS.Idle;
      // DisableRagdoll();
    }
    #endregion

    #region Actions
    public void Jump() {
      if (grounded) {
        if (state == pS.Idle || state == pS.Block || state == pS.Sprint) {
          if (!StaminaCost(jumpCost)) { return; }
          velocity.y += Mathf.Sqrt(jumpHeight * -gravity);
          anim.SetTrigger("Jump");
        }
      }
    }

    public void Move(float xIn, float yIn) {
      if (inventoryOpen) { xIn = 0f; yIn = 0f; }
      anim.SetFloat("Horizontal", xIn * speed);
      anim.SetFloat("Vertical", yIn * speed);
      direction = new Vector3(xIn, 0f, yIn);
      direction = transform.rotation * direction;
      direction = direction * speed;

      cc.Move(direction * Time.deltaTime);
      if (player.input.jump) { Jump(); player.input.jump = false; }
      velocity.y += gravity * Time.deltaTime;
      if (grounded && velocity.y < 0) { velocity.y = 0f; }
      cc.Move(velocity * Time.deltaTime);

      if (containerOpen) {
        if (Vector3.Distance(transform.position, container.transform.position) > 4) {
          CloseInventory();
        }
      }
    }

    public void Look(float xIn, float yIn, bool firstPerson) {
      if (!inventoryOpen) {
        float rot;
        if (!aiming) { rot = rotationSpeed; }
        else { rot = rotationSpeed * 0.2f; }
        mouseX += xIn * rot;
        mouseY -= yIn * rot;
        mouseY = Mathf.Clamp(mouseY, -60, 60);
      }
      camTarget.transform.rotation = Quaternion.Euler(mouseY, mouseX, 0);
      if (equipment.holstered.Value || direction.sqrMagnitude > 0) {
        transform.rotation = Quaternion.Euler(0, mouseX, 0);
        if (state != pS.Dodge) {
          spineLook.transform.localRotation = Quaternion.Euler(spineLook.transform.localEulerAngles.x, mouseY, spineLook.transform.localEulerAngles.z);
        }
      }
      if (state != pS.Dodge) {
        headLook.transform.localRotation = Quaternion.Euler(0, 0, 0);
        camTarget.transform.position = new Vector3(headLook.transform.position.x, headLook.transform.position.y + 0.05f, headLook.transform.position.z);
      }
    }

    public void Interact() {
      if (containerOpen) {
        CloseInventory();
      }
      else {
        Ray ray = new Ray(camTarget.transform.position, camTarget.transform.rotation * Vector3.forward);
        RaycastHit hit = new RaycastHit();
        Physics.SphereCast(ray, 1, out hit, 5);
        if (hit.collider != null) {
          if (hit.collider.GetComponent<Item>()) {
            if (hit.collider.isTrigger == false) {
              PickupItem(hit.collider.GetComponent<Item>());
            }
          }
          else if (hit.collider.GetComponent<Container>()) {
            if (!hit.collider.GetComponent<Container>().open)
              OpenContainer(hit.collider.GetComponent<Container>());
          }
        }
      }
    }
    #endregion

    #region Stats
    public void DropInventory(Container bag) {
      for (int i = 0; i < 7; i++) {
        if (equipment.equipSlot[i].GetComponentInChildren<Item>()) {
          Item item = equipment.equipSlot[i].GetComponentInChildren<Item>();
          bag.inventory.Store(item.dItem, 1);
          GameObject.Destroy(item.gameObject);
        }
      }
      for (int i = 0; i < inventory.nSlots; i++) {
        if (inventory.slot[i].amount > 0) {
          bag.inventory.StoreStack(inventory.slot[i]);
          inventory.RemoveStack(i);
        }
      }
    }

    public void UpdateStamina(float amount) {
      stamina += amount;
      if (stamina < 0) { stamina = 0; } else if (stamina > maxStamina) { stamina = maxStamina; }
    }

    public bool StaminaCost(float cost) {
      if (stamina - cost >= 0) {
        stamina -= cost;
        return true;
      }
      else { return false; }
    }

    public void UpdateMana(float amount) {
      mana += amount;
      if (mana < 0) { mana = 0; } else if (mana > maxMana) { mana = maxMana; }
    }

    public bool ManaCost(float cost) {
      if (mana - cost >= 0) {
        mana -= cost;
        return true;
      }
      else { return false; }
    }

    public void Regenerate() {
      UpdateHealth(healthRegen * Time.deltaTime);
      UpdateStamina(staminaRegen * Time.deltaTime);
      UpdateMana(manaRegen * Time.deltaTime);
    }
    #endregion

    #region Inventory
    public void ToggleInventory() {
      if (inventoryOpen) { CloseInventory(); } else if (!inventoryOpen) { OpenInventory(); }
    }

    public void OpenInventory() {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;
      inventoryOpen = true;
      hud.inventoryUI.gameObject.SetActive(true);
      hud.Refresh();
      hud.DisplayStats();
    }

    public void CloseInventory() {
      Cursor.visible = false;
      Cursor.lockState = CursorLockMode.Locked;
      if (containerOpen) {
        container.Close();
        container = null;
        containerOpen = false;
        hud.containerUI.SetActive(false);
      }
      inventoryOpen = false;
      hud.inventoryUI.gameObject.SetActive(false);
    }

    public void OpenContainer(Container hitContainer) {
      OpenInventory();
      container = hitContainer;
      container.interactingHero = this;
      container.Open(this);
      hud.containerUI.SetActive(true);
      containerOpen = true;
      for (int i = 0; i < container.inventory.nSlots; i++) {
        hud.cSlot[i].number = i;
        hud.cSlot[i].hero = this;
        hud.cSlot[i].slot = container.inventory.slot[i];
        hud.cSlot[i].UpdateSlot();
      }
      hud.cSlots(container.inventory.nSlots);
    }

    public void PickupItem(Item item) {
      int freeSlot = inventory.FreeSlot();
      if (freeSlot >= 0) {
        inventory.Store(item.dItem, 1);
        item.Despawn();
        hud.Refresh();
      }
    }

    public void ClickItem(dItem dItem, int number) {
      if (containerOpen) {
        int freeSlot = container.inventory.FreeSlot();
        if (freeSlot >= 0) { container.Store(number); }
      }
      else {
        if (dItem.type == iT.Armor || dItem.type == iT.Weapon) {
          equipment.EquipItem(dItem);
          inventory.RemoveStack(number);
        }
      }
      hud.Refresh();
      hud.DisplayStats();
    }
    #endregion
  }
}
