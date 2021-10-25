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
        audioQueue.Enqueue(clipIndex);
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
    public AudioClip cards1, cards2, cards3;
    public AudioClip longestRoad, largestArmy, harbormaster;


    private AudioClip SelectAudioClip(int clipIndex)
    {
        Debug.Log($"in SelectAudioIndex: {clipIndex}");
        switch (clipIndex)
        {
            case 1: return dieRoll1; break;
            case 2: return dieRoll2; break;
            case 3: return dieRoll3; break;
            case 4: return dieRoll4; break;
            case 5: return dieRoll5; break;

            case 6: return buttonPress; break;
            case 7: return coins1; break;
            case 8: return coins2; break;
            case 9: return coins3; break;

            case 10: return playKnight; break;
            case 11: return robber; break;
            case 12: return gameWon; break;

            case 13: return build1; break;
            case 14: return build2; break;
            case 15: return build3; break;

            case 16: return cards1; break;
            case 17: return cards2; break;
            case 18: return cards3; break;

            case 30: return longestRoad; break;
            case 31: return largestArmy; break;
            case 32: return harbormaster; break;

        }

        Debug.LogWarning("No matching audio clip found, setting to null.");
        return null;
    }


    



}
