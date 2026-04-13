using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("HUD")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI goldText;

    [Header("Unit Info Panel")]
    public GameObject      unitInfoPanel;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitHpText;
    public TextMeshProUGUI unitStatsText;
    public TextMeshProUGUI unitStatusText;

    [Header("Buttons")]
    public Button endTurnButton;

    [Header("Building Panel")]
    public GameObject buildingPanel;
    public Transform  unitButtonContainer;
    public GameObject unitButtonPrefab;

    [Header("Game Over")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Settings")]
    public Button     settingsToggleButton;
    public GameObject settingsPanel;
    public UnityEngine.UI.Slider volumeSlider;

    [Header("Event Popup")]
    public GameObject       eventPanel;
    public TextMeshProUGUI  eventText;
    public Button           eventConfirmButton;
    public bool IsEventShowing  => eventPanel   != null && eventPanel.activeInHierarchy;
    public bool IsSettingsOpen  => settingsPanel != null && settingsPanel.activeInHierarchy;

    private Action _eventOnConfirm;
    private bool   _isVictory;
    private Unit   _displayedUnit;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (IsEventShowing && Input.GetMouseButtonUp(0))
        {
            eventPanel.SetActive(false);
            var cb = _eventOnConfirm;
            _eventOnConfirm = null;
            cb?.Invoke();
        }
        else if (_isVictory && Input.GetMouseButtonUp(0))
        {
            SceneManager.LoadScene("StageSelect");
        }
    }

    private void Start()
    {
        endTurnButton.onClick.AddListener(() => PlayerInputHandler.Instance.OnEndTurnPressed());

        TurnManager.Instance.OnPlayerTurnStart += RefreshForPlayerTurn;
        TurnManager.Instance.OnAllyTurnStart   += RefreshForAllyTurn;
        TurnManager.Instance.OnEnemyTurnStart  += RefreshForEnemyTurn;
        TurnManager.Instance.OnVictory         += OnVictoryHandler;
        TurnManager.Instance.OnDefeat          += () => defeatPanel?.SetActive(true);
        ResourceManager.Instance.OnGoldChanged += UpdateGold;

        settingsPanel?.SetActive(false);
        settingsToggleButton?.onClick.AddListener(ToggleSettings);

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(v => AudioListener.volume = v);
        }

        HideUnitInfo();
        UpdateGold();
    }

    // ── Settings ──────────────────────────────────────────────
    public void ToggleSettings()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(!settingsPanel.activeInHierarchy);
    }

    public void ExitToStageSelect()
    {
        SceneManager.LoadScene("StageSelect");
    }

    // ── Victory handler ──────────────────────────────────────
    private void OnVictoryHandler()
    {
        victoryPanel?.SetActive(true);
        _isVictory = true;

        string sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetInt("Cleared_" + sceneName, 1);
        PlayerPrefs.Save();
    }

    // ── Turn display ──────────────────────────────────────────
    private void RefreshForPlayerTurn()
    {
        turnText.text               = $"{TurnManager.Instance.TurnNumber}턴 - 플레이어";
        endTurnButton.interactable  = true;
        UpdateGold();
    }

    private void RefreshForAllyTurn()
    {
        turnText.text              = $"{TurnManager.Instance.TurnNumber}턴 - 동맹";
        endTurnButton.interactable = false;
    }

    private void RefreshForEnemyTurn()
    {
        turnText.text              = $"{TurnManager.Instance.TurnNumber}턴 - 적";
        endTurnButton.interactable = false;
    }

    private void UpdateGold() =>
        goldText.text = $"골드: {ResourceManager.Instance.PlayerGold}";

    // ── Unit info ─────────────────────────────────────────────
    private void PositionBottomLeft(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.zero;
        rt.pivot            = Vector2.zero;
        rt.anchoredPosition = new Vector2(20f, 20f);
    }

    private void PositionBottomCenter(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -300f);
    }

    public void ShowUnitInfo(Unit unit)
    {
        if (_displayedUnit != null)
            _displayedUnit.OnStateChanged -= RefreshUnitInfo;

        _displayedUnit = unit;
        _displayedUnit.OnStateChanged += RefreshUnitInfo;

        buildingPanel?.SetActive(false);
        unitInfoPanel.SetActive(true);
        PositionBottomLeft(unitInfoPanel);
        RefreshUnitInfo();
    }

    private void RefreshUnitInfo()
    {
        if (_displayedUnit == null) return;
        var unit = _displayedUnit;

        Debug.Log($"[GameUI] {unit.data.unitName} Rank={unit.Rank} EffAtk={unit.EffectiveAttack} EffHp={unit.EffectiveMaxHp}");

        unitNameText.text  = unit.Rank > 0
            ? $"{unit.data.unitName}  {new string('★', unit.Rank)}"
            : unit.data.unitName;
        unitHpText.text    = $"HP {unit.CurrentHp} / {unit.EffectiveMaxHp}";
        unitStatsText.text = $"공격 {unit.EffectiveAttack}  방어 {unit.data.defense}  이동 {unit.data.moveRange}  사거리 {unit.data.attackRange}  [등급:{unit.Rank}]";

        if (unitStatusText != null)
        {
            if (unit.IsExhausted)
            {
                unitStatusText.text  = "행동 불가";
                unitStatusText.color = new UnityEngine.Color(0.8f, 0.4f, 0.4f, 1f);
            }
            else if (unit.HasMoved)
            {
                unitStatusText.text  = "이동 완료 / 행동 가능";
                unitStatusText.color = new UnityEngine.Color(1f, 0.85f, 0.4f, 1f);
            }
            else if (!unit.CanAct)
            {
                unitStatusText.text  = "이동 가능 / 행동 불가";
                unitStatusText.color = new UnityEngine.Color(1f, 0.85f, 0.4f, 1f);
            }
            else
            {
                unitStatusText.text  = "대기 중";
                unitStatusText.color = new UnityEngine.Color(0.6f, 1f, 0.6f, 1f);
            }
        }
    }

    public void HideUnitInfo()
    {
        if (_displayedUnit != null)
        {
            _displayedUnit.OnStateChanged -= RefreshUnitInfo;
            _displayedUnit = null;
        }
        unitInfoPanel?.SetActive(false);
        buildingPanel?.SetActive(false);
    }

    // ── Building panel ────────────────────────────────────────
    // ── Event popup ───────────────────────────────────────────
    public void ShowEventText(string text, Action onConfirm)
    {
        if (eventPanel == null)
        {
            Debug.LogWarning("[GameUI] eventPanel이 연결되지 않았습니다. Inspector에서 할당하세요.");
            onConfirm?.Invoke();
            return;
        }

        _eventOnConfirm = onConfirm;

        eventPanel.SetActive(true);

        if (eventText != null)
            eventText.text = text;

        // 확인 버튼은 사용하지 않음 — 화면 클릭으로 닫힘
        if (eventConfirmButton != null)
            eventConfirmButton.gameObject.SetActive(false);
    }

    // ── Building panel ────────────────────────────────────────
    public void ShowBuildingPanel(Building building)
    {
        if (building.data.buildingType != BuildingType.Castle) return;

        // 성 정보: unitInfoPanel 좌하단
        unitInfoPanel.SetActive(true);
        PositionBottomLeft(unitInfoPanel);
        unitNameText.text  = building.data.buildingName;
        unitHpText.text    = $"골드/턴: +{building.data.goldProduction}";
        unitStatsText.text = $"진영: {building.faction}";
        if (unitStatusText != null) unitStatusText.text = "";

        // 유닛 생성 패널: 화면 중앙 하단
        buildingPanel.SetActive(true);
        PositionBottomCenter(buildingPanel);

        var panelBg = buildingPanel.GetComponent<UnityEngine.UI.Image>();
        if (panelBg != null) panelBg.enabled = false;

        var buildingTitleTmp = buildingPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (buildingTitleTmp != null) buildingTitleTmp.gameObject.SetActive(false);

        foreach (Transform child in unitButtonContainer)
            Destroy(child.gameObject);

        var allTypes = new[] { UnitType.Warrior, UnitType.Archer, UnitType.Lancer, UnitType.Monk };

        foreach (var uType in allTypes)
        {
            var uData    = GameManager.Instance.settings.GetUnitData(uType);
            var btn      = Instantiate(unitButtonPrefab, unitButtonContainer);
            bool unlocked = GameManager.Instance.PlayerUnlockedUnits.Contains(uType);
            bool canAfford = ResourceManager.Instance.PlayerGold >= uData.goldCost;

            // Sprite
            var icon = btn.transform.Find("UnitIcon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = uData.playerIdleSprite;
                icon.color  = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 1f);
            }

            // Name
            var nameTmp = btn.transform.Find("UnitName")?.GetComponent<TextMeshProUGUI>();
            if (nameTmp != null)
            {
                nameTmp.text  = uData.unitName;
                nameTmp.color = unlocked ? Color.black : new Color(0.35f, 0.35f, 0.35f, 1f);
            }

            // Cost
            var costTmp = btn.transform.Find("UnitCost")?.GetComponent<TextMeshProUGUI>();
            if (costTmp != null)
            {
                costTmp.text  = $"{uData.goldCost}G";
                costTmp.color = !unlocked  ? new Color(0.35f, 0.35f, 0.35f, 1f)
                              : canAfford  ? new Color(0.6f, 0.45f, 0.05f, 1f)
                              :              new Color(0.6f, 0.15f, 0.05f, 1f);
            }

            var button = btn.GetComponent<Button>();
            if (unlocked && canAfford)
            {
                button.interactable = true;
                var t = uType;
                button.onClick.AddListener(() =>
                {
                    PlayerInputHandler.Instance.OnProduceUnit(t);
                    buildingPanel.SetActive(false);
                });
            }
            else
            {
                button.interactable = false;
            }
        }
    }
}
