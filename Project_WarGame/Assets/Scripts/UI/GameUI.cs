using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        endTurnButton.onClick.AddListener(() => PlayerInputHandler.Instance.OnEndTurnPressed());

        TurnManager.Instance.OnPlayerTurnStart += RefreshForPlayerTurn;
        TurnManager.Instance.OnEnemyTurnStart  += RefreshForEnemyTurn;
        TurnManager.Instance.OnVictory         += () => victoryPanel?.SetActive(true);
        TurnManager.Instance.OnDefeat          += () => defeatPanel?.SetActive(true);
        ResourceManager.Instance.OnGoldChanged += UpdateGold;

        HideUnitInfo();
        UpdateGold();
    }

    // ── Turn display ──────────────────────────────────────────
    private void RefreshForPlayerTurn()
    {
        turnText.text               = $"Turn {TurnManager.Instance.TurnNumber} - Player";
        endTurnButton.interactable  = true;
        UpdateGold();
    }

    private void RefreshForEnemyTurn()
    {
        turnText.text              = $"Turn {TurnManager.Instance.TurnNumber} - Enemy";
        endTurnButton.interactable = false;
    }

    private void UpdateGold() =>
        goldText.text = $"Gold: {ResourceManager.Instance.PlayerGold}";

    // ── Unit info ─────────────────────────────────────────────
    public void ShowUnitInfo(Unit unit)
    {
        buildingPanel?.SetActive(false);
        unitInfoPanel.SetActive(true);
        unitNameText.text  = unit.data.unitName;
        unitHpText.text    = $"HP {unit.CurrentHp} / {unit.data.maxHp}";
        unitStatsText.text = $"ATK {unit.data.attack}  DEF {unit.data.defense}  MOV {unit.data.moveRange}  RNG {unit.data.attackRange}";

        if (unitStatusText != null)
        {
            if (unit.IsExhausted)
            {
                unitStatusText.text  = "Exhausted";
                unitStatusText.color = new UnityEngine.Color(0.8f, 0.4f, 0.4f, 1f);
            }
            else if (unit.HasMoved)
            {
                unitStatusText.text  = "Moved / Can Act";
                unitStatusText.color = new UnityEngine.Color(1f, 0.85f, 0.4f, 1f);
            }
            else if (!unit.CanAct)
            {
                unitStatusText.text  = "Can Move / Cannot Act";
                unitStatusText.color = new UnityEngine.Color(1f, 0.85f, 0.4f, 1f);
            }
            else
            {
                unitStatusText.text  = "Ready";
                unitStatusText.color = new UnityEngine.Color(0.6f, 1f, 0.6f, 1f);
            }
        }
    }

    public void HideUnitInfo()
    {
        unitInfoPanel?.SetActive(false);
        buildingPanel?.SetActive(false);
    }

    // ── Building panel ────────────────────────────────────────
    public void ShowBuildingPanel(Building building)
    {
        if (building.data.buildingType != BuildingType.Castle) return;

        HideUnitInfo();
        buildingPanel.SetActive(true);

        var buildingTitleTmp = buildingPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (buildingTitleTmp != null) buildingTitleTmp.text = "Castle";

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
                nameTmp.color = unlocked ? Color.white : new Color(0.45f, 0.45f, 0.45f, 1f);
            }

            // Cost
            var costTmp = btn.transform.Find("UnitCost")?.GetComponent<TextMeshProUGUI>();
            if (costTmp != null)
            {
                costTmp.text  = $"{uData.goldCost}G";
                costTmp.color = !unlocked       ? new Color(0.45f, 0.45f, 0.45f, 1f)
                              : canAfford        ? new Color(1f, 0.85f, 0.3f, 1f)
                              :                   new Color(0.9f, 0.3f, 0.3f, 1f);
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
