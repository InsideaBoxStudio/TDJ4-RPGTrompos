using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Services.Analytics;

public class LevelStart : MonoBehaviour
{
    public string analyticsEventName = "LevelStart";
    public string[] analyticsParametersName;

    public List<Transform> players = new List<Transform>();
    public bool ActivationAnalytics = false;
    public bool ActivationDebugLog = true;

    public string[] playerChar = new string[2];

    void Start()
    {
        Debug.Log(Unity.Services.Core.UnityServices.State);

        if (ActivationAnalytics)
        {
            if (!PlayerPrefs.HasKey("AnalyticsConsent") || PlayerPrefs.GetInt("AnalyticsConsent") == 0) return; // si los usuarios aceptaron el consentimiento de analytics
        }
        
        players.AddRange( //buscar a todos los Players en la escena
            GameObject.FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name) // ordenarlos por nombre
            .Select(go => go.transform)
        );

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].gameObject.activeSelf == false) // si el jugador no esta activo, lo elimino de la lista
            {
                players.RemoveAt(i);
                i--;
            }
            else // si el jugador esta activo, guardo el nombre de su personaje como parametro de analytics
            {
                playerChar[i] = players[i].parent.name;
            }
        }

        if (players.Count < 2) return; // si no hay suficientes jugadores

        SendAnalytics();
        DebugAnalytics();
    }

    private void SendAnalytics() // enviar los datos a Unity Analytics
    {
        if (!ActivationAnalytics) return; // si las pruebas de analytics no estan activas, retornar sin hacer nada.
        // aqui enviar el evento de analytics
        CustomEvent levelStartEvent = new CustomEvent(analyticsEventName)
                {
                    { analyticsParametersName[0], playerChar[0] },
                    { analyticsParametersName[1], playerChar[1] }
                };

        AnalyticsService.Instance.RecordEvent(levelStartEvent);
        AnalyticsService.Instance.Flush();
    }

    private void DebugAnalytics() // imprimir los datos en consola
    {
        if (!ActivationDebugLog) return;
        
        Debug.Log(analyticsEventName);
        for (int i = 0; i < analyticsParametersName.Length; i++)
        {
            Debug.Log(analyticsParametersName[i] + ": " + playerChar[i]);
        }
    }
}
