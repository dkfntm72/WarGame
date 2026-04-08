using UnityEngine;

/// <summary>
/// 좌클릭 드래그로 카메라를 이동합니다.
/// 드래그 여부(IsDragging)를 PlayerInputHandler에서 참조해 탭과 구분합니다.
/// </summary>
public class CameraDragController : MonoBehaviour
{
    public static CameraDragController Instance { get; private set; }

    /// <summary>현재 프레임이 드래그 조작 중이었는지 여부. PlayerInputHandler가 탭 판별에 사용합니다.</summary>
    public static bool IsDragging { get; private set; }

    [SerializeField] private float dragThresholdPx = 8f;

    private Vector3 _dragOriginWorld;
    private Vector2 _pressScreenPos;
    private bool    _pressing;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameUI.Instance != null && GameUI.Instance.IsSettingsOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            IsDragging       = false;
            _pressing        = true;
            _pressScreenPos  = Input.mousePosition;
            _dragOriginWorld = ScreenToWorld(Input.mousePosition);
        }

        if (_pressing && Input.GetMouseButton(0))
        {
            // 임계값을 넘으면 드래그로 판정
            if (!IsDragging &&
                Vector2.Distance((Vector2)Input.mousePosition, _pressScreenPos) > dragThresholdPx)
            {
                IsDragging = true;
            }

            if (IsDragging)
            {
                // 누른 지점의 월드 좌표가 마우스 커서에 고정되도록 카메라 이동
                Vector3 current = ScreenToWorld(Input.mousePosition);
                Camera.main.transform.position += _dragOriginWorld - current;
                ClampCameraToMapBounds();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _pressing = false;
            // IsDragging은 다음 MouseButtonDown까지 유지 →
            // 같은 프레임의 MouseButtonUp에서 PlayerInputHandler가 확인 가능
        }
    }

    // 2D 직교 카메라: z를 카메라→월드(z=0) 거리로 설정해야 올바른 월드 좌표를 얻는다
    private static Vector3 ScreenToWorld(Vector3 screenPos)
    {
        screenPos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    /// <summary>스테이지 시작 시 GameManager가 호출 — 맵 가장자리에 카메라를 배치합니다.</summary>
    public void PositionAtMapEdge(CameraEdge edge)
    {
        if (GridManager.Instance == null || GridManager.Instance.grid == null) return;

        var cam  = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 mapMin = GridManager.Instance.grid.CellToWorld(new Vector3Int(0, 0, 0));
        Vector3 mapMax = GridManager.Instance.grid.CellToWorld(
            new Vector3Int(GridManager.Instance.Width, GridManager.Instance.Height, 0));

        float centerX = (mapMin.x + mapMax.x) * 0.5f;
        float centerY = (mapMin.y + mapMax.y) * 0.5f;

        Vector3 pos = cam.transform.position;
        switch (edge)
        {
            case CameraEdge.BottomLeft:  pos.x = mapMin.x + halfW; pos.y = mapMin.y + halfH; break;
            case CameraEdge.BottomRight: pos.x = mapMax.x - halfW; pos.y = mapMin.y + halfH; break;
            case CameraEdge.TopLeft:     pos.x = mapMin.x + halfW; pos.y = mapMax.y - halfH; break;
            case CameraEdge.TopRight:    pos.x = mapMax.x - halfW; pos.y = mapMax.y - halfH; break;
        }
        cam.transform.position = pos;
        ClampCameraToMapBounds();
    }

    // 카메라 위치를 맵 범위 안으로 제한
    private static void ClampCameraToMapBounds()
    {
        if (GridManager.Instance == null || GridManager.Instance.grid == null) return;

        var cam  = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // 맵 전체의 월드 좌표 최솟값·최댓값 (타일 셀 경계 기준)
        Vector3 mapMin = GridManager.Instance.grid.CellToWorld(new Vector3Int(0, 0, 0));
        Vector3 mapMax = GridManager.Instance.grid.CellToWorld(
            new Vector3Int(GridManager.Instance.Width, GridManager.Instance.Height, 0));

        // 맵이 화면보다 작으면 중앙에 고정, 크면 반화면 크기만큼 여유를 두고 클램프
        float minX = mapMin.x + halfW;
        float maxX = mapMax.x - halfW;
        if (minX > maxX) minX = maxX = (mapMin.x + mapMax.x) * 0.5f;

        float minY = mapMin.y + halfH;
        float maxY = mapMax.y - halfH;
        if (minY > maxY) minY = maxY = (mapMin.y + mapMax.y) * 0.5f;

        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        cam.transform.position = pos;
    }
}
