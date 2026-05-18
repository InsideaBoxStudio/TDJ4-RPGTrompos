using UnityEngine;
using TMPro;
using System.Collections;

public class CountDown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countDownText;
    [SerializeField] private GameObject[] objectsToDestroy; //objetos a eliminar despues de la cuenta regresiva
    [SerializeField] private GameObject[] objectsToActive; //objetos a ocultar despues de la cuenta regresiva
    public int countDownTime = 3;

    void Awake()
    {
        Time.timeScale = 0f;
    }
    
    void Start()
    {
        StartCoroutine(CountDownCoroutine());
    }

    IEnumerator CountDownCoroutine()
    {
        while (countDownTime > 0)
        {
            countDownText.text = countDownTime.ToString();
            yield return new WaitForSecondsRealtime(1f);
            countDownTime--;

            if (countDownTime == 0)
            {
                countDownText.text = "GO!";
            }
        }

        //esperar un segundo antes de ocultar el texto
        yield return new WaitForSecondsRealtime(1f);

        //ocultar texto
        countDownText.text = "";
        //destruir objetos
        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }

        foreach (GameObject obj in objectsToActive)
        {
            obj.SetActive(true);
        }

        //reanudar tiempo
        Time.timeScale = 1f;
    }
}