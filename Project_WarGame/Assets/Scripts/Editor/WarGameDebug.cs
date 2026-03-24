using UnityEngine;
using UnityEditor;

public static class WarGameDebug
{
    public static void CheckGameState()
    {
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null) { Debug.Log("[Debug] GameManager 없음 (플레이 중이 아님)"); return; }

        Debug.Log($"[Debug] 플레이어 유닛: {gm.PlayerUnits.Count}개");
        foreach (var u in gm.PlayerUnits)
            Debug.Log($"  - {u.data.unitName} HP:{u.CurrentHp} 위치:({u.CurrentTile.X},{u.CurrentTile.Y})");

        Debug.Log($"[Debug] 적군 유닛: {gm.EnemyUnits.Count}개");
        foreach (var u in gm.EnemyUnits)
            Debug.Log($"  - {u.data.unitName} HP:{u.CurrentHp} 위치:({u.CurrentTile.X},{u.CurrentTile.Y})");

        Debug.Log($"[Debug] 건물: {gm.Buildings.Count}개");
        foreach (var b in gm.Buildings)
            Debug.Log($"  - {b.data.buildingName} [{b.faction}] 위치:({b.tile.X},{b.tile.Y})");

        Debug.Log($"[Debug] 골드 - 플레이어:{ResourceManager.Instance?.PlayerGold} 적군:{ResourceManager.Instance?.EnemyGold}");
        Debug.Log($"[Debug] 현재 상태: {TurnManager.Instance?.CurrentState} 턴:{TurnManager.Instance?.TurnNumber}");
    }
}
