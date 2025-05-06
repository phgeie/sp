using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 5f;  // Speed at which the player moves
    private float rotationSpeed = 100f;  // Speed at which the player rotates
    private float cellSize = 0.5f;   // The size of each cell in the maze

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
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotationInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
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
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 targetPosition = transform.position + transform.forward * cellSize;

            // Check if the next position is blocked
            if (!IsBlocked(transform.position, 1, 0.5f))
            {
                // If not blocked, move the player forward
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        // Handle backward movement (S or Down Arrow)
        else if (Input.GetKey(KeyCode.S))
        {
            Vector3 targetPosition = transform.position - transform.forward * cellSize;

            // Check if the next position is blocked
            if (!IsBlocked(transform.position, 2, 0.5f))
            {
                // If not blocked, move the player backward
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        // Handle backward movement (S or Down Arrow)
        else if (Input.GetKey(KeyCode.A))
        {

            Vector3 targetPosition = transform.position - transform.right * cellSize;
            // Check if the next position is blocked
            if (!IsBlocked(transform.position, 3, 0.5f))
            {
                // If not blocked, move the player backward
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        // Handle backward movement (S or Down Arrow)
        else if (Input.GetKey(KeyCode.D))
        {
            Vector3 targetPosition = transform.position + transform.right * cellSize;

            // Check if the next position is blocked
            if (!IsBlocked(transform.position, 4, 0.5f))
            {
                // If not blocked, move the player backward
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
    }

    bool IsBlocked(Vector3 target, int direction, float val)
    {
        // Perform a raycast in the direction the player is facing (forward or backward)
        RaycastHit hit;
        Vector3 dir;
        if (direction == 1){
            dir = transform.forward;
        }else if(direction == 2){
            dir = -transform.forward;
        }else if(direction == 3){
            dir = -transform.right;
        }else{
            dir = transform.right;
        }
        
        Debug.DrawRay(target, dir * val, Color.white, 0.0f, true); 
        if (Physics.Raycast(target + Vector3.up * 0.5f, dir, out hit, val))
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


