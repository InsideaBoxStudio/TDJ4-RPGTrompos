using UnityEngine;

public class CheckPlayerReady : MonoBehaviour
{
    [SerializeField] private GameObject[] readyIndicator;
    [SerializeField] private GameObject TodoListo;

    void Update()
    {
        bool todosListos = true;

        foreach (GameObject indicator in readyIndicator)
        {
            if (indicator == null || !indicator.activeInHierarchy)
            {
                todosListos = false;
                break;
            }
        }

        TodoListo.SetActive(todosListos);
    }
}