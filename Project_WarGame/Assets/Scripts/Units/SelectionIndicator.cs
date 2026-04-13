using UnityEngine;

/// <summary>
/// 유닛 선택 시 4개의 코너 브라켓 스프라이트를 유닛 주변 모서리에 표시합니다.
/// PlayerInputHandler에서 Show / Hide를 호출합니다.
/// </summary>
public class SelectionIndicator : MonoBehaviour
{
    public static SelectionIndicator Instance { get; private set; }

    [Header("Corner Sprites (TL / TR / BR / BL)")]
    public Sprite spriteTopLeft;
    public Sprite spriteTopRight;
    public Sprite spriteBottomRight;
    public Sprite spriteBottomLeft;

    [Header("Settings")]
    [Tooltip("유닛 중심으로부터 각 코너까지의 오프셋 (월드 단위)")]
    public float cornerOffset = 0.45f;
    [Tooltip("코너 스프라이트 스케일")]
    public float cornerScale  = 0.5f;

    private SpriteRenderer[] corners = new SpriteRenderer[4]; // TL, TR, BR, BL
    private Unit targetUnit;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Vector2[] offsets = {
            new Vector2(-cornerOffset,  cornerOffset),   // Top-Left
            new Vector2( cornerOffset,  cornerOffset),   // Top-Right
            new Vector2( cornerOffset, -cornerOffset),   // Bottom-Right
            new Vector2(-cornerOffset, -cornerOffset),   // Bottom-Left
        };
        string[] names = { "Corner_TL", "Corner_TR", "Corner_BR", "Corner_BL" };

        for (int i = 0; i < 4; i++)
        {
            var child = new GameObject(names[i]);
            child.transform.SetParent(transform);
            child.transform.localPosition = new Vector3(offsets[i].x, offsets[i].y, 0f);
            child.transform.localScale    = Vector3.one * cornerScale;

            var sr = child.AddComponent<SpriteRenderer>();
            sr.enabled = false;
            corners[i] = sr;
        }

        AssignSprites();
        SetEnabled(false);
    }

    private void AssignSprites()
    {
        if (corners[0] != null) corners[0].sprite = spriteTopLeft;
        if (corners[1] != null) corners[1].sprite = spriteTopRight;
        if (corners[2] != null) corners[2].sprite = spriteBottomRight;
        if (corners[3] != null) corners[3].sprite = spriteBottomLeft;
    }

    public void Show(Unit unit)
    {
        targetUnit = unit;
        // 스프라이트가 나중에 Inspector에서 설정됐을 수 있으므로 재할당
        AssignSprites();
        SetEnabled(true);
        SyncTransform();
    }

    public void Hide()
    {
        targetUnit = null;
        SetEnabled(false);
    }

    private void LateUpdate()
    {
        if (!corners[0].enabled) return;

        if (targetUnit == null || !targetUnit.IsAlive)
        {
            Hide();
            return;
        }

        SyncTransform();
    }

    private void SyncTransform()
    {
        if (targetUnit == null) return;

        transform.position = targetUnit.transform.position;

        var unitSr = targetUnit.GetComponent<SpriteRenderer>();
        if (unitSr == null) return;

        int order = unitSr.sortingOrder + 1;
        foreach (var sr in corners)
        {
            if (sr == null) continue;
            sr.sortingLayerID = unitSr.sortingLayerID;
            sr.sortingOrder   = order;
        }
    }

    private void SetEnabled(bool value)
    {
        foreach (var sr in corners)
            if (sr != null) sr.enabled = value;
    }
}
