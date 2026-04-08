using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 정예 전사 UnitData 에셋을 생성하고 GameSettings에 등록합니다.
/// Window > WarGame > Create Elite Warrior Data
/// </summary>
public class CreateEliteWarriorData
{
    private const string AssetSavePath   = "Assets/GameData/Units/EliteWarrior.asset";
    private const string GameSettingsPath = "Assets/GameData/GameSettings.asset";

    [MenuItem("Window/WarGame/Create Elite Warrior Data")]
    public static void Execute()
    {
        // ── 1. UnitData 에셋 생성 ─────────────────────────────
        var existing = AssetDatabase.LoadAssetAtPath<UnitData>(AssetSavePath);
        if (existing != null)
        {
            Debug.Log("[CreateEliteWarrior] EliteWarrior.asset이 이미 존재합니다. 덮어씁니다.");
            AssetDatabase.DeleteAsset(AssetSavePath);
        }

        var data = ScriptableObject.CreateInstance<UnitData>();
        data.unitType    = UnitType.EliteWarrior;
        data.unitName    = "정예 전사";
        data.isElite     = true;
        data.maxHp       = 150;   // Warrior(100) × 1.5
        data.attack      = 23;    // Warrior(15)  × 1.5 ≈ 23
        data.defense     = 8;     // Warrior(5)   × 1.5 ≈ 8
        data.moveRange   = 4;     // Warrior(3)   × 1.5 ≈ 4
        data.attackRange = 1;
        data.detectRange = 6;
        data.goldCost    = 0;     // 생산 불가

        // 스프라이트/애니메이터는 Warrior와 동일한 것을 참조 (없으면 수동 연결)
        var warriorData = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/GameData/Units/Warrior.asset");
        if (warriorData != null)
        {
            data.playerAnimController = warriorData.playerAnimController;
            data.playerIdleSprite     = warriorData.playerIdleSprite;
            // 정예유닛은 플레이어 전용이므로 enemy 쪽은 비워 둠
        }

        AssetDatabase.CreateAsset(data, AssetSavePath);
        AssetDatabase.SaveAssets();

        // ── 2. GameSettings.unitDataList에 등록 ──────────────
        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(GameSettingsPath);
        if (settings == null)
        {
            Debug.LogWarning("[CreateEliteWarrior] GameSettings.asset을 찾을 수 없습니다. 수동으로 unitDataList[4]에 등록해 주세요.");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = data;
            return;
        }

        Undo.RecordObject(settings, "Add EliteWarrior to GameSettings");

        // unitDataList 크기 보정: EliteWarrior는 index 4
        var list = new List<UnitData>(settings.unitDataList ?? new UnitData[0]);
        while (list.Count < 4) list.Add(null); // 빈 슬롯 채우기

        if (list.Count == 4)
            list.Add(data);
        else
            list[4] = data; // 이미 5개 이상이면 교체

        settings.unitDataList = list.ToArray();
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log("[CreateEliteWarrior] EliteWarrior.asset 생성 및 GameSettings 등록 완료.");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;
        EditorUtility.DisplayDialog("완료",
            "정예 전사 데이터 생성 완료!\n\n" +
            "HP 150 / 공격 23 / 방어 8 / 이동 4\n\n" +
            "스프라이트·애니메이터는 Warrior와 공유됩니다.\n" +
            "필요 시 EliteWarrior.asset에서 별도 지정하세요.", "OK");
    }
}
