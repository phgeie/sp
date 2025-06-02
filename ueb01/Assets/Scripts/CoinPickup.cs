using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public AudioClip pickupSound;

    private GameObject objectToMove;
    private float moveDistance = 2.1f;
    private float moveSpeed = 1f;
    private bool isCollected = false;
    private bool invoked = false;
    private Vector3 targetPosition;
    private AudioSource audioSource;
    private string exitTag = "Exit"; 

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = pickupSound;
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            isCollected = true;
            PlayerCollect();
        }
    }

    void PlayerCollect(){
        objectToMove = GameObject.FindGameObjectWithTag(exitTag);
            
        targetPosition = objectToMove.transform.position + Vector3.up * moveDistance;

        if (pickupSound != null){
            audioSource.Play();
        }

        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        if (isCollected && objectToMove != null)
        {
            OpenExit();
        } 
    }

    void OpenExit()
    {
       objectToMove.transform.position = Vector3.MoveTowards(
                objectToMove.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

        if (objectToMove.transform.position == targetPosition){
            if (!invoked){
                invoked = true;
                DestroyCoin();
            }
        }
    }

    void DestroyCoin()
    {
        Destroy(gameObject);
    }
}