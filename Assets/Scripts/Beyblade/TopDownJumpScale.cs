using UnityEngine;

public class TopDownJumpScale : MonoBehaviour
{
    [SerializeField] private Collider2D collider;
    public bool isTouchingFloor = true;
    public bool isJumping = false;
    public float jumpDuration = 1f;
    public float maxScale = 2f;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private float timer;
    private bool jumping;

    private void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isJumping) return;

        if (jumping)
        {
            spriteRenderer.sortingLayerName = "UI";
            isTouchingFloor = false;
            timer += Time.deltaTime;

            float t = timer / jumpDuration;

            if (t >= 1f)
            {
                transform.localScale = originalScale;
                jumping = false;
                return;
            }

            // Parábola: 0 → 1 → 0
            float jumpHeight = 4f * t * (1f - t);

            float scaleMultiplier = Mathf.Lerp(1f, maxScale, jumpHeight);

            transform.localScale = originalScale * scaleMultiplier;

            if (jumpHeight >= 0.2)
            {
                collider.enabled = false;
            }
            else
            {
                collider.enabled = true;
            }
        }
        else
        {
            spriteRenderer.sortingLayerName = "Players";
            collider.enabled = true;
            isJumping = false;
            isTouchingFloor = true;
        }
    }

    public void StartJump()
    {
        timer = 0f;
        isJumping = true;
        jumping = true;
    }
}