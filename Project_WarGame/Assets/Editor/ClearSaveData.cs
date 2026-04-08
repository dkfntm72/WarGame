using UnityEngine;
using UnityEditor;

public class ClearSaveData
{
    [MenuItem("WarGame/Clear Save Data (PlayerPrefs)")]
    public static void Execute()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[ClearSaveData] PlayerPrefs 전체 초기화 완료");
    }
}
