using UnityEngine;
using UnityEditor;

public class CheckRedButtonSprite
{
    public static void Execute()
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Tiny Swords/UI Elements/Buttons/SmallRedSquareButton_Regular.png");
        foreach (var obj in all)
            Debug.Log($"[Check] type={obj.GetType().Name}  name={obj.name}");

        // ExitButton 위치도 확인
        var uiGO = GameObject.Find("UI");
        if (uiGO != null)
        {
            int found = 0;
            foreach (var t in uiGO.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "ExitButton") { Debug.Log($"[Check] ExitButton found: parent={t.parent?.name}"); found++; }
            }
            if (found == 0) Debug.LogWarning("[Check] ExitButton 없음");
        }
    }
}
