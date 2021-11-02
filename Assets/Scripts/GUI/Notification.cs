using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Notification : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Text label;
    public bool isTemporary;

    public float lerpTime = 1f;

    public void ParseMessage(NotificationMessage notification)
    {
        if (PlayerController.playerIndex == notification.targetPlayer)
        {
            if (notification.privateMessage == "")
                Destroy(this.gameObject);

            label.text = notification.privateMessage;
        }
        else
        {
            if (notification.publicMessage == "")
                Destroy(this.gameObject);

            label.text = notification.publicMessage;
        }

        isTemporary = notification.temporary;
        if (isTemporary)
            Destroy(this.gameObject, 7.5f);

        
    }



    /*
    private IEnumerator FadeOutCanvasGroup()
    {
        float timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (percentageComplete < 1.0f)
        {
            timeSinceStarted = Time.time - timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentValue = Mathf.Lerp(1.0f, 0.0f, percentageComplete);
            canvasGroup.alpha = currentValue;

            yield return new WaitForEndOfFrame();
        }
        Destroy(this.gameObject);
    }
    */
}

public struct NotificationMessage
{
    public int targetPlayer;
    public string privateMessage;
    public string publicMessage;

    public bool temporary;
    public float totalDuration;
}
