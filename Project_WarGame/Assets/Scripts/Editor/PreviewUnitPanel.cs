using UnityEngine;
using UnityEditor;
using TMPro;

public class PreviewUnitPanel
{
    public static void Show()
    {
        var panel = GameObject.Find("UI/UnitInfoPanel");
        if (panel == null) { Debug.LogError("Panel not found"); return; }
        panel.SetActive(true);

        // Fill with sample data
        SetTmp(panel, "UnitName",  "워리어 (Blue)",    Color.white);
        SetTmp(panel, "UnitHp",    "HP 100 / 100",     Color.white);
        SetTmp(panel, "UnitStats", "ATK 30  DEF 10  MOV 3  RNG 1", Color.white);
        SetTmp(panel, "UnitStatus","이동 가능 / 행동 가능", new Color(0.6f, 1f, 0.6f));
    }

    public static void Hide()
    {
        var panel = GameObject.Find("UI/UnitInfoPanel");
        if (panel != null) panel.SetActive(false);
    }

    static void SetTmp(GameObject parent, string childName, string text, Color color)
    {
        var t = parent.transform.Find(childName);
        if (t == null) return;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp == null) return;
        tmp.text = text;
        tmp.color = color;
    }
}
