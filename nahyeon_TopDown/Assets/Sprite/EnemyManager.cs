using UnityEngine;
using UnityEngine.SceneManagement;

// =================================================================
// 1. 메인 클래스: 스폰 및 20초 타이머 관리 (Hiearchy의 스포너 오브젝트에 넣는 스크립트)
// =================================================================
public class EnemyManager : MonoBehaviour
{
    [Header("스포너 설정")]
    public GameObject enemyPrefab;    // 적 외형 프리팹 (스크립트가 안 붙어있어도 괜찮습니다!)
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

        // ⭐ [핵심] 생성된 적에게 아래에 있는 EnemyBehavior 컴포넌트를 코드로 자동 추가합니다!
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
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("적과 부딪힘! 처음부터 다시 시작합니다.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}