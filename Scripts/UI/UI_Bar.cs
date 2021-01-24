using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

namespace Postcarbon {
  public class UI_Bar: NetworkedBehaviour {
    public Text txt;
    public Slider slider;

    void Awake() {
      slider.minValue = 0;
      slider.maxValue = 1;
    }

    public void SetPercent(float percent) {
      slider.value = percent;
      txt.text = (Mathf.RoundToInt(percent * 100).ToString()) + "/100";
    }
  }
}