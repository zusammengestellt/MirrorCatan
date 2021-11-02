using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    public Text menu;
    public RawImage menu_bg;

    public Text sidebar;
    public RawImage sidebar_bg;

    private GameManager gm;

    private bool menuToggle = false;

    // Start is called before the first frame update
    void Start()
    {
        menu_bg = GetComponentInChildren<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gm == null)
        {
            if (GameObject.FindGameObjectWithTag("GameController") != null)
                gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
            return;
        }


        // Sidebar
        sidebar.enabled = sidebar_bg.enabled = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //sidebar.text = 
            Corner c = GameBoard.CornerUnderMouse()?.GetComponent<CornerComponent>().corner;
            Hex h = GameBoard.HexUnderMouse()?.GetComponent<HexComponent>().hex;

            if (c != null)
                sidebar.text = $"Corner {c.idNum}: owned {c.owned}, {c.playerOwner}\nisHarbor: {c.isHarbor}, type: {c.harborType.ToString()}";
            else if (h != null)
                sidebar.text = $"Hex {h.id}: resource: {h.resource}, roll: {h.roll}\nrobbed: {h.robbed}";
        }


        // Menu
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.O))
            menuToggle = !menuToggle;
        
        menu_bg.enabled = menuToggle;
        menu.enabled = menuToggle;

        if (!menuToggle) { return; }

        ProcessInput();

        string newText = $"Current turn: {gm.playerNames[gm.currentTurn]} / Player {gm.currentTurn}\n";

        newText += $"\nGame State {gm.GameState.ToString()} / Setup: {gm.setup}\n";
        
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

        newText += "\nstillToDiscard\n";

        for (int i = 1; i <= GameManager.playerCount; i++)
        {
            newText += $"{i}: {gm.stillToDiscard[i]}\n";
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
        newText += "V/B: add/remove victory points";
        menu.text = newText;

    }

    private int selectedPlayer = 1;

    private void ProcessInput()
    {
        // Shift+P: force idle
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            gm.CmdDebugForceIdle();

        // Shift+Y: next turn
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Y))
            gm.CmdRequestNextTurn();

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
        
        if (Input.GetKeyDown(KeyCode.V))
            gm.CmdAddDevCard(selectedPlayer, Dev.VP);
        if (Input.GetKeyDown(KeyCode.B))
            gm.CmdRemoveDevCard(selectedPlayer, Dev.VP);

    }
}

public class Utility
{
    public static void playerIdsAudit(GameManager gm)
    {
        Debug.Log("playerIds audit");
        Debug.Log(gm.playerIds);
        Debug.Log(gm.playerIds.Count);

        for (int i = 1; i <= GameManager.playerCount; i++)
            Debug.Log($"{i}: {gm.playerIds[i]}");
    }
}