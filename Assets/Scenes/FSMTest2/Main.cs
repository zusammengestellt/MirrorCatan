using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    TurnBasedFsm fsm = new TurnBasedFsm();
    BaseState lastState = null;

    private void Start()
    {
        Utility.ClearLogConsole();

        Debug.Log("Main Start()");
        fsm.Initialize();

    }

    private void Update()
    {

        if (lastState != fsm.Current)
            Debug.Log(fsm.Current);
        
        lastState = fsm.Current;

        if (Input.GetKeyDown(KeyCode.Space))
            fsm.PopState();
    }
    
}
