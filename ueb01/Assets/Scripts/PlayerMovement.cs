using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed at which the player moves
    public float rotationSpeed = 100f;  // Speed at which the player rotates
    public float cellSize = 1f;   // The size of each cell in the maze

    private void Update()
    {
        // Handle player rotation with A and D
        HandleRotation();

        // Handle movement forward/backward based on player rotation
        HandleMovement();
    }

    void HandleRotation()
    {
        float rotationInput = 0;

        // Rotate player left with 'A' and right with 'D'
        if (Input.GetKey(KeyCode.A))
        {
            rotationInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rotationInput = 1f;
        }

        // Rotate the player smoothly
        if (rotationInput != 0)
        {
            transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);
        }
    }

    void HandleMovement()
    {
        // Handle forward movement (W or Up Arrow)
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 targetPosition = transform.position + transform.forward * cellSize;

            // Check if the next position is blocked
            if (!IsBlocked(targetPosition))
            {
                // If not blocked, move the player forward
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        // Handle backward movement (S or Down Arrow)
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 targetPosition = transform.position - transform.forward * cellSize;

            // Check if the next position is blocked
            if (!IsBlocked(targetPosition))
            {
                // If not blocked, move the player backward
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
    }

    bool IsBlocked(Vector3 target)
    {
        // Perform a raycast in the direction the player is facing (forward or backward)
        RaycastHit hit;
        if (Physics.Raycast(target + Vector3.up * 0.5f, -transform.forward, out hit, cellSize))
        {
            // If the ray hits something with the "Wall" tag, the move is blocked
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("WALL!");
                return true;
            }
        }
        return false;
    }
}


