using UnityEngine;

public class PausePlayer : MonoBehaviour
{
    private float nextTurn = 0;
    public void Pause(float time)
    {
        var gravity = GetComponent<GravityToPoint>();
        var collider = GetComponent<BeybladeCollider>();
        var rb = GetComponent<Rigidbody2D>();
        var turn = GetComponent<CheckPlayerTurn>();

        if (gravity != null) gravity.enabled = false;
        if (collider != null) collider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (turn != null)
        {
            nextTurn = turn.turnTime;
            turn.PlayerChoseAnAction(0.5f, -1f, false);
        }

        if (time <= 0f) return;
        Invoke(nameof(UnPause), time);
    }

    public void UnPause()
    {
        var turn = GetComponent<CheckPlayerTurn>();
        turn.PlayerChoseAnAction(nextTurn, -1f, false);

        var gravity = GetComponent<GravityToPoint>();
        var collider = GetComponent<BeybladeCollider>();
        var rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.simulated = true;
            rb.WakeUp();
        }
        if (gravity != null) gravity.enabled = true;
        if (collider != null) collider.enabled = true;
    }
}