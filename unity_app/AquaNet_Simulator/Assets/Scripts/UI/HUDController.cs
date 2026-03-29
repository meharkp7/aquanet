using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Text riskText;
    public Text retryText;
    public Text statusText;

    public void UpdateHUD(float risk, int retries, string status)
    {
        riskText.text = "Risk: " + risk.ToString("F1");
        retryText.text = "Retries: " + retries;
        statusText.text = status;
    }
}