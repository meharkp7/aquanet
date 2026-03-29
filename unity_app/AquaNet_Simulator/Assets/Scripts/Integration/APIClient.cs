using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class APIClient : MonoBehaviour
{
    public string apiUrl = "http://127.0.0.1:8000/run";
    public float refreshInterval = 5f;

    public Action<string> OnResponse;

    void Start()
    {
        StartCoroutine(PollAPI());
    }

    IEnumerator PollAPI()
    {
        while (true)
        {
            yield return Request();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    IEnumerator Request()
    {
        UnityWebRequest req = UnityWebRequest.Get(apiUrl);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("📡 API Response");

            OnResponse?.Invoke(req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ API Error: " + req.error);
        }
    }
}