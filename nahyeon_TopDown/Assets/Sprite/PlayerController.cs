using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
        
    public float moveSpeed = 5f;

    public Sprite[] spriteUp;

    public Sprite[] spriteDown;

    public Sprite[] spriteLeft;

    public Sprite[] spriteRight;

    public float frameTime = 0.15f;

    private Rigidbody2D rb;

    private SpriteRenderer sr;

    private Vector2 input;

    private Vector2 velocity;

    private Sprite[] currentSprites;

    private int frameIndex = 0;

    private float timer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        sr.sprite = currentSprites[0];
    }

    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
        velocity = input.normalized * moveSpeed;

        if (input.sqrMangnitude > 0.01f)
        {
            if (Mathf =.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0)
                    ChangeSprites(spriteRight);
            }
        }
    }
   
}
