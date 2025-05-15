using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSelfController : MonoBehaviour
{
    public Transform player;            // Assign player/camera here
    public float activationDistance = 10f;
    public LayerMask obstacleMask;      // LayerMask for walls or occluders
    public float checkInterval = 1f;    // How often the light checks

    private Light lightComponent;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        InvokeRepeating(nameof(UpdateLightState), 0f, checkInterval);
    }

    void UpdateLightState()
    {
        Debug.Log(lightComponent.enabled);
        if (player == null || lightComponent == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > activationDistance)
        {
            lightComponent.enabled = false;
            return;
        }

        // Check if player is visible (no wall in the way)
        Vector3 direction = (player.position - transform.position).normalized;
        float rayDistance = distance;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayDistance, obstacleMask))
        {
            // Wall or obstacle hit
            lightComponent.enabled = false;
        }
        else
        {
            // Clear line of sight
            lightComponent.enabled = true;
        }
    }
}

