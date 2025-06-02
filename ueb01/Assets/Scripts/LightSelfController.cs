using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSelfController : MonoBehaviour
{
    private GameObject player;
    public LayerMask obstacleMask;
    private float activationDistance = 10f;
    private float checkInterval = 0.2f;
    private Light lightComponent;

    void Start()
    {
        player = GameObject.Find("Player");
        lightComponent = GetComponent<Light>();
        InvokeRepeating(nameof(UpdateLightState), 0f, checkInterval);
    }

    void UpdateLightState()
    {
        if (player == null || lightComponent == null)
            return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > activationDistance)
        {
            lightComponent.enabled = false;
            return;
        }

        Vector3 direction = (player.transform.position - transform.position).normalized;
        float rayDistance = distance;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayDistance, obstacleMask))
        {
            lightComponent.enabled = false;
        }
        else
        {
            lightComponent.enabled = true;
        }
    }
}