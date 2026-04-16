using UnityEngine;

/// <summary>
/// 【応用編 模範解答】敵の動作を制御するクラス
/// 応用1（踏みつけで倒される処理）を含む
/// ※ このファイルは模範解答です。Unityプロジェクトには含めません。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField]
    private bool moveRight = true;

    [Header("移動範囲")]
    [SerializeField]
    private bool usePatrol = true;

    [SerializeField]
    private float patrolDistance = 3f;

    [Header("壁検知")]
    [SerializeField]
    private Transform wallCheck;

    [SerializeField]
    private float wallCheckDistance = 0.5f;

    [SerializeField]
    private LayerMask groundLayer;

    // コンポーネント参照
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // パトロール用
    private Vector3 startPosition;
    private float currentDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startPosition = transform.position;
        currentDirection = moveRight ? 1f : -1f;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged")
        {
            gameObject.tag = "Enemy";
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (usePatrol)
        {
            Patrol();
        }

        CheckWall();
        Move();
    }

    /// <summary>
    /// パトロール範囲内で移動する
    /// </summary>
    private void Patrol()
    {
        float distanceFromStart = transform.position.x - startPosition.x;

        if (distanceFromStart > patrolDistance)
        {
            currentDirection = -1f;
        }
        else if (distanceFromStart < -patrolDistance)
        {
            currentDirection = 1f;
        }
    }

    /// <summary>
    /// 壁を検知したら反転する
    /// </summary>
    private void CheckWall()
    {
        Vector2 checkPos;
        if (wallCheck != null)
        {
            checkPos = wallCheck.position;
        }
        else
        {
            checkPos = (Vector2)transform.position + Vector2.right * currentDirection * 0.5f;
        }

        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.right * currentDirection, wallCheckDistance, groundLayer);

        if (hit.collider != null)
        {
            currentDirection *= -1f;
        }
    }

    /// <summary>
    /// 移動を適用する
    /// </summary>
    private void Move()
    {
        rb.linearVelocity = new Vector2(currentDirection * moveSpeed, rb.linearVelocity.y);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = currentDirection < 0;
        }
    }

    // ===== 応用1: 踏みつけで倒される処理 =====

    /// <summary>
    /// プレイヤーに踏まれた時の処理
    /// </summary>
    public void OnStomped()
    {
        // 動きを止める
        rb.linearVelocity = Vector2.zero;

        // コライダーを無効化（もう当たらないように）
        GetComponent<BoxCollider2D>().enabled = false;

        // 潰れるアニメーション（Y方向に縮める）
        transform.localScale = new Vector3(
            transform.localScale.x,
            transform.localScale.y * 0.3f,
            transform.localScale.z
        );

        // 応用4: スコア加算（応用4と組み合わせる場合）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(200);
        }

        // 少し待ってから消す
        Destroy(gameObject, 0.3f);
    }

    /// <summary>
    /// デバッグ用のギズモを描画
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 pos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawLine(pos + Vector3.left * patrolDistance, pos + Vector3.right * patrolDistance);

        Gizmos.color = Color.red;
        Vector3 wallCheckPos = wallCheck != null ? wallCheck.position : transform.position;
        float dir = Application.isPlaying ? currentDirection : (moveRight ? 1f : -1f);
        Gizmos.DrawRay(wallCheckPos, Vector3.right * dir * wallCheckDistance);
    }
}
