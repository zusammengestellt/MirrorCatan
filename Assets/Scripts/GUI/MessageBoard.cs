using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class MessageBoard : MonoBehaviour
{
    private TextMeshProUGUI messageBoard;

    private int maxLines = 6;

    private void OnEnable()
    {
        GameManager.postMessage += PostMessage;
    }

    private void PostMessage(string message)
    {
        messageBoard = this.gameObject.GetComponent<TextMeshProUGUI>();

        string newText = "";
        string line = "";
        
        int numLines = 0;

        StringReader strReader = new StringReader(messageBoard.text);
        
        // Get numlines.
        while (true)
        {
            line = strReader.ReadLine();
            if (line != null)
                numLines++;
            else
                break;
        }

        // Get last 5 lines to reverse.
        strReader = new StringReader(messageBoard.text);
        int i = 0;
        int j = 0;
        string[] oldLines = new string[maxLines];

        while (true)
        {
            line = strReader.ReadLine();
            if (line != null)
            {
                if (i > numLines - maxLines)
                {
                    oldLines[j] = line;
                    j++;
                }
                i++;
            }
            else
                break;

        }

        // Reverse and add message.
        for (int k = 0; k < maxLines; k++)
            newText = newText + oldLines[k] + "\n";
        newText = newText + message;

        messageBoard.text = newText;
    }

}
