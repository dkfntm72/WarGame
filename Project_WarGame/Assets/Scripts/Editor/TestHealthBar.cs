using UnityEngine;
using UnityEditor;

public class TestHealthBar
{
    [MenuItem("Window/WarGame/Test Health Bar")]
    public static void Execute()
    {
        if (!Application.isPlaying) { Debug.LogError("Play mode required!"); return; }

        var units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        Debug.Log($"Found {units.Length} units");

        foreach (var u in units)
        {
            var hb = u.GetComponent<HealthBar>();
            Debug.Log($"  {u.data?.unitName} — HealthBar={hb != null}, HP={u.CurrentHp}/{u.data?.maxHp}");
            if (hb != null)
            {
                hb.SetHP(u.data.maxHp / 2, u.data.maxHp);
                Debug.Log($"  -> SetHP called: {u.data.maxHp / 2}/{u.data.maxHp}");
            }
        }
    }
}
