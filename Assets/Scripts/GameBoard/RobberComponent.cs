using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RobberComponent : NetworkBehaviour
{
    private bool isDragging = false;
    private Vector3 startPosition;

    public void OnStartDrag()
    {
        //if (!hasAuthority) return;
        
        isDragging = true;
        startPosition = transform.position;    
    }

    public void OnStopDrag()
    {
        //if (!hasAuthority) return;
        
        isDragging = false;
        startPosition = transform.position;

        // if (isValidSpot)
        /*if (true)
        {
            
        }
        else
        {
            transform.position = startPosition;
        }*/
    }

    void Update()
    {
        if (isDragging)
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            Vector3 currentPosition;

            if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, (1 << 8)))
            {
                currentPosition = hitInfo.point;
                transform.position = currentPosition;
            }
            
        }
    }
}