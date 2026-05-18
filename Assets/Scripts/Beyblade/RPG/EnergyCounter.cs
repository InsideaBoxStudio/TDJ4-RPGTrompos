using UnityEngine;
using System.Collections.Generic;

public class EnergyCounter : MonoBehaviour
{
    public List<GameObject> energias = new List<GameObject>();
    public int currentEnergy = 0;

    void Start()
    {
        // Opcional: llenar automáticamente la lista con los hijos
        energias.Clear();
        foreach (Transform hijo in transform)
        {
            energias.Add(hijo.gameObject);
        }

        UpdateBar();
    }

    public void ChangeEnergy(int cantidad)
    {
        currentEnergy += cantidad;

        // Limitar entre 0 y el máximo
        currentEnergy = Mathf.Clamp(currentEnergy, 0, energias.Count);

        UpdateBar();
    }

    void UpdateBar()
    {
        for (int i = 0; i < energias.Count; i++)
        {
            // Activa los primeros "currentEnergy"
            energias[i].SetActive(i < currentEnergy);
        }
    }
}
