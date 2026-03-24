using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddMissingBuildings
{
    public static void Execute()
    {
        var mapData = AssetDatabase.LoadAssetAtPath<MapData>("Assets/GameData/Maps/Stage01.asset");
        if (mapData == null) { Debug.LogError("Stage01 MapData not found"); return; }

        var list = new List<BuildingPlacement>(mapData.buildings);

        // Player ArcheryRange (3,4) — 아군 성 오른쪽
        list.Add(new BuildingPlacement { x = 3, y = 4, buildingType = BuildingType.ArcheryRange, faction = Faction.Player });

        // Player Cathedral (2,5) — 아군 성 위
        list.Add(new BuildingPlacement { x = 2, y = 5, buildingType = BuildingType.Cathedral, faction = Faction.Player });

        // Enemy Cathedral (7,6) — 적 궁수 양성소 위
        list.Add(new BuildingPlacement { x = 7, y = 6, buildingType = BuildingType.Cathedral, faction = Faction.Enemy });

        // Enemy House (9,5) — 적 성 오른쪽
        list.Add(new BuildingPlacement { x = 9, y = 5, buildingType = BuildingType.House, faction = Faction.Enemy });

        mapData.buildings = list.ToArray();

        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Buildings updated: {mapData.buildings.Length} total");
    }
}
