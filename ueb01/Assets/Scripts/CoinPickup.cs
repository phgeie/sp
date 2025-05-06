using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public AudioClip pickupSound; // Sound to play on pickup
    public GameObject objectToMove; // Object that moves up
    private float moveDistance = 2f; // How far to move the object
    private float moveSpeed = 1f; // Speed of movement

    private bool isCollected = false;
    private bool invoked = false;
    private Vector3 targetPosition;
    private AudioSource audioSource;

    void Start()
    {
        if (objectToMove != null)
        {
            targetPosition = objectToMove.transform.position + Vector3.up * moveDistance;
        }

        // Set up audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = pickupSound;
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            isCollected = true;
            Debug.Log("PICK IT UP");

            if (pickupSound != null)
            {
                audioSource.Play();
            }

            // Disable coin visually but wait to destroy it after sound
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;

        }
    }

    void Update()
    {
        if (isCollected && objectToMove != null)
        {
            objectToMove.transform.position = Vector3.MoveTowards(
                objectToMove.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }
        if (objectToMove.transform.position == targetPosition){
            if (!invoked){
                invoked = true;
                Invoke("DestroyCoin", pickupSound != null ? pickupSound.length : 0f);
            }
        }
    }

    void DestroyCoin()
    {
        Destroy(gameObject);
    }
}

