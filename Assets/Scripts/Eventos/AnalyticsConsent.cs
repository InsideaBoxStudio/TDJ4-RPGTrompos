using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;

public class AnalyticsConsent : MonoBehaviour
{
    public GameObject consentPanel;

    async void Start() //debe ser asincrona para esperar a que se inicialicen los servicios de Unity
    {
        try
        {
            await UnityServices.InitializeAsync(); // Inicializar los servicios de Unity

            if (PlayerPrefs.HasKey("AnalyticsConsent")) //comprueba si el usuario ya dio su concentimiento previamente
            {
                bool accepted = PlayerPrefs.GetInt("AnalyticsConsent") == 1;

                EndUserConsent.SetConsentState(new ConsentState
                {
                    AnalyticsIntent = accepted
                        ? ConsentStatus.Granted
                        : ConsentStatus.Denied,

                    AdsIntent = ConsentStatus.Denied
                });

                consentPanel.SetActive(false); // desactivar panel de consentimiento
            }
            else
            {
                consentPanel.SetActive(true); // activar panel de consentimiento si no ha dado su concentimiento
            }
        }
        catch (System.Exception e) //en caso de un error al iniciar los servicios de Unity
        {
            Debug.LogError(e);
        }
    }

    public void ConsentAccepted() // funcion del boton para aceptar el consentimiento
    {
        EndUserConsent.SetConsentState(new ConsentState // establece el estado de consentimiento para analytics en concedido
        {
            AnalyticsIntent = ConsentStatus.Granted,
            AdsIntent = ConsentStatus.Denied
        });

        PlayerPrefs.SetInt("AnalyticsConsent", 1); // guarda el consentimiento del usuario para futuras sesiones
        PlayerPrefs.Save();

        consentPanel.SetActive(false); // desactiva el panel de consentimiento
    }

    public void ConsentDenied() // funcion del boton para negar el consentimiento
    {
        EndUserConsent.SetConsentState(new ConsentState // establece el estado de consentimiento para analytics en denegado
        {
            AnalyticsIntent = ConsentStatus.Denied,
            AdsIntent = ConsentStatus.Denied
        });

        PlayerPrefs.SetInt("AnalyticsConsent", 0); // guarda el consentimiento del usuario para futuras sesiones
        PlayerPrefs.Save();

        consentPanel.SetActive(false); // desactiva el panel de consentimiento
    }
}