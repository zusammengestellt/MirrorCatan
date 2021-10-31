using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncomeUnit : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Text label;
    public Image icon;

    [Range(0.0f, 10.0f)] public float totalDuration;
    [Range(0.0f, 10.0f)] public float lerpTime;

    private void Start()
    {
        StartCoroutine(FadeOutCanvasGroup());
    }

    private IEnumerator FadeOutCanvasGroup()
    {
        yield return new WaitForSeconds(totalDuration - lerpTime);

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

}
