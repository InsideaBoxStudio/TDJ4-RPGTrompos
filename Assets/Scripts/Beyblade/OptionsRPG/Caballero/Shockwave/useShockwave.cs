using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class useShockwave : MonoBehaviour
{
    [SerializeField] private GameObject shockwave;
    [SerializeField] private GameObject[] players;
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private EnergyCounter energyCounter;

    [Header("Stats")]
    [SerializeField] private float moveDuration = 5f;
    [SerializeField] private int energyCost = 1; //costo de energia

    private CheckPlayerTurn rpgTurn;
    private GameObject shockwaveInstantiate;

    private void Awake()
    {
        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        rpgTurn = players[playerIndex].GetComponent<CheckPlayerTurn>();
    }

    void Update()
    {
        if (!rpgTurn.isTurnActive) return; // actualizar estado del turno

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].rightShoulder.wasPressedThisFrame)
        {
            if (energyCounter.currentEnergy < energyCost) return; // verificar energia suficiente
            energyCounter.ChangeEnergy(-energyCost, "Shockwave");
            rpgTurn.PlayerChoseAnAction(moveDuration, 1000f, false);

            shockwaveInstantiate = Instantiate(shockwave);
            shockwaveInstantiate.transform.SetParent(players[playerIndex].transform);
            shockwaveInstantiate.GetComponent<Shockwave>().playerIndex = playerIndex;
        }
    }
}
