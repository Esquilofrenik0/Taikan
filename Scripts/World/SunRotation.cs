using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  [System.Serializable]
  public class SunRotation: MonoBehaviour {
    public GameObject sun;
    public float secondsPerMinute = 1;
    [Range(0, 24)] public float timeOfDay = 12;
    [HideInInspector] public float secondsPerHour;
    [HideInInspector] public float secondsPerDay;

    void Start() {
      secondsPerHour = secondsPerMinute * 60;
      secondsPerDay = secondsPerHour * 24;
    }

    void Update() {
      SunUpdate();
    }

    public void SunUpdate() {
      if (timeOfDay < 4 || timeOfDay > 20) { timeOfDay += Time.deltaTime / (secondsPerHour * 0.1f); }
      else { timeOfDay += Time.deltaTime / secondsPerHour; }
      timeOfDay %= 24;
      sun.transform.localRotation = Quaternion.Euler(((timeOfDay - 6) / 24) * 360 - 0, -30, 0);
    }
  }
}
