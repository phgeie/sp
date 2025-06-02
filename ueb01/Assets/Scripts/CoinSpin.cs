using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    private float RotationSpeed = 50f;
    private float BounceHeight = 0.25f;
    private float BounceSpeed = 2f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        RotateSpin();
    }

    void RotateSpin(){
        // Rotate around Y axis
        transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime, Space.World);

        // Bounce up and down
        float newY = startPos.y + Mathf.Sin(Time.time * BounceSpeed) * BounceHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}