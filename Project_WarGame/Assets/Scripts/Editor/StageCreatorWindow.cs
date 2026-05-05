using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class StageCreatorWindow : EditorWindow
{
    private int    _stageNumber = 2;
    private string _stageTitle  = "새 스테이지";
    private int    _mapWidth    = 10;
    private int    _mapHeight   = 8;
    private int    _playerGold  = 100;
    private int    _enemyGold   = 80;

    [MenuItem("Window/WarGame/Stage Creator")]
    public static void Open() => GetWindow<StageCreatorWindow>("Stage Creator");

    private void OnGUI()
    {
        GUILayout.Label("새 스테이지 생성", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        _stageNumber = EditorGUILayout.IntField("스테이지 번호", _stageNumber);
        _stageTitle  = EditorGUILayout.TextField("스테이지 제목", _stageTitle);

        EditorGUILayout.Space(6);
        GUILayout.Label("맵 크기", EditorStyles.boldLabel);
        _mapWidth  = EditorGUILayout.IntField("너비", _mapWidth);
        _mapHeight = EditorGUILayout.IntField("높이", _mapHeight);

        EditorGUILayout.Space(6);
        GUILayout.Label("초기 자원", EditorStyles.boldLabel);
        _playerGold = EditorGUILayout.IntField("플레이어 골드", _playerGold);
        _enemyGold  = EditorGUILayout.IntField("적 골드",       _enemyGold);

        EditorGUILayout.Space(10);

        string stageId   = $"Stage{_stageNumber:D2}";
        string scenePath = $"Assets/Scenes/{stageId}.unity";
        string mapPath   = $"Assets/GameData/Maps/{stageId}.asset";

        EditorGUILayout.HelpBox($"씬:  {scenePath}\n맵:  {mapPath}", MessageType.None);

        bool sceneExists = System.IO.File.Exists(
            System.IO.Path.Combine(Application.dataPath, "..", scenePath));
        if (sceneExists)
            EditorGUILayout.HelpBox($"'{scenePath}' 이 이미 존재합니다. 씬을 덮어씁니다.", MessageType.Warning);

        EditorGUILayout.Space(4);

        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>("Assets/GameData/GameSettings.asset");
        if (settings == null)
            EditorGUILayout.HelpBox("GameSettings.asset 없음 — Setup Stage01을 먼저 실행하세요.", MessageType.Error);

        GUI.enabled = settings != null;
        if (GUILayout.Button("스테이지 생성", GUILayout.Height(40)))
        {
            // OnGUI 루프 완료 후 실행 — 씬 전환을 GUI 이벤트 처리 중에 하면
            // EndLayoutGroup 오류 발생
            var id = stageId; var sp = scenePath; var mp = mapPath; var s = settings;
            EditorApplication.delayCall += () => CreateStage(id, sp, mp, s);
        }
        GUI.enabled = true;
    }

    // ── 생성 ──────────────────────────────────────────────────────

    private void CreateStage(string stageId, string scenePath, string mapPath, GameSettings settings)
    {
        // 1. MapData 생성 / 업데이트
        var map = AssetDatabase.LoadAssetAtPath<MapData>(mapPath);
        if (map == null)
        {
            map = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(map, mapPath);
        }
        map.stageTitle          = _stageTitle;
        map.width               = _mapWidth;
        map.height              = _mapHeight;
        map.playerStartGold     = _playerGold;
        map.enemyStartGold      = _enemyGold;
        map.winLossDescription  = "";
        map.InitializeTerrain();
        EditorUtility.SetDirty(map);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 2. 씬 생성
        BuildScene(scenePath, map, settings);

        // 3. StageSelect 씬에 자동 연결
        string linkResult = LinkToStageSelect(map, _stageNumber);

        Debug.Log($"[StageCreator] {stageId} 생성 완료!");
        EditorUtility.DisplayDialog("완료",
            $"{stageId} 생성 완료!\n씬: {scenePath}\n맵: {map.stageTitle}\n\n{linkResult}", "확인");
    }

    // ── 씬 구성 (Stage01을 템플릿으로 복사 후 MapData만 교체) ─────────

    private static void BuildScene(string scenePath, MapData mapData, GameSettings settings)
    {
        const string templatePath = "Assets/Scenes/Stage01.unity";

        if (!System.IO.File.Exists(templatePath))
        {
            EditorUtility.DisplayDialog("오류",
                "템플릿 씬 Assets/Scenes/Stage01.unity 를 찾을 수 없습니다.\n" +
                "Setup Stage01을 먼저 실행하세요.", "확인");
            return;
        }

        // Stage01을 복사
        AssetDatabase.CopyAsset(templatePath, scenePath);
        AssetDatabase.Refresh();

        // 복사된 씬을 열어 GameManager.currentMap 교체
        var newScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        GameManager gm = null;
        foreach (var root in newScene.GetRootGameObjects())
        {
            gm = root.GetComponentInChildren<GameManager>(true);
            if (gm != null) break;
        }

        if (gm != null)
        {
            gm.currentMap = mapData;
            EditorUtility.SetDirty(gm);
        }
        else
        {
            Debug.LogWarning("[StageCreator] 복사된 씬에서 GameManager를 찾을 수 없습니다.");
        }

        EditorSceneManager.SaveScene(newScene);
        EditorSceneManager.CloseScene(newScene, false);
        AddToBuild(scenePath);
    }

    // ── StageSelect 자동 연결 ─────────────────────────────────────

    private static string LinkToStageSelect(MapData newMapData, int stageNumber)
    {
        const string selectScenePath = "Assets/Scenes/StageSelect.unity";

        if (!System.IO.File.Exists(selectScenePath))
            return "StageSelect.unity 없음 — 수동 연결 필요";

        var selectScene = EditorSceneManager.OpenScene(selectScenePath, OpenSceneMode.Additive);

        StageSelectUI ui = null;
        foreach (var root in selectScene.GetRootGameObjects())
        {
            ui = root.GetComponentInChildren<StageSelectUI>(true);
            if (ui != null) break;
        }

        if (ui == null)
        {
            EditorSceneManager.CloseScene(selectScene, true);
            return "StageSelectUI를 찾을 수 없음 — 수동 연결 필요";
        }

        int dataIndex = stageNumber - 1;

        if (ui.stageButtons != null && dataIndex >= ui.stageButtons.Length)
        {
            EditorSceneManager.CloseScene(selectScene, true);
            return $"StageSelect에 버튼이 {ui.stageButtons.Length}개뿐 — 버튼 추가 후 수동 연결 필요";
        }

        var datas = ui.stageDatas ?? new MapData[0];
        if (datas.Length <= dataIndex)
            System.Array.Resize(ref datas, dataIndex + 1);

        datas[dataIndex] = newMapData;
        ui.stageDatas = datas;

        EditorUtility.SetDirty(ui);
        EditorSceneManager.SaveScene(selectScene);
        EditorSceneManager.CloseScene(selectScene, true);

        return $"StageSelect.stageDatas[{dataIndex}] 연결 완료";
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────

    static void AddToBuild(string scenePath)
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(
            EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == scenePath))
        {
            list.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
