using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    public Text label;
    public RawImage bg;

    private GameManager gm;

    private bool toggle = false;

    // Start is called before the first frame update
    void Start()
    {
        bg = GetComponentInChildren<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.O))
            toggle = !toggle;
        
        bg.enabled = toggle;
        label.enabled = toggle;

        if (!toggle) { return; }

        if (gm == null)
        {
            if (GameObject.FindGameObjectWithTag("GameController") != null)
                gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            return;
        }

        ProcessInput();

        string newText = $"Current turn: {gm.playerNames[gm.currentTurn]} / Player {gm.currentTurn}\n";

        newText += $"\nGame State {gm.GameState.ToString()}\n";
        
        newText += "\nplayerResources\n";

        for (int i = 1; i <= GameManager.playerCount; i++)
        {
            newText += $"{gm.playerNames[i]}/{i}: {gm.playerResources[i].Count}: ";

            foreach (Resource res in gm.playerResources[i])
                newText += $"{res.ToString()} ";
            newText += "\n";
        }

        newText += "\nplayerSelectedCards\n";

        for (int i = 1; i <= GameManager.playerCount; i++)
        {
            newText += $"{i}: {gm.playerSelectedCards[i].Count}: ";

            foreach (Resource res in gm.playerSelectedCards[i])
                newText += $"{res.ToString()} ";
            newText += "\n";
        }

        newText += "\nplayerOfferingTrade\n";

        for (int i = 1; i <= GameManager.playerCount; i++)
        {
            newText += $"{i}: {gm.playerOfferingTrade[i]}\n";
        }        

        newText += $"\nDebug selected player: {selectedPlayer}\n";
        newText += "Shift+P: force idle state.\n";
        newText += "Shift+Y: force next turn.\n";
        newText += "1234: select player.\n";
        newText += "QWERT: add Wood/Brick/Wool/Grain/Ore\n";
        newText += "ASDFG: remove Wood/Brick/Wool/Grain/Ore";
        
        label.text = newText;

    }

    private int selectedPlayer = 1;

    private void ProcessInput()
    {
        // Shift+P: force idle
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            gm.CmdDebugForceIdle();

        // Shift+Y: next turn
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Y))
            gm.CmdRequestNextTurn(gm.currentTurn);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            selectedPlayer = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            selectedPlayer = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            selectedPlayer = 3;
        if (Input.GetKeyDown(KeyCode.Alpha4))
            selectedPlayer = 4;

        if (selectedPlayer < 1 || selectedPlayer > GameManager.playerCount) selectedPlayer = 1;

        if (Input.GetKeyDown(KeyCode.Q))
            gm.CmdAddResource(selectedPlayer, Resource.Wood);
        if (Input.GetKeyDown(KeyCode.W))
            gm.CmdAddResource(selectedPlayer, Resource.Brick);
        if (Input.GetKeyDown(KeyCode.E))
            gm.CmdAddResource(selectedPlayer, Resource.Wool);
        if (Input.GetKeyDown(KeyCode.R))
            gm.CmdAddResource(selectedPlayer, Resource.Grain);
        if (Input.GetKeyDown(KeyCode.T))
            gm.CmdAddResource(selectedPlayer, Resource.Ore);

        if (Input.GetKeyDown(KeyCode.A))
            gm.CmdRemoveResource(selectedPlayer, Resource.Wood);
        if (Input.GetKeyDown(KeyCode.S))
            gm.CmdRemoveResource(selectedPlayer, Resource.Brick);
        if (Input.GetKeyDown(KeyCode.D))
            gm.CmdRemoveResource(selectedPlayer, Resource.Wool);
        if (Input.GetKeyDown(KeyCode.F))
            gm.CmdRemoveResource(selectedPlayer, Resource.Grain);
        if (Input.GetKeyDown(KeyCode.G))
            gm.CmdRemoveResource(selectedPlayer, Resource.Ore);

    }
}
