using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Analytics;

public class LevelComplete : MonoBehaviour
{
    public string analyticsEventName = "LevelComplete";
    public bool ActivationAnalytics = false;
    public bool ActivationDebugLog = true;

    [SerializeField] private EndGame endGameScript;

    private bool levelComplete = false;
    private bool trigger = false;

    private bool win1 = false;
    private bool win2 = false;
    private string loseCause = "";
    public float time = 0f;

    // Update is called once per frame
    void Update()
    {

        if (!trigger)
        {
            time += Time.unscaledDeltaTime; // Tiempo de juego real (no usar DeltaTime para que no lo afecten las pausas al elegir acciones)

            if (levelComplete)
            {
                trigger = true;

                if (ActivationAnalytics)
                {
                    if (!PlayerPrefs.HasKey("AnalyticsConsent") || PlayerPrefs.GetInt("AnalyticsConsent") == 0) return; // si los usuarios aceptaron el consentimiento de analytics
                }

                win1 = endGameScript.playerID == 0;
                win2 = endGameScript.playerID == 1;
                loseCause = endGameScript.loseCause;

                SendAnalytics();
                DebugAnalytics();
            }
        }

        levelComplete = endGameScript.gameFinished;
    }

    private void SendAnalytics()
    {
        if (!ActivationAnalytics) return; // si las pruebas de analytics no estan activas, retornar sin hacer nada.
        // aqui enviar el evento de analytics
        CustomEvent levelStartEvent = new CustomEvent(analyticsEventName)
        {
            { "win1", win1 },
            { "win2", win2 },
            { "time", time },
            { "loseCause", loseCause },
        };

        AnalyticsService.Instance.RecordEvent(levelStartEvent);
        AnalyticsService.Instance.Flush();
    }

    private void DebugAnalytics() // imprimir los datos en consola
    {
        if (!ActivationDebugLog) return;

        Debug.Log(analyticsEventName);
        Debug.Log("Win1: " + win1);
        Debug.Log("win2: " + win2);
        Debug.Log("time: " + time);
        Debug.Log("loseCause: " + loseCause);
    }
}