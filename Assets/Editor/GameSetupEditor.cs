using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// ゲームのセットアップを行うエディタ拡張
/// メニューから実行可能
/// </summary>
public class GameSetupEditor : EditorWindow
{
    [MenuItem("Mario Sample/Setup Game")]
    public static void ShowWindow()
    {
        GetWindow<GameSetupEditor>("Game Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mario Sample セットアップ", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. タグを設定する"))
        {
            SetupTags();
        }

        if (GUILayout.Button("2. レイヤーを設定する"))
        {
            SetupLayers();
        }

        if (GUILayout.Button("3. Build Settingsにシーンを追加"))
        {
            SetupBuildSettings();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("全てセットアップ"))
        {
            SetupTags();
            SetupLayers();
            SetupBuildSettings();
            Debug.Log("セットアップが完了しました！");
        }

        GUILayout.Space(20);
        GUILayout.Label("シーンを開く", EditorStyles.boldLabel);

        if (GUILayout.Button("TitleScene を開く"))
        {
            OpenScene("Assets/Scenes/TitleScene.unity");
        }

        if (GUILayout.Button("GameScene を開く"))
        {
            OpenScene("Assets/Scenes/GameScene.unity");
        }

        if (GUILayout.Button("GameOverScene を開く"))
        {
            OpenScene("Assets/Scenes/GameOverScene.unity");
        }

        if (GUILayout.Button("GameClearScene を開く"))
        {
            OpenScene("Assets/Scenes/GameClearScene.unity");
        }
    }

    private static void SetupTags()
    {
        // タグを追加
        AddTag("Player");
        AddTag("Enemy");
        AddTag("Item");
        AddTag("Ground");

        Debug.Log("タグの設定が完了しました。");
    }

    private static void SetupLayers()
    {
        // レイヤーを追加
        AddLayer("Ground");
        AddLayer("Player");
        AddLayer("Enemy");
        AddLayer("Item");

        Debug.Log("レイヤーの設定が完了しました。");
    }

    private static void SetupBuildSettings()
    {
        // Build Settingsにシーンを追加
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/TitleScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameOverScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameClearScene.unity", true),
        };

        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build Settingsにシーンを追加しました。");
    }

    private static void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    private static void AddTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // タグが既に存在するかチェック
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tagName))
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTag.stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }
    }

    private static void AddLayer(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        // レイヤー8から31まで使用可能（0-7はUnity予約）
        for (int i = 8; i < 32; i++)
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return;
            }

            if (sp.stringValue.Equals(layerName))
            {
                return; // 既に存在
            }
        }
    }
}
