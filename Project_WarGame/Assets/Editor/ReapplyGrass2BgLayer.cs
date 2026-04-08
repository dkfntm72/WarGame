using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

/// <summary>
/// MapData의 Grass2 위치 중 Grass와 인접한 곳에 grassTile을 Grass2BgTilemap(orderInLayer=-1)에 배치.
/// TerrainTilemap은 건드리지 않음.
/// </summary>
public class ReapplyGrass2BgLayer
{
    public static void Execute()
    {
        // GameSettings
        var gsGuids = AssetDatabase.FindAssets("t:GameSettings");
        if (gsGuids.Length == 0) { Debug.LogError("[Grass2Bg] GameSettings not found"); return; }
        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(AssetDatabase.GUIDToAssetPath(gsGuids[0]));
        if (settings.grassTile == null) { Debug.LogError("[Grass2Bg] settings.grassTile is null"); return; }

        // RuleTile 대신 순수 스프라이트 Tile 사용 — TerrainTileConfig(Grass)의 defaultTile
        // RuleTile은 이웃을 보고 자동으로 엣지/코너 변형이 적용되므로, 배경용으로는 부적합.
        TileBase bgGrassTile = null;
        foreach (var guid in AssetDatabase.FindAssets("t:TerrainTileConfig"))
        {
            var cfg = AssetDatabase.LoadAssetAtPath<TerrainTileConfig>(AssetDatabase.GUIDToAssetPath(guid));
            if (cfg != null && cfg.terrainType == TerrainType.Grass && cfg.defaultTile != null)
            {
                bgGrassTile = cfg.defaultTile;
                break;
            }
        }
        if (bgGrassTile == null)
        {
            Debug.LogWarning("[Grass2Bg] TerrainTileConfig(Grass)의 defaultTile을 찾지 못해 grassTile(RuleTile) 사용");
            bgGrassTile = settings.grassTile;
        }
        Debug.Log($"[Grass2Bg] 배경 타일 = {bgGrassTile.name} (RuleTile 여부: {bgGrassTile is RuleTile})");

        // MapData
        var mdGuids = AssetDatabase.FindAssets("t:MapData");
        if (mdGuids.Length == 0) { Debug.LogError("[Grass2Bg] MapData not found"); return; }
        MapData mapData = null;
        foreach (var g in mdGuids)
        {
            var md = AssetDatabase.LoadAssetAtPath<MapData>(AssetDatabase.GUIDToAssetPath(g));
            if (md != null) { mapData = md; break; }
        }
        Debug.Log($"[Grass2Bg] MapData={mapData.scenarioName} {mapData.width}x{mapData.height}");

        // Grass2 분포 확인
        int grass2Total = 0, grassTotal = 0;
        for (int y = 0; y < mapData.height; y++)
        for (int x = 0; x < mapData.width; x++)
        {
            var t = mapData.GetTerrain(x, y);
            if (t == TerrainType.Grass2) grass2Total++;
            else if (t == TerrainType.Grass) grassTotal++;
        }
        Debug.Log($"[Grass2Bg] MapData 통계 — Grass={grassTotal}, Grass2={grass2Total}");

        // Grid / Grass2BgTilemap 찾기
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("[Grass2Bg] Grid not found in scene"); return; }

        Tilemap grass2BgTilemap = null;
        foreach (Transform child in grid.transform)
            if (child.name == "Grass2BgTilemap")
            { grass2BgTilemap = child.GetComponent<Tilemap>(); break; }

        if (grass2BgTilemap == null)
        {
            var go = new GameObject("Grass2BgTilemap");
            go.transform.SetParent(grid.transform, false);
            grass2BgTilemap = go.AddComponent<Tilemap>();
            var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = -1;
            Undo.RegisterCreatedObjectUndo(go, "Create Grass2BgTilemap");
            Debug.Log("[Grass2Bg] Grass2BgTilemap 새로 생성");
        }

        Undo.RecordObject(grass2BgTilemap, "Reapply Grass2BgLayer");
        grass2BgTilemap.ClearAllTiles();

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0,  0, 1, -1 };
        int placed = 0;

        for (int y = 0; y < mapData.height; y++)
        for (int x = 0; x < mapData.width; x++)
        {
            if (mapData.GetTerrain(x, y) != TerrainType.Grass2) continue;

            bool nextToGrass = false;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d], ny = y + dy[d];
                if (nx < 0 || nx >= mapData.width || ny < 0 || ny >= mapData.height) continue;
                if (mapData.GetTerrain(nx, ny) == TerrainType.Grass) { nextToGrass = true; break; }
            }

            if (nextToGrass)
            {
                grass2BgTilemap.SetTile(new Vector3Int(x, y, 0), bgGrassTile);
                placed++;
            }
        }

        grass2BgTilemap.RefreshAllTiles();

        // 정렬 순서 확인/보정
        var renderer = grass2BgTilemap.GetComponent<TilemapRenderer>();
        if (renderer != null && renderer.sortingOrder != -1)
        {
            Undo.RecordObject(renderer, "Fix Grass2BgTilemap sortingOrder");
            renderer.sortingOrder = -1;
            Debug.Log("[Grass2Bg] sortingOrder를 -1로 설정");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[Grass2Bg] 완료 — Grass2와 Grass 인접 위치 {placed}개에 grassTile 배치 (sortingOrder={renderer?.sortingOrder})");
    }
}
