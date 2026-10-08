using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] public float duration = 1f; // El punto al que atrae
    [SerializeField] public float gravityStrength; // El punto al que atrae
    public int playerIndex = 0;

    private GameObject[] players;
    private GameObject otherPlayer;

    private Transform startGravityPoint;
    private float startGravityStrength;

    private void Awake()
    {
        gameObject.GetComponent<ParticleSystem>().Play();

        players = GameObject
            .FindGameObjectsWithTag("Player")
            .OrderBy(go => go.name)
            .ToArray();

        players[playerIndex].GetComponent<GravityToPoint>().enabled = false;

        for (int i = 0; i < players.Length; i++)
        {
            if (i != playerIndex)
            {
                startGravityPoint = players[i].GetComponent<GravityToPoint>().gravityPoint;
                startGravityStrength = players[i].GetComponent<GravityToPoint>().gravityStrength;

                players[i].GetComponent<GravityToPoint>().gravityPoint = transform;
                players[i].GetComponent<GravityToPoint>().gravityStrength = gravityStrength;
            }
        }

        Invoke("End", duration);
    }

    private void End()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (i != playerIndex)
            {
                players[i].GetComponent<GravityToPoint>().gravityPoint = startGravityPoint;
                players[i].GetComponent<GravityToPoint>().gravityStrength = startGravityStrength;
            }
        }
        players[playerIndex].GetComponent<GravityToPoint>().enabled = true;

        Destroy(gameObject);
    }
}
