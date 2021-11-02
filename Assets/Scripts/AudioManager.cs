using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    Queue<int> audioQueue = new Queue<int>();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(ProcessAudioQueue());
    }

    private bool toggleSound = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            toggleSound = !toggleSound;
            audioSource.enabled = toggleSound;
        }
    }

    public void PlayAudio(int clipIndex)
    {
        // Victory jingle plays immediately.
        if (clipIndex == 12)
        {
            InterruptAudio();
            audioSource.clip = SelectAudioClip(12);    
            audioSource.Play();
        }
        else
        {
            audioQueue.Enqueue(clipIndex);
        }
    }

    private IEnumerator ProcessAudioQueue()
    {
        while (audioQueue.Count >= 0)
        {
            if (audioQueue.Count > 0)
            {
                int clipIndex = audioQueue.Dequeue();

                audioSource.clip = SelectAudioClip(clipIndex);
                audioSource.Play();
                
                yield return new WaitForSeconds(SelectAudioClip(clipIndex).length);
            }
            yield return null;
        }        
    }

    public void InterruptAudio()
    {
        audioQueue.Clear();
        audioSource.Stop();
    }

    public AudioClip dieRoll1, dieRoll2, dieRoll3, dieRoll4, dieRoll5;
    public AudioClip buttonPress, coins1, coins2, coins3;
    public AudioClip playKnight, robber, gameWon;
    public AudioClip build1, build2, build3;
    public AudioClip cards1, cards2, cards3, yourTurn;
    public AudioClip longestRoad, largestArmy, harbormaster;
    public AudioClip coinSingle1, coinSingle2, coinSingle3;
    public AudioClip monopoly;


    private AudioClip SelectAudioClip(int clipIndex)
    {
        switch (clipIndex)
        {
            case 1: return dieRoll1;
            case 2: return dieRoll2;
            case 3: return dieRoll3;
            case 4: return dieRoll4;
            case 5: return dieRoll5;

            case 6: return buttonPress;
            case 7: return coins1;
            case 8: return coins2;
            case 9: return coins3;

            case 10: return playKnight;
            case 11: return robber;
            case 12: return gameWon;

            case 13: return build1;
            case 14: return build2;
            case 15: return build3;

            case 16: return cards1;
            case 17: return cards2;
            case 18: return cards3;
            case 19: return yourTurn;

            case 30: return longestRoad;
            case 31: return largestArmy;
            case 32: return harbormaster;

            case 33: return coinSingle1;
            case 34: return coinSingle2;
            case 35: return coinSingle3;

            case 36: return monopoly;

        }

        Debug.LogWarning("No matching audio clip found, setting to null.");
        return null;
    }


    



}
