using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

namespace SRPG {
  [System.Serializable]
  public class SunRotation: NetworkedBehaviour {
    public float secondsPerMinute = 1;
    [Range(0, 24)] public float timeOfDay = 8;
    [HideInInspector] public float secondsPerHour;
    [HideInInspector] public float secondsPerDay;

    void Start() {
      secondsPerHour = secondsPerMinute * 60;
      secondsPerDay = secondsPerHour * 24;
    }

    void FixedUpdate() {
      SunUpdate();
    }

    public void SunUpdate() {
      if (timeOfDay < 4 || timeOfDay > 20) {
        timeOfDay += Time.deltaTime / (secondsPerHour * 0.1f);
        Light light = GetComponent<Light>();
        if (timeOfDay > 20) { light.intensity = 2 - ((timeOfDay % 20) / 2); }
        else { light.intensity = timeOfDay/2; }
      }
      else { timeOfDay += Time.deltaTime / secondsPerHour; GetComponent<Light>().enabled = true; }
      timeOfDay %= 24;
      transform.localRotation = Quaternion.Euler(((timeOfDay - 6) / 24) * 360 - 0, -30, 0);
    }
  }
}
