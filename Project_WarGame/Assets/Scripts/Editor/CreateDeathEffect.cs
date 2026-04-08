using UnityEngine;
using UnityEditor;

/// <summary>
/// DeathEffect 프리팹을 생성하고 GameSettings에 연결합니다.
/// Window > WarGame > Create Death Effect Prefab
/// </summary>
public class CreateDeathEffect
{
    private const string ControllerPath  = "Assets/Tiny Swords/Particle FX/Explosion 1 Animation/Explosion 1.controller";
    private const string PrefabSavePath  = "Assets/Prefabs/DeathEffect.prefab";
    private const string GameSettingsPath = "Assets/GameData/GameSettings.asset";

    [MenuItem("Window/WarGame/Create Death Effect Prefab")]
    public static void Execute()
    {
        // Prefabs 폴더 없으면 생성
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"[CreateDeathEffect] Animator Controller를 찾을 수 없습니다: {ControllerPath}");
            return;
        }

        // 임시 GameObject 생성
        var go = new GameObject("DeathEffect");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Units";
        sr.sortingOrder     = 10;

        var anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        // 프리팹으로 저장
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabSavePath);
        Object.DestroyImmediate(go);

        if (prefab == null)
        {
            Debug.LogError("[CreateDeathEffect] 프리팹 저장 실패");
            return;
        }

        // GameSettings에 연결
        var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(GameSettingsPath);
        if (settings != null)
        {
            settings.deathEffectPrefab = prefab;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("[CreateDeathEffect] DeathEffect 프리팹 생성 및 GameSettings 연결 완료");
        }
        else
        {
            Debug.LogWarning($"[CreateDeathEffect] GameSettings를 찾을 수 없습니다: {GameSettingsPath}. Inspector에서 직접 할당하세요.");
        }

        AssetDatabase.Refresh();
    }
}
