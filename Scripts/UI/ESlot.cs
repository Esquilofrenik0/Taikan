using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MLAPI;

namespace Postcarbon {
  public class ESlot: NetworkedBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public Transform slotIcon;
    [HideInInspector] public dItem dItem;
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
      if (number < 2) {
        if (hero.equipment.weapon[number] != 0) { dItem = GetNetworkedObject(hero.equipment.weapon[number]).GetComponent<Weapon>().dItem; }
      }
      else {
        if (hero.equipment.armor[number-2] != null) { dItem = hero.equipment.armor[number-2]; }
      }
      if (dItem != null) {
        slotIcon.GetComponent<Image>().sprite = dItem.icon;
        empty = false;
      }
      else {
        dItem = null;
        slotIcon.GetComponent<Image>().sprite = background;
        empty = true;
      }
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
      if (!hero) { return; }
      if (number < 2) {
        if (hero.equipment.weapon[number] != 0) { dItem = GetNetworkedObject(hero.equipment.weapon[number]).GetComponent<Weapon>().dItem; }
      }
      else {
        if (hero.equipment.armor[number-2] != null) { dItem = hero.equipment.armor[number-2]; }
      }
      if (dItem != null) { hero.equipment.ClearSlot(dItem); }
      UpdateSlot();
    }

    public void OnPointerEnter(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 0, 200);
      if (number < 2) {
        if (hero.equipment.weapon[number] != 0) { dItem = GetNetworkedObject(hero.equipment.weapon[number]).GetComponent<Weapon>().dItem; }
      }
      else {
        if (hero.equipment.armor[number-2] != null) { dItem = hero.equipment.armor[number-2]; }
      }
      if (dItem != null) { hero.hud.DisplayInfo(dItem); }
      UpdateSlot();
    }

    public void OnPointerExit(PointerEventData pointerEventData) {
      if (!hero) { return; }
      bgImage.color = new Vector4(255, 255, 255, 100);
      hero.hud.ResetInfo();
    }
  }
}