using UnityEngine;
using TMPro;

/// <summary>
/// 【応用編 模範解答】ゲーム中のUI表示を管理するクラス
/// 応用4（スコアとタイマー表示）を含む
/// ※ このファイルは模範解答です。Unityプロジェクトには含めません。
/// </summary>
public class GameUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI itemText;

    // ===== 応用4: スコアとタイマー表示 =====
    [SerializeField]
    private TextMeshProUGUI scoreText;    // スコア表示用

    [SerializeField]
    private TextMeshProUGUI timerText;    // タイマー表示用

    void Update()
    {
        if (GameManager.Instance == null) return;

        // アイテム表示（既存）
        if (itemText != null)
        {
            itemText.text = "ITEMS: " +
                GameManager.Instance.GetItemCount() + " / " +
                GameManager.Instance.GetRequiredItemCount();
        }

        // 応用4: スコア表示
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + GameManager.Instance.GetScore();
        }

        // 応用4: タイマー表示
        if (timerText != null)
        {
            // 残り時間を整数に切り上げて表示
            int timeInt = Mathf.CeilToInt(GameManager.Instance.GetRemainingTime());
            timerText.text = "TIME: " + timeInt;

            // 残り10秒以下は赤くする
            if (timeInt <= 10)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
}
