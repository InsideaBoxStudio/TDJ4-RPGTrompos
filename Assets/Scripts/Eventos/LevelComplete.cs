using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Analytics;

public class LevelComplete : MonoBehaviour
{
    public string analyticsEventName = "LevelComplete";
    public bool isPractice = true;
    public bool ActivationAnalytics = false;
    public bool ActivationDebugLog = true;

    [SerializeField] private EndGame endGameScript;
    [SerializeField] private EnergyCounter[] energyCounter;

    private bool levelComplete = false;
    private bool trigger = false;

    private bool winAI = false;
    private bool win1 = false;
    private bool win2 = false;
    private string loseCause = "";
    private string mostUsedAttack = "";
    public float time = 0f;

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

                for (int i = 0; i < energyCounter.Length; i++)
                {
                    if (energyCounter[i].transform.gameObject.activeSelf == true) // verificar que el jugador este activo
                    {
                        mostUsedAttack = energyCounter[i].ObtenerAtaqueMasUsado(); // obtener el ataque mas usado del jugador activo
                        break;
                    }
                }

                win1 = endGameScript.playerID == 0; // si el jugador 1 gano = true
                win2 = endGameScript.playerID == 1; // si el jugador 2 gano = true
                loseCause = endGameScript.loseCause; // causa de la derrota

                SendAnalytics(); // enviar eventos a Unity Analytics
                DebugAnalytics(); // imprimir en consola
            }
        }

        levelComplete = endGameScript.gameFinished;
    }

    private void SendAnalytics()
    {
        if (!ActivationAnalytics) return; // si las pruebas de analytics no estan activas, retornar sin hacer nada.
        // aqui enviar el evento de analytics
        if(isPractice)
        {
            CustomEvent levelCompleteEvent = new CustomEvent(analyticsEventName)
            {
                { "winAI", win2 },
                { "time", time },
                { "loseCause", loseCause },
                { "favAtk", mostUsedAttack },
            };
            AnalyticsService.Instance.RecordEvent(levelCompleteEvent);
        }
        else{
            CustomEvent levelCompleteEvent = new CustomEvent(analyticsEventName)
            {
                { "win1", win1 },
                { "win2", win2 },
                { "time", time },
                { "loseCause", loseCause },
                { "favAtk", mostUsedAttack },
            };
            AnalyticsService.Instance.RecordEvent(levelCompleteEvent);
        }

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
        Debug.Log("mostUsedAttack: " + mostUsedAttack);
    }
}