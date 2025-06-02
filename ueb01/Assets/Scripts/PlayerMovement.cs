using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 2.5f;
    private float rotationSpeed = 50f;
    private float cellSize = 0.5f;

    private void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        float rotationInput = 0;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotationInput = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rotationInput = 1f;
        }

        if (rotationInput != 0)
        {
            transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);
        }
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 targetPosition = transform.position + transform.forward * cellSize;

            if (!IsBlocked(transform.position, 1, 0.5f))
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Vector3 targetPosition = transform.position - transform.forward * cellSize;

            if (!IsBlocked(transform.position, 2, 0.5f))
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Vector3 targetPosition = transform.position - transform.right * cellSize;
            if (!IsBlocked(transform.position, 3, 0.5f))
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Vector3 targetPosition = transform.position + transform.right * cellSize;

            if (!IsBlocked(transform.position, 4, 0.5f))
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
    }

    bool IsBlocked(Vector3 target, int direction, float val)
    {
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
        
        if (Physics.Raycast(target + Vector3.up * 0.5f, dir, out hit, val))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
}