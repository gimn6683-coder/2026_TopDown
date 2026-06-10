using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ⭐ UI(Text, Image) 제어를 위해 반드시 필요합니다!

public class PlayerController : MonoBehaviour
{
    // ==========================================
    // [1] 기존 이동 및 애니메이션 변수 (교수님 코드)
    // ==========================================
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

    // ==========================================
    // [2] 인벤토리 및 무적 시스템 변수 (새로 추가됨)
    // ==========================================
    [Header("인벤토리 UI 연결")]
    public Image[] slotIcons;       // 아이템 이미지 4개
    public Text[] amountTexts;      // 숫자 텍스트 4개
    public GameObject[] highlights; // 선택 표시(테두리) 4개

    private string[] itemNames = new string[4];
    private int[] itemCounts = new int[4];
    private int selectedSlot = 0;

    [Header("무적 설정")]
    public bool isInvincible = false;
    private float invincibleTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        sr.sprite = currentSprites[0];
    }

    private void Start()
    {
        // 게임 시작 시 인벤토리 UI를 깔끔하게 비우고 1번 칸을 선택합니다.
        for (int i = 0; i < 4; i++)
        {
            if (slotIcons[i] != null) slotIcons[i].enabled = false;
            if (amountTexts[i] != null) amountTexts[i].enabled = false;
        }
        SelectSlot(0);
    }

    private void Update()
    {
        // ----------------------------------------
        // 1. 인벤토리 조작 (가만히 서있을 때도 작동하도록 최상단에 배치)
        // ----------------------------------------
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                UseItem();
            }
        }

        // ----------------------------------------
        // 2. 무적 시간 카운트다운
        // ----------------------------------------
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                Debug.Log("무적 상태가 끝났습니다!");
            }
        }

        // ----------------------------------------
        // 3. 기존 애니메이션 로직
        // ----------------------------------------
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            sr.sprite = currentSprites[frameIndex];
            return;
        }

        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;

            if (frameIndex >= currentSprites.Length)
            {
                frameIndex = 0;
            }

            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
        velocity = input.normalized * moveSpeed;

        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0) ChangeSprites(spriteRight);
                else ChangeSprites(spriteLeft);
            }
            else
            {
                if (input.y > 0) ChangeSprites(spriteUp);
                else ChangeSprites(spriteDown);
            }
        }
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        if (currentSprites == newSprites) return;

        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        sr.sprite = currentSprites[frameIndex];
    }

    // ==========================================
    // [3] 인벤토리 기능 메서드 (새로 추가됨)
    // ==========================================

    // 슬롯 선택 시 테두리 UI 켜기
    private void SelectSlot(int index)
    {
        selectedSlot = index;
        for (int i = 0; i < 4; i++)
        {
            if (highlights[i] != null)
            {
                highlights[i].SetActive(i == index);
            }
        }
    }

    // 아이템 획득 (ItemPickup 스크립트에서 호출됨)
    public void AddItem(string newItemName, Sprite newIcon)
    {
        // 1. 이미 똑같은 아이템이 있는지 검사
        for (int i = 0; i < 4; i++)
        {
            if (itemNames[i] == newItemName)
            {
                itemCounts[i]++;
                UpdateUI();
                return;
            }
        }

        // 2. 새 아이템이면 빈칸에 넣기
        for (int i = 0; i < 4; i++)
        {
            if (string.IsNullOrEmpty(itemNames[i]))
            {
                itemNames[i] = newItemName;
                itemCounts[i] = 1;
                slotIcons[i].sprite = newIcon;
                slotIcons[i].enabled = true;
                UpdateUI();
                return;
            }
        }
        Debug.Log("인벤토리가 꽉 찼습니다!");
    }

    // 아이템 사용 로직
    private void UseItem()
    {
        if (itemCounts[selectedSlot] > 0)
        {
            if (itemNames[selectedSlot] == "성당")
            {
                isInvincible = true;
                invincibleTimer = 5f;
                Debug.Log("5초 동안 무적입니다!");
            }

            itemCounts[selectedSlot]--; // 개수 차감

            if (itemCounts[selectedSlot] <= 0)
            {
                itemNames[selectedSlot] = "";
                slotIcons[selectedSlot].enabled = false;
            }
            UpdateUI();
        }
    }

    // 화면의 숫자 텍스트 갱신
    private void UpdateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            if (amountTexts[i] != null)
            {
                if (itemCounts[i] > 1)
                {
                    amountTexts[i].text = itemCounts[i].ToString();
                    amountTexts[i].enabled = true;
                }
                else
                {
                    amountTexts[i].enabled = false;
                }
            }
        }
    }
}