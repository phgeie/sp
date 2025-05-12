using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartLightManager : MonoBehaviour
{
    public Transform player;             // Spieler oder Kamera
    private float maxDistance = 4f;      // Ab hier wird Licht deaktiviert
    private float checkInterval = 1f;     // Wie oft pro Sekunde geprüft wird

    private Light[] allLights;

    void Start()
    {
        allLights = FindObjectsOfType<Light>();
        InvokeRepeating(nameof(UpdateLights), 0f, checkInterval);
    }

    void UpdateLights()
    {
        if (allLights.Length <= 1){   
            allLights = FindObjectsOfType<Light>();
            Debug.Log(allLights.Length);
        }
        foreach (Light light in allLights)
        {
            if (light == null) continue;

            float dist = Vector3.Distance(player.position, light.transform.position);
            light.enabled = dist < maxDistance;
        }
    }
}
