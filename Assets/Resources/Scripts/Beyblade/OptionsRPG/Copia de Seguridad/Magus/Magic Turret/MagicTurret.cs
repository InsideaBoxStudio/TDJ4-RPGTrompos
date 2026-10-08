using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MagicTurret : MonoBehaviour
{
    [SerializeField] GameObject[] Orbs;
    [SerializeField] float velocity = 1;
    public int playerIndex = 0;

    private GameObject[] players;
    private GameObject otherPlayer;
    private int OrbsCount = 0;

    private void Awake()
    {
        OrbsCount = Orbs.Length;

        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        for (int i = 0; i < players.Length; i++)
        {
            if (i != playerIndex)
            {
                otherPlayer = players[i];
            }
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        if (Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].buttonEast.wasPressedThisFrame)
        {
            OrbsCount--;

            while (OrbsCount >= 0 && Orbs[OrbsCount] == null)
            {
                OrbsCount--;
            }

            if (OrbsCount < 0)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 direccion = (otherPlayer.transform.position - Orbs[OrbsCount].transform.position).normalized;

            Orbs[OrbsCount].transform.SetParent(null);
            Orbs[OrbsCount].GetComponent<ColliderOrbs>().playerIndex = playerIndex;
            Orbs[OrbsCount].GetComponent<Rigidbody2D>().AddForce(direccion * velocity, ForceMode2D.Impulse);

        }
    }
}
