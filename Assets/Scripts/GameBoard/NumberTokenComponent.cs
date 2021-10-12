using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NumberTokenComponent : NetworkBehaviour
{
    public int roll;

    public GameObject camCon;
    
    private Canvas canvas;
    private Text label;
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        // Remove client-only components.
        GetComponent<MeshRenderer>().enabled = false;
        GetComponentInChildren<Canvas>().enabled = false;

    }

    void Start()
    {
        camCon = GameObject.Find("CameraRig");
    }

    void Update()
    {
       // float yRotation = camCon.transform.rotation.eulerAngles.y + 180f;
       // transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public void SetLabel(int newNum)
    {
        roll = newNum;
        
        canvas = GetComponentInChildren<Canvas>();
        label = canvas.GetComponentInChildren<Text>();

        if (label != null)
        {
            label.text = roll.ToString();
            
            if (roll == 6 || roll == 8)
                label.color = Color.red;
        }
        
    }
}
