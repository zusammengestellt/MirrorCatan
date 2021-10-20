using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RobberComponent : NetworkBehaviour
{    
    public GameObject HexSelector;
    private GameObject currentHexSelector;
    private GameObject animatedHexSelector;
    
    public bool isDragging = false;

    private Vector3 startPosition;
 
    public Hex startHex;
    public Hex location;

    private void Start()
    {

    }

    [Command(requiresAuthority = false)]
    public void CmdFollowRobber(Vector3 currentPosition)
    {
        transform.position = currentPosition;
        RpcFollowRobber(currentPosition);
    }

    [ClientRpc]
    public void RpcFollowRobber(Vector3 currentPosition)
    {
        transform.position = currentPosition;
    }

    private void Update()
    {       
        if (isServer) { return; }
        
        GameManager gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        if (gm.GameState != GameManager.State.ROBBER) { return; }
        if (gm.currentTurn != PlayerController.playerIndex) { return; }


        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        Vector3 currentPosition;

        // When left-mouse is pressed, start drag
        if (Input.GetMouseButtonDown(0))
        {
            if (GameBoard.HexUnderMouse() != null)
                {
                if (Physics.Raycast(mouseRay, out hitInfo, 50000.0f, (1 << 8)))
                {
                    startPosition = hitInfo.point;
                    startHex = GameBoard.HexUnderMouse().GetComponent<HexComponent>().hex;
                    Cursor.visible = false;
                    isDragging = true;
                }
            }
        }

        // While left-mouse is held down dragging, show selectors
        if (Input.GetMouseButton(0) && isDragging)
        {
            if (GameBoard.HexUnderMouse() == null)
                return;
           
            Hex h = GameBoard.HexUnderMouse().GetComponent<HexComponent>().hex;
            
            // Make first if it doesn't exist
            if (currentHexSelector == null)
            {
                currentHexSelector = (GameObject)Instantiate(
                    HexSelector,
                    h.position,
                    Quaternion.identity
                );
            }
            
            // Don't make another if it's at the same location
            else if (currentHexSelector != null)
            {
                if (h.position != currentHexSelector.transform.position)
                {
                    GameObject.Destroy(currentHexSelector);
                }
            }   

        }
        
        // Left-mouse let go; move Robber or process non-movement
        if (!Input.GetMouseButton(0) && isDragging)
        {
            if (GameBoard.HexUnderMouse() != null)
            {
                location = GameBoard.HexUnderMouse().GetComponent<HexComponent>().hex;
            }
            else
            {
                location = startHex;
            }

            Cursor.visible = true;
            isDragging = false;
            
            if (currentHexSelector != null)
                GameObject.Destroy(currentHexSelector);

            // New location chosen
            if (location != startHex)
            {
                startHex = location;
                gm.CmdRequestEndRobber(location.id);
            }

            CmdFollowRobber(transform.position);
        }

        // Right-click to cancel move and reset to startPosition
        if (Input.GetMouseButton(1) && isDragging)
        {
            location = startHex;
            transform.position = startPosition;
            Cursor.visible = true;
            isDragging = false;

            if (currentHexSelector != null)
                GameObject.Destroy(currentHexSelector);

            CmdFollowRobber(transform.position);  
        }

        if (isDragging)
        {
            if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, (1 << 8)))
            {
                currentPosition = hitInfo.point;
                transform.position = currentPosition;
                CmdFollowRobber(currentPosition);
            }
        }


    }
}