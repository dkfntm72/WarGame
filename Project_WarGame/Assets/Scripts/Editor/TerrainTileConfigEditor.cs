using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// TerrainTileConfig 커스텀 인스펙터.
/// 각 규칙마다 3×3 이웃 그리드를 표시하고 클릭으로 조건을 설정한다.
///
/// 그리드 레이아웃:
///   [NW][N ][NE]
///   [W ][  ][E ]
///   [SW][S ][SE]
///
/// 색상:
///   회색 = Any (무시)
///   초록 = Same (같은 지형)
///   빨강 = Diff (다른 지형)
/// </summary>
[CustomEditor(typeof(TerrainTileConfig))]
public class TerrainTileConfigEditor : Editor
{
    // 그리드 (col, row) → neighbor 인덱스
    // N=0, NE=1, E=2, SE=3, S=4, SW=5, W=6, NW=7
    static readonly int[,] GridIndex = new int[3, 3]
    {
        { 7, 0, 1 },  // row 0: NW, N,  NE
        { 6,-1, 2 },  // row 1: W,  (center), E
        { 5, 4, 3 },  // row 2: SW, S,  SE
    };

    static readonly Color ColCenter = new Color(0.20f, 0.55f, 0.95f);

    static Color ReqColor(NeighborReq r) => r switch
    {
        NeighborReq.Grass   => new Color(0.30f, 0.72f, 0.30f),
        NeighborReq.Wall    => new Color(0.52f, 0.48f, 0.42f),
        NeighborReq.Slope   => new Color(0.72f, 0.62f, 0.38f),
        NeighborReq.Water   => new Color(0.20f, 0.50f, 0.90f),
        NeighborReq.Same    => new Color(0.20f, 0.80f, 0.60f),
        NeighborReq.NotSame => new Color(0.85f, 0.35f, 0.20f),
        _                   => new Color(0.35f, 0.35f, 0.35f),  // Any
    };

    static string ReqLabel(NeighborReq r) => r switch
    {
        NeighborReq.Grass   => "Grs",
        NeighborReq.Wall    => "Wal",
        NeighborReq.Slope   => "Slp",
        NeighborReq.Water   => "Wat",
        NeighborReq.Same    => "==",
        NeighborReq.NotSame => "≠",
        _                   => "-",
    };

    // 클릭 시 순환 순서
    static readonly NeighborReq[] Cycle =
    {
        NeighborReq.Any, NeighborReq.Same, NeighborReq.NotSame,
        NeighborReq.Grass, NeighborReq.Wall, NeighborReq.Slope, NeighborReq.Water,
    };
    // NOTE: Same/NotSame 은 음수(-2/-3)이므로 Array.IndexOf 탐색이 올바르게 동작함

    const float CellSize = 22f;
    const float GridSize = CellSize * 3 + 4f;

    public override void OnInspectorGUI()
    {
        var cfg = (TerrainTileConfig)target;
        serializedObject.Update();

        // ── 기본 필드 ──────────────────────────────────────────────
        EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultTile"));
        EditorGUILayout.Space(8);

        // ── 규칙 목록 ─────────────────────────────────────────────
        EditorGUILayout.LabelField("Rules  (위에서 순서대로 평가, 처음 일치 사용)", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        for (int i = 0; i < cfg.rules.Count; i++)
        {
            DrawRule(cfg, i);
            EditorGUILayout.Space(4);
        }

        // ── 추가 버튼 ─────────────────────────────────────────────
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+ Add Rule", GUILayout.Height(24)))
            {
                Undo.RecordObject(cfg, "Add TileRule");
                cfg.rules.Add(new TileRule());
                EditorUtility.SetDirty(cfg);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawRule(TerrainTileConfig cfg, int idx)
    {
        var rule = cfg.rules[idx];

        using var box = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"Rule {idx + 1}", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.FlexibleSpace();

            // 위로/아래로 이동
            GUI.enabled = idx > 0;
            if (GUILayout.Button("▲", EditorStyles.miniButton, GUILayout.Width(24)))
            {
                Undo.RecordObject(cfg, "Move Rule");
                (cfg.rules[idx], cfg.rules[idx - 1]) = (cfg.rules[idx - 1], cfg.rules[idx]);
                EditorUtility.SetDirty(cfg);
                return;
            }
            GUI.enabled = idx < cfg.rules.Count - 1;
            if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(24)))
            {
                Undo.RecordObject(cfg, "Move Rule");
                (cfg.rules[idx], cfg.rules[idx + 1]) = (cfg.rules[idx + 1], cfg.rules[idx]);
                EditorUtility.SetDirty(cfg);
                return;
            }
            GUI.enabled = true;

            // 삭제
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("✕", EditorStyles.miniButton, GUILayout.Width(24)))
            {
                Undo.RecordObject(cfg, "Remove Rule");
                cfg.rules.RemoveAt(idx);
                EditorUtility.SetDirty(cfg);
                GUI.backgroundColor = oldColor;
                return;
            }
            GUI.backgroundColor = oldColor;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            // 3×3 그리드
            var gridRect = GUILayoutUtility.GetRect(GridSize, GridSize,
                GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            DrawGrid(gridRect, cfg, rule, idx);

            GUILayout.Space(12);

            // 오른쪽: 타일 필드 + 조건 설명
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label("→ Tile", EditorStyles.miniLabel);

                EditorGUI.BeginChangeCheck();
                var newTile = (TileBase)EditorGUILayout.ObjectField(
                    rule.tile, typeof(TileBase), false, GUILayout.Width(140));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(cfg, "Set Tile");
                    rule.tile = newTile;
                    EditorUtility.SetDirty(cfg);
                }

                GUILayout.Space(6);

                // 조건 요약 텍스트
                var summary = BuildSummary(rule);
                EditorGUILayout.LabelField(summary, EditorStyles.wordWrappedMiniLabel,
                    GUILayout.Width(140));
            }
        }
    }

    void DrawGrid(Rect rect, TerrainTileConfig cfg, TileRule rule, int ruleIdx)
    {
        float step = CellSize + 1f;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var cellRect = new Rect(
                    rect.x + col * step,
                    rect.y + row * step,
                    CellSize, CellSize);

                int nIdx = GridIndex[row, col];

                if (nIdx == -1)
                {
                    // 중앙 (현재 셀)
                    EditorGUI.DrawRect(cellRect, ColCenter);
                    var s = new GUIStyle(EditorStyles.miniLabel)
                        { alignment = TextAnchor.MiddleCenter, fontSize = 9,
                          normal = { textColor = Color.white } };
                    EditorGUI.LabelField(cellRect, "●", s);
                    continue;
                }

                var req = rule.neighbors[nIdx];
                EditorGUI.DrawRect(cellRect, ReqColor(req));

                // 라벨
                var style = new GUIStyle(EditorStyles.miniLabel)
                    { alignment = TextAnchor.MiddleCenter, fontSize = 8,
                      normal = { textColor = Color.white } };
                EditorGUI.LabelField(cellRect, ReqLabel(req), style);

                // 클릭 → 순환: Any → Grass → Wall → Slope → Tree → Water → Any
                if (Event.current.type == EventType.MouseDown
                    && cellRect.Contains(Event.current.mousePosition))
                {
                    Undo.RecordObject(cfg, "Edit Neighbor");
                    int cur = System.Array.IndexOf(Cycle, req);
                    rule.neighbors[nIdx] = Cycle[(cur + 1) % Cycle.Length];
                    EditorUtility.SetDirty(cfg);
                    Event.current.Use();
                    Repaint();
                }
            }
        }
    }

    static string BuildSummary(TileRule rule)
    {
        string[] dirs = { "N","NE","E","SE","S","SW","W","NW" };
        var parts = new List<string>();
        for (int i = 0; i < 8; i++)
            if (rule.neighbors[i] != NeighborReq.Any)
                parts.Add($"{dirs[i]}={ReqLabel(rule.neighbors[i])}");
        return parts.Count == 0 ? "(항상 일치)" : string.Join(", ", parts);
    }
}
