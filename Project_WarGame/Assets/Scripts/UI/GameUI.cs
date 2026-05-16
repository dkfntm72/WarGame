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
    public Button speedButton;
    public Button adButton;

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
    public Button     exitButton;
    public UnityEngine.UI.Slider volumeSlider;
    public TextMeshProUGUI conditionsOfVictoryText;

    [Header("Dialogue Box")]
    public GameObject       eventPanel;
    public TextMeshProUGUI  eventText;          // 대사 본문
    public TextMeshProUGUI  dialogueSpeakerText; // 화자 이름
    public Button           eventConfirmButton;  // 미사용 (호환용)
    public bool IsEventShowing  => eventPanel   != null && eventPanel.activeInHierarchy;
    public bool IsSettingsOpen  => settingsPanel != null && settingsPanel.activeInHierarchy;

    private Action   _eventOnConfirm;
    private bool     _isVictory;
    private bool     _is2x;
    private Unit     _displayedUnit;
    // 대화 진행 상태
    private string[] _dialogueLines;
    private string   _dialogueSpeakerName;
    private int      _dialogueIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (IsEventShowing && Input.GetMouseButtonUp(0))
        {
            AdvanceDialogue();
        }
        else if (_isVictory && Input.GetMouseButtonUp(0))
        {
            SceneManager.LoadScene("StageSelect");
        }
    }

    private void AdvanceDialogue()
    {
        _dialogueIndex++;
        if (_dialogueLines == null || _dialogueIndex >= _dialogueLines.Length)
        {
            eventPanel.SetActive(false);
            var cb = _eventOnConfirm;
            _eventOnConfirm = null;
            cb?.Invoke();
        }
        else
        {
            ShowDialogueLine(_dialogueIndex);
        }
    }

    private void ShowDialogueLine(int index)
    {
        if (eventText != null)
            eventText.text = _dialogueLines[index];

        // 화자 표시: 해당 줄에 "이름:" 형식이 있으면 덮어씀
        string speaker = _dialogueSpeakerName ?? "";
        if (dialogueSpeakerText != null)
        {
            dialogueSpeakerText.text = speaker;
            dialogueSpeakerText.transform.parent.gameObject.SetActive(!string.IsNullOrWhiteSpace(speaker));
        }
    }

    private void Start()
    {
        endTurnButton.onClick.AddListener(() => PlayerInputHandler.Instance.OnEndTurnPressed());
        speedButton?.onClick.AddListener(ToggleSpeed);
        adButton?.onClick.AddListener(OnAdButtonClicked);

        TurnManager.Instance.OnPlayerTurnStart += RefreshForPlayerTurn;
        TurnManager.Instance.OnAllyTurnStart   += RefreshForAllyTurn;
        TurnManager.Instance.OnEnemyTurnStart  += RefreshForEnemyTurn;
        TurnManager.Instance.OnVictory         += OnVictoryHandler;
        TurnManager.Instance.OnDefeat          += () => defeatPanel?.SetActive(true);
        ResourceManager.Instance.OnGoldChanged += UpdateGold;

        settingsPanel?.SetActive(false);
        settingsToggleButton?.onClick.AddListener(ToggleSettings);

        if (conditionsOfVictoryText != null && GameManager.Instance?.currentMap != null)
            conditionsOfVictoryText.text = GameManager.Instance.currentMap.winLossDescription;

        if (exitButton == null)
            exitButton = settingsPanel?.transform.Find("ExitButton")?.GetComponent<Button>();
        exitButton?.onClick.AddListener(ExitToStageSelect);

        // CloseButton — 설정창 닫기
        var closeBtn = settingsPanel?.transform.Find("CloseButton")?.GetComponent<Button>();
        closeBtn?.onClick.AddListener(ToggleSettings);

        if (volumeSlider != null)
        {
            volumeSlider.value   = PlayerPrefs.GetFloat("Volume", 1f);
            AudioListener.volume = volumeSlider.value;
            volumeSlider.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = v;
                PlayerPrefs.SetFloat("Volume", v);
                PlayerPrefs.Save();
            });
        }

        HideUnitInfo();
        UpdateGold();
    }

    // ── Speed toggle ──────────────────────────────────────────
    private void ToggleSpeed()
    {
        _is2x = !_is2x;
        Time.timeScale = _is2x ? 2f : 1f;
        var label = speedButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = _is2x ? "2배속" : "1배속";
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
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
        turnText.text              = $"{TurnManager.Instance.TurnNumber}턴 - 플레이어";
        endTurnButton.interactable = true;
        RefreshAdButton();
        UpdateGold();
    }

    private void RefreshForAllyTurn()
    {
        turnText.text              = $"{TurnManager.Instance.TurnNumber}턴 - 동맹";
        endTurnButton.interactable = false;
        if (adButton != null) adButton.interactable = false;
    }

    private void RefreshForEnemyTurn()
    {
        turnText.text              = $"{TurnManager.Instance.TurnNumber}턴 - 적";
        endTurnButton.interactable = false;
        if (adButton != null) adButton.interactable = false;
    }

    public void RefreshAdButton()
    {
        if (adButton == null) return;
        bool canShow = AdManager.Instance != null && AdManager.Instance.CanShowAd;
        adButton.interactable = canShow;
        var label = adButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            int remaining = AdManager.Instance?.AdsRemaining ?? 0;
            label.text = $"광고 ({remaining})";
        }
    }

    private void OnAdButtonClicked()
    {
        AdManager.Instance?.ShowRewardedAd();
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
        rt.anchoredPosition = new Vector2(-200f, -300f);
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
    // ── Dialogue ──────────────────────────────────────────────
    /// <summary>연속 대사를 순서대로 표시. 클릭할 때마다 다음 줄로 진행.</summary>
    public void ShowDialogue(string[] lines, string speakerName, Action onConfirm)
    {
        if (eventPanel == null)
        {
            Debug.LogWarning("[GameUI] eventPanel이 연결되지 않았습니다.");
            onConfirm?.Invoke();
            return;
        }
        if (lines == null || lines.Length == 0)
        {
            onConfirm?.Invoke();
            return;
        }

        _dialogueLines       = lines;
        _dialogueSpeakerName = speakerName ?? "";
        _dialogueIndex       = 0;
        _eventOnConfirm      = onConfirm;

        if (eventConfirmButton != null)
            eventConfirmButton.gameObject.SetActive(false);

        eventPanel.SetActive(true);
        ShowDialogueLine(0);
    }

    public void ShowNotice(string message)
    {
        if (unitStatusText != null) unitStatusText.text = message;
    }

    /// <summary>레거시 단일 텍스트 호환용.</summary>
    public void ShowEventText(string text, Action onConfirm) =>
        ShowDialogue(new[] { text }, "", onConfirm);

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
                var b = building;
                button.onClick.AddListener(() =>
                {
                    PlayerInputHandler.Instance.OnProduceUnit(t, b);
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
