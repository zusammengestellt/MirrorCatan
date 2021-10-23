using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Lobby : MonoBehaviour
{
    public GameObject loginScreen;
    public Text defaultIpAddress;

    public GameObject connectingScreen;
    public GameObject startScreen;

    public GameObject connectedLabel;

    /*
    public GameObject inputFieldName;
    public GameObject inputFieldIP;
    
    public GameObject connectButton;
    */

    public string playerName = "";
    public string ipAddress = "";

    private void Awake()
    {
        ipAddress = defaultIpAddress.text;
    }


    
}
