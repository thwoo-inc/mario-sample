using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 【応用編 模範解答】プレイヤーの操作を制御するクラス
/// 応用1（敵踏みつけ）と応用2（ダブルジャンプ）を含む
/// ※ このファイルは模範解答です。Unityプロジェクトには含めません。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float jumpForce = 10f;

    [Header("接地判定")]
    [SerializeField]
    private Transform groundCheck;

    [SerializeField]
    private float groundCheckRadius = 0.2f;

    [SerializeField]
    private LayerMask groundLayer;

    [Header("落下判定")]
    [SerializeField]
    private float fallThreshold = -10f;

    // ===== 応用1: 踏みつけ設定 =====
    [Header("踏みつけ設定")]
    [SerializeField]
    private float stompBounceForce = 8f;  // 踏んだ後の跳ね返り力

    // ===== 応用2: ダブルジャンプ設定 =====
    [Header("ダブルジャンプ設定")]
    [SerializeField]
    private int maxJumpCount = 2;  // 最大ジャンプ回数（2=ダブルジャンプ）

    // コンポーネント参照
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 状態
    private bool isGrounded = false;
    private int currentJumpCount = 0;  // 応用2: ジャンプ回数カウント

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Rigidbody2Dの設定
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        // ゲームプレイ中のみ操作可能
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        // 接地判定
        CheckGround();

        // 移動処理
        HandleMovement();

        // ジャンプ処理
        HandleJump();

        // 落下判定
        CheckFall();
    }

    /// <summary>
    /// 接地判定を行う（応用2: 着地時にジャンプ回数リセット）
    /// </summary>
    private void CheckGround()
    {
        // 前のフレームの接地状態を保存
        bool wasGrounded = isGrounded;

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(
                transform.position + Vector3.down * 0.5f,
                groundCheckRadius,
                groundLayer
            );
        }

        // 応用2: 着地した瞬間にジャンプ回数をリセット
        if (!wasGrounded && isGrounded)
        {
            currentJumpCount = 0;
        }
    }

    /// <summary>
    /// 左右移動を処理する
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal = -1f;
            }
            else if (Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal = 1f;
            }
        }

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (horizontal != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = horizontal < 0;
        }
    }

    /// <summary>
    /// ジャンプを処理する（応用2: ダブルジャンプ対応）
    /// </summary>
    private void HandleJump()
    {
        // 応用2: isGrounded の代わりに currentJumpCount < maxJumpCount で判定
        if (Keyboard.current != null &&
            Keyboard.current.upArrowKey.wasPressedThisFrame &&
            currentJumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            currentJumpCount++;

            // 応用5: ジャンプ音を鳴らす
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySE("jump");
            }
        }
    }

    /// <summary>
    /// 落下判定を行う
    /// </summary>
    private void CheckFall()
    {
        if (transform.position.y < fallThreshold)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    /// <summary>
    /// 敵との衝突時の処理（応用1: 踏みつけ判定を追加）
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 応用1: 衝突点を取得して、上から踏んだか判定
            float playerBottom = transform.position.y -
                GetComponent<BoxCollider2D>().bounds.extents.y;
            float enemyCenter = collision.transform.position.y;

            if (playerBottom > enemyCenter)
            {
                // 踏みつけ成功！
                EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.OnStomped();
                }

                // プレイヤーが少し跳ねる
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounceForce);

                // 応用5: 踏みつけ音
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySE("stomp");
                }
            }
            else
            {
                // 横から当たった → ゲームオーバー
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }
    }

    /// <summary>
    /// アイテムとの衝突時の処理（トリガー）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectItem();
            }
            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// 接地判定用のギズモを描画（デバッグ用）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 checkPos = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 0.5f;
        Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
    }
}
