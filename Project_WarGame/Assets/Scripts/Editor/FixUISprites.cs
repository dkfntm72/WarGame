using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixUISprites
{
    public static void Execute()
    {
        var hudGO      = GameObject.Find("UI/HUD");
        var endTurnGO  = GameObject.Find("UI/EndTurnButton");
        // SettingsPanel은 비활성 상태이므로 Canvas 전체에서 비활성 포함 탐색
        GameObject exitBtnGO = null;
        var uiGO = GameObject.Find("UI");
        if (uiGO != null)
        {
            foreach (var t in uiGO.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "ExitButton" && t.parent?.name == "SettingsPanel")
                { exitBtnGO = t.gameObject; break; }
            }
        }

        // ── 스프라이트 로드 ────────────────────────────────────
        Sprite specialPaper = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Papers/SpecialPaper.png"))
            if (obj is Sprite sp && sp.name == "SpecialPaper_0") { specialPaper = sp; break; }

        Sprite blueBtn = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Buttons/SmallBlueSquareButton_Regular.png"))
            if (obj is Sprite sp && sp.name == "SmallBlueSquareButton_Regular_0") { blueBtn = sp; break; }

        Sprite redBtn = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Buttons/SmallRedSquareButton_Regular.png"))
            if (obj is Sprite sp) { redBtn = sp; break; }

        // ── HUD 배경 → SpecialPaper_0 ─────────────────────────
        if (hudGO != null && specialPaper != null)
        {
            var img       = hudGO.GetComponent<Image>();
            img.sprite    = specialPaper;
            img.type      = Image.Type.Sliced;
            img.color     = Color.white;
            EditorUtility.SetDirty(hudGO);
            Debug.Log("[WarGame] HUD 배경 → SpecialPaper_0 적용");
        }

        // ── 턴 종료 버튼 → SmallBlueSquareButton ─────────────
        if (endTurnGO != null && blueBtn != null)
        {
            var img       = endTurnGO.GetComponent<Image>();
            img.sprite    = blueBtn;
            img.type      = Image.Type.Sliced;
            img.color     = Color.white;
            EditorUtility.SetDirty(endTurnGO);
            Debug.Log("[WarGame] EndTurnButton → SmallBlueSquareButton_Regular_0 적용");
        }

        // ── 나가기 버튼 → SmallRedSquareButton ───────────────
        if (exitBtnGO != null && redBtn != null)
        {
            var img       = exitBtnGO.GetComponent<Image>();
            img.sprite    = redBtn;
            img.type      = Image.Type.Sliced;
            img.color     = Color.white;
            EditorUtility.SetDirty(exitBtnGO);
            Debug.Log("[WarGame] ExitButton → SmallRedSquareButton_Regular_0 적용");
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
