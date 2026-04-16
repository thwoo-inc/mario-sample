using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 【応用編 模範解答】ゲーム全体を管理するマネージャークラス
/// 応用4（スコアとタイマー）と応用5（BGM制御）を含む
/// ※ このファイルは模範解答です。Unityプロジェクトには含めません。
/// </summary>
public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameManager Instance { get; private set; }

    // ゲームの状態
    public enum GameState
    {
        Title,
        Playing,
        GameOver,
        GameClear
    }

    // 現在のゲーム状態
    public GameState CurrentState { get; private set; }

    // アイテム取得数
    private int itemCount = 0;

    // クリアに必要なアイテム数
    [SerializeField]
    private int requiredItemCount = 3;

    // ===== 応用4: スコア設定 =====
    [Header("スコア設定")]
    [SerializeField]
    private int scorePerItem = 100;      // アイテム1個あたりのスコア

    [SerializeField]
    private int scorePerStomp = 200;     // 敵踏みつけのスコア

    [SerializeField]
    private int timeBonus = 10;          // 残り時間1秒あたりのボーナス

    // ===== 応用4: タイマー設定 =====
    [Header("タイマー設定")]
    [SerializeField]
    private float timeLimit = 60f;       // 制限時間（秒）

    // スコアとタイマー
    private int score = 0;
    private float remainingTime;

    void Awake()
    {
        // シングルトンパターン
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateStateFromScene();
    }

    void Update()
    {
        // 応用4: タイマー処理（プレイ中のみ）
        if (CurrentState == GameState.Playing)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                GameOver();  // 時間切れ！
                return;
            }
        }

        // スペースキー入力の処理
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            switch (CurrentState)
            {
                case GameState.Title:
                    StartGame();
                    break;
                case GameState.GameOver:
                case GameState.GameClear:
                    ReturnToTitle();
                    break;
            }
        }
    }

    /// <summary>
    /// 現在のシーン名からゲーム状態を更新
    /// </summary>
    private void UpdateStateFromScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "TitleScene":
                CurrentState = GameState.Title;
                break;
            case "GameScene":
                CurrentState = GameState.Playing;
                break;
            case "GameOverScene":
                CurrentState = GameState.GameOver;
                break;
            case "GameClearScene":
                CurrentState = GameState.GameClear;
                break;
        }

        // 応用5: タイトルならタイトルBGMを再生
        if (sceneName == "TitleScene" && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM("title");
        }
    }

    /// <summary>
    /// ゲームを開始する
    /// </summary>
    public void StartGame()
    {
        itemCount = 0;
        score = 0;                         // 応用4: スコアをリセット
        remainingTime = timeLimit;         // 応用4: タイマーをリセット
        CurrentState = GameState.Playing;

        // 応用5: ゲームBGMを再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM("game");
        }

        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// タイトル画面に戻る
    /// </summary>
    public void ReturnToTitle()
    {
        itemCount = 0;
        score = 0;
        CurrentState = GameState.Title;
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// ゲームオーバーにする
    /// </summary>
    public void GameOver()
    {
        CurrentState = GameState.GameOver;

        // 応用5: BGM停止 & ゲームオーバー音
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySE("gameover");
        }

        SceneManager.LoadScene("GameOverScene");
    }

    /// <summary>
    /// ゲームクリアにする
    /// </summary>
    public void GameClear()
    {
        CurrentState = GameState.GameClear;

        // 応用5: BGM停止 & クリア音
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySE("clear");
        }

        SceneManager.LoadScene("GameClearScene");
    }

    /// <summary>
    /// アイテムを取得した時に呼ばれる
    /// </summary>
    public void CollectItem()
    {
        itemCount++;
        score += scorePerItem;  // 応用4: スコア加算
        Debug.Log("スコア: " + score + " アイテム: " + itemCount + " / " + requiredItemCount);

        // 応用5: アイテム取得音
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE("item");
        }

        // クリア条件を達成したらゲームクリア
        if (itemCount >= requiredItemCount)
        {
            // 応用4: クリアボーナス
            int bonus = Mathf.CeilToInt(remainingTime) * timeBonus;
            score += bonus;

            GameClear();
        }
    }

    // ===== 応用4: スコア関連メソッド =====

    /// <summary>
    /// スコアを加算する（敵を倒した時など）
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
    }

    /// <summary>
    /// 現在のスコアを取得
    /// </summary>
    public int GetScore()
    {
        return score;
    }

    /// <summary>
    /// 残り時間を取得
    /// </summary>
    public float GetRemainingTime()
    {
        return remainingTime;
    }

    /// <summary>
    /// 現在のアイテム数を取得
    /// </summary>
    public int GetItemCount()
    {
        return itemCount;
    }

    /// <summary>
    /// 必要なアイテム数を取得
    /// </summary>
    public int GetRequiredItemCount()
    {
        return requiredItemCount;
    }
}
