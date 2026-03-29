using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TimelineUI : MonoBehaviour
{
    public Text timelineText;

    Queue<string> logs = new Queue<string>();

    public void Log(string message)
    {
        string entry = System.DateTime.Now.ToString("HH:mm:ss") + "  " + message;

        logs.Enqueue(entry);

        if (logs.Count > 6)
            logs.Dequeue();

        timelineText.text = string.Join("\n", logs);
    }
}