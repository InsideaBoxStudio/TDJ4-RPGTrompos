using UnityEngine;
using UnityEngine.UI;

public class StatisticsEnergy : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;

    [SerializeField] private GameObject[] energyObject;
    [SerializeField] private GameObject energyPrefab;

    [SerializeField] private Color InitColor = Color.black;
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color inactiveColor = Color.green;

    [SerializeField] private int initEnergy = 0;
    [SerializeField] private int activeEnergy = 0;
    [SerializeField] private int maxEnergy = 16;

    private void Start()
    {
        // Si no tiene hijos, los crea
        if (transform.childCount == 0)
        {
            energyObject = new GameObject[maxEnergy];

            for (int i = 0; i < maxEnergy; i++)
            {
                energyObject[i] = Instantiate(energyPrefab, transform);
            }
        }
        else
        {
            // Si ya tiene hijos, utiliza los que existen
            energyObject = new GameObject[transform.childCount];

            for (int i = 0; i < transform.childCount; i++)
            {
                energyObject[i] = transform.GetChild(i).gameObject;
            }
        }

        UpdateEnergyUI();
    }

    private void Update()
    {
        initEnergy = PlayerSettings.initEnergy[playerIndex];
        activeEnergy = PlayerSettings.maxEnergy[playerIndex];
        UpdateEnergyUI();
    }

    private void UpdateEnergyUI()
    {
        for (int i = 0; i < energyObject.Length; i++)
        {
            Image image = energyObject[i].GetComponent<Image>();

            if (i < initEnergy)
            {
                image.color = InitColor;
            }
            else if (i < activeEnergy)
            {
                image.color = activeColor;
            }
            else
            {
                image.color = inactiveColor;
            }
        }
    }
}