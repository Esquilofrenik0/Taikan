using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace Postcarbon {
  public class ASlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Transform slotIcon;
    [HideInInspector] public dArmor dArmor;
    [HideInInspector] public bool empty = true;
    [HideInInspector] public Sprite background;
    [HideInInspector] public Image bgImage;
    [HideInInspector] public Hero hero;
    [HideInInspector] public int number;

    void Start() {
      bgImage = GetComponent<Image>();
      background = slotIcon.GetComponent<Image>().sprite;
    }

    public void UpdateSlot() {
      if (hero.equipment.armor[number]) {
        dArmor = hero.equipment.armor[number];
        slotIcon.GetComponent<Image>().sprite = dArmor.icon;
        empty = false;
      }
      else {
        dArmor = null;
        slotIcon.GetComponent<Image>().sprite = background;
        empty = true;
      }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      if (!hero) { return; }
      if (hero.equipment.armor[number]) {
        dArmor = hero.equipment.armor[number];
        int slot = hero.equipment.ArmorSlot(dArmor);
        hero.hud.WriteWorldInfo("Unequipped " + dArmor.name);
        hero.equipment.UnequipArmor(slot);
      }
      UpdateSlot();
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (hero.equipment.armor[number]) {
        dArmor = hero.equipment.armor[number];
        hero.hud.DisplayItemInfo(dArmor);
      }
      UpdateSlot();
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetInfo();
    }
  }
}