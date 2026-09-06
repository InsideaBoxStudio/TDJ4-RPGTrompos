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
    private bool falling;

    private void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        //Invoke("InitFall", 0.25f);
        StartFall(1f, 2f);
    }

    private void InitFall()
    {
        StartFall(1f, 2f);
    }

    private void Update()
    {
        if (!isJumping) return;

        timer += Time.deltaTime;

        float t = timer / jumpDuration;

        if (t >= 1f)
        {
            transform.localScale = originalScale;

            jumping = false;
            falling = false;
            isJumping = false;

            collider.enabled = true;
            isTouchingFloor = true;
            spriteRenderer.sortingLayerName = "Players";

            return;
        }

        if (jumping)
        {
            UpdateJump(t);
        }
        else if (falling)
        {
            UpdateFall(t);
        }
    }

    private void UpdateJump(float t)
    {
        spriteRenderer.sortingLayerName = "UI";
        isTouchingFloor = false;

        // Parábola: 0 → 1 → 0
        float jumpHeight = 4f * t * (1f - t);

        float scaleMultiplier = Mathf.Lerp(
            1f,
            maxScale,
            jumpHeight
        );

        transform.localScale = originalScale * scaleMultiplier;

        // Mientras está en el aire no tiene collider
        collider.enabled = jumpHeight < 0.2f;
    }

    private void UpdateFall(float t)
    {
        spriteRenderer.sortingLayerName = "UI";
        isTouchingFloor = false;

        // Empieza en maxScale y termina en 1
        float scaleMultiplier = Mathf.Lerp(
            maxScale,
            1f,
            t
        );

        transform.localScale = originalScale * scaleMultiplier;

        // Durante la caída no tiene collider
        collider.enabled = false;
    }

    public void StartJump(float jumpTime, float jumpScale)
    {
        jumpDuration = jumpTime;
        maxScale = jumpScale;

        timer = 0f;

        isJumping = true;
        jumping = true;
        falling = false;

        collider.enabled = true;
    }

    public void StartFall(float fallTime, float startScale)
    {
        jumpDuration = fallTime;
        maxScale = startScale;

        timer = 0f;

        isJumping = true;
        jumping = false;
        falling = true;

        isTouchingFloor = false;
        collider.enabled = false;

        // Comienza inmediatamente escalado
        transform.localScale = originalScale * maxScale;
    }
}