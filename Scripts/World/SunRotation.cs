using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPG {
  [System.Serializable]
  public class SunRotation: MonoBehaviour {
    [HideInInspector]public GameObject sun;
    [HideInInspector]public Light sunLight;
    [Range(0, 24)]
    public float timeOfDay = 12;
    public float secondsPerMinute = 1;
    [HideInInspector] public float secondsPerHour;
    [HideInInspector] public float secondsPerDay;

    // public float timeMultiplier = 1;

    void Start() {
      sun = gameObject;
      sunLight = gameObject.GetComponent<Light>();

      secondsPerHour = secondsPerMinute * 60;
      secondsPerDay = secondsPerHour * 24;
    }

    void Update() {
      SunUpdate();
    }

    public void SunUpdate() {
      //30,-30,0 = sunrise
      //90,-30,0 = High noon
      //180,-30,0 = sunset
      //-90,-30,0 = Midnight

      timeOfDay += Time.deltaTime/secondsPerHour;
      timeOfDay %= 20;
      if(timeOfDay<4){sunLight.enabled = false;} 
      else {sunLight.enabled = true;}
      sun.transform.localRotation = Quaternion.Euler(((timeOfDay-6) / 24) * 360 - 0, -30, 0);
      // sun.transform.localEulerAngles = new Vector3(Time.time * timeMultiplier, -30, 0);
    }
  }
}
