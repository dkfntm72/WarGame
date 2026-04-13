using UnityEngine;
using UnityEditor;

public class SetupSelectionIndicator
{
    public static void Execute()
    {
        // Cursor_04.png 서브스프라이트 로드
        var allSprites = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Cursors/Cursor_04.png");

        Sprite tl = null, tr = null, bl = null, br = null;
        foreach (var obj in allSprites)
        {
            if (obj is not Sprite s) continue;
            switch (s.name)
            {
                case "Cursor_04_0": tl = s; break; // Top-Left
                case "Cursor_04_1": tr = s; break; // Top-Right
                case "Cursor_04_2": bl = s; break; // Bottom-Left
                case "Cursor_04_3": br = s; break; // Bottom-Right
            }
        }

        Debug.Log($"[SetupSelectionIndicator] 로드: TL={tl}, TR={tr}, BL={bl}, BR={br}");

        // 씬에서 SelectionIndicator 컴포넌트 찾기
        var indicator = Object.FindFirstObjectByType<SelectionIndicator>();
        if (indicator == null)
        {
            Debug.LogError("[SetupSelectionIndicator] SelectionIndicator를 씬에서 찾을 수 없습니다.");
            return;
        }

        Undo.RecordObject(indicator, "Setup SelectionIndicator Sprites");
        indicator.spriteTopLeft    = tl;
        indicator.spriteTopRight   = tr;
        indicator.spriteBottomLeft = bl;
        indicator.spriteBottomRight = br;

        EditorUtility.SetDirty(indicator);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            indicator.gameObject.scene);

        Debug.Log("[SetupSelectionIndicator] 스프라이트 설정 완료!");
    }
}
