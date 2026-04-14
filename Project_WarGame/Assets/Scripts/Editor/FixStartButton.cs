using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixStartButton
{
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();

        GameObject canvas = null;
        foreach (var r in roots)
            if (r.name == "Canvas") { canvas = r; break; }
        if (canvas == null) { Debug.LogError("Canvas not found"); return; }

        // StartButton
        var startBtn = canvas.transform.Find("StartButton");
        if (startBtn == null) { Debug.LogError("StartButton not found"); return; }

        // 레거시 Text 제거 후 TMP 교체
        var textGo = startBtn.Find("Text");
        if (textGo != null)
        {
            var legacyText = textGo.GetComponent<Text>();
            if (legacyText != null) Object.DestroyImmediate(legacyText);

            var tmp = textGo.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = "시작";
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/Fonts/경기천년제목OTF_Bold SDF.asset");
            if (font != null) tmp.font = font;
        }

        // Button 색상: BackButton과 동일하게
        var btn = startBtn.GetComponent<Button>();
        if (btn != null)
        {
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.32f, 1f);
            colors.pressedColor     = new Color(0.1f, 0.1f, 0.12f, 1f);
            btn.colors = colors;
        }

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("StartButton 설정 완료");
    }
}
