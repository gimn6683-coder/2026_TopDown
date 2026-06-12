using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro; // ⭐ TextMeshPro 사용을 위해 필수!
using System.Collections; // ⭐ 기절 코루틴(IEnumerator) 사용을 위해 필수!

public class PlayerController : MonoBehaviour
{
    // ==========================================
    // [1] 기존 이동 및 애니메이션 변수
    // ==========================================
    [Header("이동 설정")]
    public float moveSpeed = 8f;

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

    // 플레이어 자체 상태 변수 (스크린샷 내 기절 로직 연동용)
    private bool isStunned = false;

    // ==========================================
    // [2] 인벤토리 시스템 변수
    // ==========================================
    [Header("인벤토리 UI 연결")]
    public Image[] slotIcons;
    public TMP_Text[] amountTexts;
    public GameObject[] highlights;

    private string[] itemNames = new string[4];
    private int[] itemCounts = new int[4];
    private int selectedSlot = 0;

    // ==========================================
    // [3] 아이템 효과 관련 변수들
    // ==========================================
    [Header("아이템 상태 타이머")]
    public bool isInvincible = false;
    private float invincibleTimer = 0f;

    private bool isSpeedBoosted = false;
    private float speedBoostTimer = 0f;

    [Header("우측 상단 목표 수집 UI")]
    public Image goalItemIcon;
    public TMP_Text goalItemText;
    public string goalItemName = "Cross";
    public int targetCount = 5;
    private int currentCount = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        if (sr != null && currentSprites.Length > 0) sr.sprite = currentSprites[0];
    }

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            if (slotIcons[i] != null) slotIcons[i].enabled = false;
            if (amountTexts[i] != null) amountTexts[i].enabled = false;
        }
        SelectSlot(0);
        UpdateGoalUI();
    }

    private void Update()
    {
        // 기절 상태일 때는 입력 및 타이머 처리를 건너뜁니다.
        if (isStunned) return;

        // 1. 인벤토리 조작
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

        // 2. [Water] 무적 시간 카운트다운
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                Debug.Log("무적(Water) 효과가 종료되었습니다.");
            }
        }

        // 3. [Butterfly] 스피드업 시간 카운트다운
        if (isSpeedBoosted)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f)
            {
                isSpeedBoosted = false;
                moveSpeed = 8f;
                velocity = input.normalized * moveSpeed;
                Debug.Log("스피드업(Butterfly) 효과가 종료되었습니다.");
            }
        }

        // 4. 기존 애니메이션 로직
        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            if (sr != null && currentSprites.Length > 0) sr.sprite = currentSprites[frameIndex];
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

            if (sr != null) sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {
        if (isStunned) return;
        if (rb != null) rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        if (isStunned) return;

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
        if (sr != null) sr.sprite = currentSprites[frameIndex];
    }

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

    public void AddItem(string newItemName, Sprite newIcon)
    {
        if (newItemName == goalItemName)
        {
            currentCount++;
            UpdateGoalUI();
            return;
        }

        // 1. 중복 아이템 개수 추가
        for (int i = 0; i < 4; i++)
        {
            if (itemNames[i] == newItemName)
            {
                itemCounts[i]++;
                UpdateUI();
                return;
            }
        }

        // 2. 새 아이템 등록
        for (int i = 0; i < 4; i++)
        {
            if (string.IsNullOrEmpty(itemNames[i]))
            {
                itemNames[i] = newItemName;
                itemCounts[i] = 1;
                if (slotIcons[i] != null)
                {
                    slotIcons[i].sprite = newIcon;
                    slotIcons[i].enabled = true;
                }
                UpdateUI();
                return;
            }
        }
    }

    private void UseItem()
    {
        if (itemCounts[selectedSlot] > 0)
        {
            string currentItem = itemNames[selectedSlot];

            if (currentItem == "Water")
            {
                isInvincible = true;
                invincibleTimer = 10f;
                Debug.Log("Water 사용: 10초 동안 무적 상태입니다!");
            }
            else if (currentItem == "Head")
            {
                StunAllEnemies(10f);
                Debug.Log("Head 사용: 10초 동안 모든 적이 얼어붙습니다!");
            }
            else if (currentItem == "Butterfly")
            {
                isSpeedBoosted = true;
                speedBoostTimer = 10f;
                moveSpeed = 11f;
                velocity = input.normalized * moveSpeed;
                Debug.Log("Butterfly 사용: 10초 동안 이동 속도가 11로 증가합니다!");
            }

            itemCounts[selectedSlot]--;
            if (itemCounts[selectedSlot] <= 0)
            {
                itemNames[selectedSlot] = "";
                if (slotIcons[selectedSlot] != null) slotIcons[selectedSlot].enabled = false;
            }

            UpdateUI();
        }
    }

    private void StunAllEnemies(float duration)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            enemy.SendMessage("Stun", duration, SendMessageOptions.DontRequireReceiver);
        }
    }

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

    private void UpdateGoalUI()
    {
        if (goalItemText != null)
        {
            int remaining = targetCount - currentCount;
            if (remaining < 0) remaining = 0;

            goalItemText.text = remaining.ToString();

            if (remaining == 0)
            {
                goalItemText.text = "탈출 가능!";
                Debug.Log("모든 십자가를 모았습니다. 탈출구로 가세요!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Cross") || collision.CompareTag("Water") ||
            collision.CompareTag("Head") || collision.CompareTag("Butterfly"))
        {
            string itemName = collision.tag;

            // ⭐ [수정된 부분] 자식 오브젝트까지 뒤져서 이미지를 찾아옵니다!
            SpriteRenderer itemSr = collision.GetComponent<SpriteRenderer>();
            if (itemSr == null)
            {
                itemSr = collision.GetComponentInChildren<SpriteRenderer>();
            }

            Sprite itemIcon = itemSr != null ? itemSr.sprite : null;

            // ⭐ [디버그 추가] 만약 이미지를 못 찾았다면 유니티 콘솔창에 경고를 띄워줍니다.
            if (itemIcon == null)
            {
                Debug.LogWarning($"[{itemName}] 아이템의 이미지를 찾을 수 없습니다! 해당 아이템 프리팹에 SpriteRenderer가 있는지 확인해주세요.");
            }

            AddItem(itemName, itemIcon);
            Destroy(collision.gameObject);
        }
    }

    // ==========================================
    // 💥 플레이어 기절/밀림 제어용 코루틴 함수
    // ==========================================
    public IEnumerator StunPlayer(float duration)
    {
        isStunned = true;
        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(duration);

        if (sr != null)
        {
            sr.color = originalColor;
        }
        isStunned = false;
    }
}