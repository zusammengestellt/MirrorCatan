using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkDebug : MonoBehaviour
{
    public MyNetworkManager myNetworkManager;
    private string debugText = "";

    void Start()
    {
        //myNetworkManager = GameObject.Find("MyNetworkManager").GetComponent<MyNetworkManager>();
    }
    void Update()
    {
        debugText = myNetworkManager.networkAddress;

        GetComponentInChildren<Text>().text = debugText;
    }
}
