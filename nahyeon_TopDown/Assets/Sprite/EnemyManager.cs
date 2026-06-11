using UnityEngine;
using UnityEngine.SceneManagement;

// =================================================================
// 1. 메인 클래스: 스폰 및 20초 타이머 관리 (Hierarchy의 스포너 오브젝트에 넣는 스크립트)
// =================================================================
public class EnemyManager : MonoBehaviour
{
    [Header("스포너 설정")]
    public GameObject enemyPrefab;    // 적 외형 프리팹 (인스펙터에서 꼭 드래그해서 넣어주세요!)
    public float spawnInterval = 20f; // 적 생성 주기 (20초)
    public float distanceX = 5f;      // 플레이어 기준 오른쪽으로 떨어질 거리

    private GameObject currentEnemy;  // 현재 맵에 존재하는 적
    private float timer;

    void Start()
    {
        timer = 0f; // 게임 시작하자마자 첫 번째 적이 바로 나오도록 설정
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            // 1. 기존 적이 있다면 삭제
            if (currentEnemy != null)
            {
                Destroy(currentEnemy);
            }

            // 2. 새 적 생성
            SpawnNewEnemy();

            // 3. 타이머 리셋
            timer = spawnInterval;
        }
    }

    void SpawnNewEnemy()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPosition = transform.position;

        if (player != null)
        {
            // 플레이어의 현재 위치에서 오른쪽(X축)으로 distanceX 만큼 떨어진 위치 계산
            spawnPosition = player.transform.position + new Vector3(distanceX, 0f, 0f);
        }

        // 적 프리팹을 맵에 생성
        currentEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // ⭐ 생성된 적에게 아래에 있는 EnemyBehavior 컴포넌트를 코드로 자동 추가합니다!
        if (currentEnemy.GetComponent<EnemyBehavior>() == null)
        {
            currentEnemy.AddComponent<EnemyBehavior>();
        }
    }
}

// =================================================================
// 2. 서브 클래스: 적의 추적 및 플레이어 충돌 처리 (코드가 자동으로 적에게 붙여줍니다)
// =================================================================
public class EnemyBehavior : MonoBehaviour
{
    public float speed = 3f; // 적 이동 속도
    private Transform playerTransform;

    // 기절 상태 관리를 위해 추가된 변수들입니다.
    private bool isStunned = false;
    private Coroutine stunCoroutine;

    void Start()
    {
        // 생성되자마자 플레이어를 찾아서 타겟으로 설정
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // ⭐ [기절 체크] 얼어붙은 상태라면 플레이어를 쫓아오지 않고 움직임을 멈춥니다!
        if (isStunned) return;

        // 플레이어를 향해 이동
        if (playerTransform != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                speed * Time.deltaTime
            );
        }
    }

    // 플레이어와 부딪히면 게임 재시작
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ⭐ [기절 체크] 적이 얼어있을 때는 부딪혀도 억울하게 죽지 않도록 예외 처리합니다.
        if (isStunned) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("적과 부딪힘! 처음부터 다시 시작합니다.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // =================================================================
    // 기절 핵심 기능: 플레이어가 Head 아이템을 쓰면 실행되는 함수들입니다.
    // =================================================================
    public void Stun(float duration)
    {
        // 이미 기절 중인데 아이템을 또 썼다면, 기존 타이머를 취소하고 새로 10초를 시작합니다.
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    // 10초 동안 적을 실제로 얼리고 멈추게 하는 코루틴 로직
    private System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        Debug.Log(duration + "초 동안 기절 상태가 됩니다.");

        // [시각 효과] 적 스프라이트 색상을 하늘색(얼어붙은 느낌)으로 변경
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null)
        {
            originalColor = sr.color;
            sr.color = new Color(0.4f, 0.75f, 1f); // 얼음 같은 하늘색
        }

        // ⭐ [물리 제어] 최신 유니티 문법에 맞춰 rb.linearVelocity로 수정 완료!
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 인자로 들어온 시간(10초) 동안 이 상태로 대기합니다.
        yield return new WaitForSeconds(duration);

        // [원상 복구] 10초가 지나면 원래 색상과 상태로 돌려놓습니다.
        if (sr != null)
        {
            sr.color = originalColor;
        }
        isStunned = false;
        Debug.Log("기절 상태가 해제되었습니다.");
    }
}