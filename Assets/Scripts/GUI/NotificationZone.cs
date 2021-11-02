using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationZone : MonoBehaviour
{
    public GameManager gm;
    public GameObject notificationPrefab;
    public GameObject previousNotification;

    private void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        previousNotification = null;
    }

    private void OnEnable()
    {
        GameManager.onNotification += ShowNotification;
        GameManager.onNotificationClear += ClearNotification;
    }

    private void ShowNotification(NotificationMessage notification)
    {
        if (previousNotification != null)
            Destroy(previousNotification);

        GameObject newNotification = Instantiate(notificationPrefab, this.transform);
        newNotification.GetComponent<Notification>().ParseMessage(notification);

        previousNotification = newNotification;
    }

    private void ClearNotification()
    {
        if (previousNotification != null && !previousNotification.GetComponent<Notification>().isTemporary)
        {
            Destroy(previousNotification);
            previousNotification = null;
        }

    }
}
