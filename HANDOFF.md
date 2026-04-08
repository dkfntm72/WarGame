# WarGame — 작업 인수인계 (HANDOFF)

최종 업데이트: 2026-04-08 (세션3)

---

## 프로젝트 개요

Unity 6 (URP 2D) 기반 모바일 턴제 전략 게임.
유럽전쟁 모티브, 시나리오 모드 전용, 스테이지 형식.

- **Unity 버전:** 6000.0.58f2
- **렌더 파이프라인:** URP 2D
- **주요 에셋:** Tiny Swords (건물/유닛/타일맵)
- **씬:** `Assets/Stage01.unity` (실행 중인 씬), `Assets/Scenes/Stage01.unity`
- **UI 폰트:** `Assets/Fonts/경기천년제목OTF_Bold SDF.asset` (TMP 기본 폰트로 설정됨)

---

## 현재 구현 상태

### 핵심 시스템 (완료)

| 스크립트 | 위치 | 역할 |
|---|---|---|
| `GameManager.cs` | Core/ | 게임 흐름 총괄, 셀 클릭 처리, 유닛 소환, 건물 점령, 언락 관리 |
| `TurnManager.cs` | Core/ | 턴 전환 (Player ↔ Enemy) |
| `ResourceManager.cs` | Core/ | 골드 수입/지출 |
| `EventTriggerManager.cs` | Core/ | 이벤트 트리거 시스템 |
| `GridManager.cs` | Grid/ | 타일맵 ↔ 논리 격자 연동 |
| `Pathfinding.cs` | Grid/ | BFS 이동 범위/경로 탐색 |
| `TileNode.cs` | Grid/ | 격자 셀 데이터 |
| `Unit.cs` | Units/ | 이동, 공격, 체력바, 애니메이터 |
| `HealthBar.cs` | Units/ | 유닛 HP바 컴포넌트 |
| `DamageNumber.cs` | Units/ | 피격 시 떠오르는 데미지/회복 숫자 표시 |
| `RankBar.cs` | Units/ | 유닛 등급 막대 UI (0~5, World-Space Canvas 자동 생성) |
| `AllyAI.cs` | AI/ | 동맹군 자동 행동 AI |
| `FactionHelper.cs` | Data/ | `IsHostileTo()` 진영 적대 관계 유틸리티 |
| `Building.cs` | Buildings/ | 점령, 유닛 생산 |
| `EnemyAI.cs` | AI/ | 적 턴 자동 행동 |
| `PlayerInputHandler.cs` | Input/ | New Input System 래핑 |
| `CameraDragController.cs` | Input/ | 좌클릭 드래그 카메라 이동, 맵 경계 클램프 |
| `GameUI.cs` | UI/ | 턴 표시, 유닛 패널, 건물 패널, 승리/패배, 이벤트 팝업, 설정 패널 |
| `MainMenuUI.cs` | UI/ | 메인 메뉴 (시작→StageSelect, 설정) |
| `StageSelectUI.cs` | UI/ | 스테이지 선택 화면, 클리어 배지 표시 |

### 데이터 (ScriptableObject)

| 에셋 | 파일 | 설명 |
|---|---|---|
| GameSettings | `Assets/GameData/GameSettings.asset` | 유닛/건물 데이터, 타일, 프리팹 참조 |
| Stage01 (MapData) | `Assets/GameData/Maps/Stage01.asset` | 스테이지 맵 데이터 |
| UnitData ×4 | `Assets/GameData/` | Warrior/Archer/Lancer/Monk 스탯 |
| BuildingData ×4 | `Assets/GameData/` | Castle/ArcheryRange/Cathedral/House |

---

## 최근 작업 내역

### 2026-04-08 (세션3) — 동맹군 시스템 / 반격 / 새 유닛 타입 / 감지 범위 / 등급 시스템

#### 동맹군(Ally) 시스템

- `Faction.Ally` 추가 (`Enums.cs`)
- `GameState.AllyTurn` 추가 — 턴 순서: **PlayerTurn → AllyTurn → EnemyTurn**
- **`TurnManager.cs`** 수정
  - `EndPlayerTurn()`: 동맹군 존재 시 `StartAllyTurn()`, 없으면 바로 `StartEnemyTurn()`
  - `StartAllyTurn()` / `EndAllyTurn()` 추가
  - `OnAllyTurnStart` 이벤트 추가
  - 승리 조건 분기: `VictoryCondition.AnnihilateEnemy` / `CaptureEnemyCastle` 맵별 적용
  - 패배 조건: 아군 성 + 유닛 모두 소멸 (승리 조건과 독립)
- **`AllyAI.cs`** 신규 (`Scripts/AI/`)
  - `ExecuteTurn()` 코루틴 — 동맹군 유닛 순서대로 자동 행동
  - 적 유닛 탐색 시 `DetectRange` 적용
  - Monk일 경우 체력 낮은 아군(Player/Ally) 치료 우선
- **`GameManager.cs`** 수정
  - `AllyUnits` 리스트 추가
  - `SpawnUnit()` — `rank`, `detectRange` 파라미터 추가
  - `HandleUnitDied()`에서 `AllyUnits.Remove()` 처리
  - 맵 로드 후 `CameraDragController.PositionAtMapEdge()` 호출
- **`GameUI.cs`** 수정 — `OnAllyTurnStart` 구독, "Ally" 턴 텍스트 표시
- 동맹군 애니메이터 컨트롤러 4종 신규: `Ally_Warrior/Archer/Lancer/Monk_Animator.controller`
- **`UnitData.cs`** — `allyAnimController`, `allyIdleSprite` 필드 추가, `GetAnimController()`/`GetIdleSprite()` Ally 분기 추가
- **`EnemyAI.cs`** — `FindNearestPlayerUnit()` → `FindNearestHostileUnit()` 변경 (PlayerUnits + AllyUnits 합산, DetectRange 적용)

#### 새 유닛 타입

- **EliteWarrior** (정예 전사) — `UnitType.EliteWarrior`, `UnitData.isElite = true`, 생산 불가, 플레이어 전용
  - `EliteWarrior.asset`, `EliteWarrior_Animator.controller` 신규
  - 에디터 스크립트: `CreateEliteWarriorData.cs`
- **Tower** — `UnitType.Tower`, `moveRange = 0` (이동 불가), `attackRange = 3`, 생산 불가
  - `Tower.asset` 신규
  - `Unit.Initialize()`: Tower 타입이면 `localScale = 0.5f`, HP바 스케일 상쇄(2×), 타워 위 궁수 오버레이(`ArcherOverlay`) 자동 생성
  - `Unit.CanMove`: `data.moveRange > 0` 조건 추가 → Tower 이동 불가
- **`FactionHelper.cs`** 신규 (`Scripts/Data/`) — `IsHostileTo(Faction a, Faction b)` 유틸리티

#### 반격(Counter-Attack) 시스템

- **`Unit.cs`** 수정
  - `CanCounter(Unit attacker)`: Monk 제외, 적대 관계이고 공격자가 내 사거리 내일 때 `true`
  - `CounterAttack(Unit attacker)` 코루틴: 공격력 50% × 0.5f, 방어력 적용, 반격 경험치 +3
  - `Attack()` 코루틴 끝에 `if (target.IsAlive && target.CanCounter(this))` 반격 호출

#### 적 AI 감지 범위(DetectRange)

- **`UnitData.cs`** — `detectRange` 필드 추가 (기본값 6)
- **`Unit.cs`** — `DetectRange` 프로퍼티, `SetDetectRange(int)` 메서드
- **`GameManager.SpawnUnit()`** — `detectRange` 파라미터로 개별 설정 가능
- `EnemyAI` / `AllyAI` 모두 감지 범위 밖 유닛 무시

#### 등급(Rank) / 경험치 시스템

- **`Unit.cs`** 수정
  - `Rank` 프로퍼티 (0~5), `_exp` 필드, `ExpPerRank = 50`
  - `EffectiveAttack = data.attack + Rank × 2`
  - `EffectiveMaxHp  = data.maxHp  + Rank × 10`
  - `SetRank(int)`: HP 상승분 반영, HP바·랭크바 갱신, `OnStateChanged` 호출
  - `GainExp(int)`: 경험치 누적, 50 달성 시 등급 상승 (이월 적용, while 루프), 5등급 이후 고정
  - 공격/회복/피격 모두 `Effective` 스탯 사용
- **`GameManager.SpawnUnit()`** — `rank` 파라미터, `RankBar` 컴포넌트 자동 추가
- **`GameUI.ShowUnitInfo()`** — 등급 수만큼 ★ 표시, `EffectiveAttack`/`EffectiveMaxHp` 표시, `[R:{rank}]`
- **`RankBar.cs`** 신규 (`Scripts/Units/`)
  - `Awake()`에서 World-Space Canvas + 5개 직사각형 막대 자동 생성
  - 0등급: 숨김 / 1~5등급: 등급 수만큼 금색(#FFD21A) 막대 표시
  - 위치: `localPosition.y = -0.55f` (HP바 아래), `localScale = 0.01`, sortingOrder 6

**경험치 획득 규칙:**

| 상황 | 경험치 |
|---|---|
| 공격 시 | 5~10 (랜덤) |
| 피격 시 (생존) | 3~6 (랜덤) |
| 적 처치 시 | +20 (공격 경험치에 추가) |
| 반격 성공 시 | +3 |
| 사망 시 | 없음 |
| 5등급 도달 후 | 경험치 0으로 고정 |

#### 기타 변경

- **`Enums.cs`** — `CameraEdge` (BottomLeft/Right, TopLeft/Right), `EventConditionType.OnEnemyTurnStart` 추가, `TerrainType.Grass2` 추가
- **`MapData.cs`** — `cameraStartEdge` 필드 추가
- `Grass2config.asset` 신규 (`Assets/GameData/`)
- **`GameUI.cs`** — 유닛 패널 `OnStateChanged` 구독으로 실시간 갱신 (`_displayedUnit`), `PositionBottomLeft()` / `PositionBottomCenter()` 헬퍼 추가

---

### 2026-04-06 (세션2) — 전투 랜덤성 / 이동 차단 / 유닛 등급 시스템 / 경험치 시스템

#### 공격력 무작위성 (±5)

- **`Unit.cs`** `Attack()` 수정
  - Monk 제외 모든 유닛: 기본 공격력에 `Random.Range(-5, 6)` 추가
  - `EffectiveAttack ± 5 - 방어력` 으로 최종 데미지 계산

#### 유닛 이동 시 적 통과 불가

- **`GridManager.GetMovementRange()`** 수정
  - BFS 탐색 시 적 진영 유닛 점령 타일은 큐에 추가하지 않음 → 이동 가능 범위 자체가 적 뒤쪽으로 뚫리지 않음
- **`Pathfinding.FindPath()`** 수정
  - `faction` 파라미터 추가 (기본값 `Faction.Neutral`)
  - 경유 타일에 적 유닛이 있으면 해당 경로 차단 (목적지 타일 자체는 예외)
- **`PlayerInputHandler.cs`** — `FindPath` 호출 시 `Faction.Player` 전달
- **`EnemyAI.cs`** — `FindPath` 호출 시 `Faction.Enemy` 전달

#### 유닛 등급 시스템 (0~5)

- **`RankBar.cs`** 신규 (`Scripts/Units/`)
  - `Awake()`에서 World-Space Canvas + 5개 직사각형 막대 자동 생성
  - 0등급: 숨김 / 1~5등급: 등급 수만큼 금색 막대 표시
  - HP바 아래 위치 (`localPosition.y = -0.55f`, `localScale = 0.01`)
- **`Unit.cs`** 수정
  - `Rank` 프로퍼티 (0~5), `_exp` 필드 추가
  - `EffectiveAttack = data.attack + Rank × 2`
  - `EffectiveMaxHp  = data.maxHp  + Rank × 10`
  - `SetRank(int)`: HP 상승분 반영, HP바·랭크바 갱신
  - `GainExp(int)`: 경험치 누적, 50 달성 시 등급 상승 (이월 적용, while 루프)
  - 공격/회복/피격 모두 `Effective` 스탯 사용
- **`GameManager.SpawnUnit()`** 수정 — `RankBar` 컴포넌트 없으면 자동 추가

#### 경험치 획득 규칙

| 상황 | 경험치 |
|---|---|
| 공격 시 | 5~10 (랜덤) |
| 피격 시 (생존) | 3~6 (랜덤) |
| 적 처치 시 | +20 (공격 경험치에 추가) |
| 사망 시 | 없음 |
| 5등급 도달 후 | 경험치 0으로 고정 |

- 필요 경험치: 50 (등급당 통일)
- 이월 적용: 잉여 경험치 다음 등급으로 누적, 연속 등급 상승 가능
- 경험치량 UI 표시 없음

---

### 2026-04-06 — 이벤트 대사창 UI 개편 / 유닛 사망 이펙트

#### 이벤트 팝업 → 하단 대사창 스타일

- **`CreateEventPanel.cs`** 수정
  - 기존: 전체화면 어두운 반투명 오버레이
  - 변경: 화면 하단 RPG 대사창 스타일
  - 구조:
    ```
    EventPanel          (전체화면 투명 — 입력 차단)
      └── DialogBox     (검정 반투명 테두리, 가로 75%)
            └── DialogInner  (검정 반투명 내부)
                  └── EventText    (흰색 텍스트)
    ```
  - 배치: anchorMin.x=0.1625, anchorMax.x=0.8375 / 세로 화면 4%~36%
  - 클릭으로 닫기 동작 유지
  - 씬에 적용하려면: **Window > WarGame > Create Event Panel** 실행

#### 유닛 사망 시 Explosion 이펙트

- **`GameSettings.cs`** 수정 — `deathEffectPrefab` 필드 추가 (Effects 헤더)
- **`Unit.cs`** 수정
  - `Die()`: 스프라이트·HP바 즉시 숨기고 `DeathRoutine()` 코루틴 시작
  - `DeathRoutine()`: `deathEffectPrefab` 인스턴스 생성 → 0.85초 후 유닛 제거
  - 버그 수정: `healthBar.gameObject.SetActive(false)` → `healthBar.enabled = false`
    (유닛 GameObject가 비활성화되어 코루틴 시작 불가 오류 해결)
- **`CreateDeathEffect.cs`** 신규 (`Assets/Scripts/Editor/`)
  - `Assets/Prefabs/DeathEffect.prefab` 자동 생성
  - Explosion 1 Animation 컨트롤러 사용 (0.8초, 비루프)
  - GameSettings.deathEffectPrefab에 자동 연결
  - 실행: **Window > WarGame > Create Death Effect Prefab**

#### 확정된 설계 결정

- **Slope 지형 이동 비용 미적용** — 시각적 배경 레이어만 사용 (의도적 결정)

---

### 2026-04-05 — 승리 후 씬 전환 / 스테이지 클리어 표시 / 설정 패널 / 버그 수정

#### 승리 후 화면 클릭 → StageSelect 이동

- **`GameUI.cs`** 수정
  - `OnVictoryHandler()` 신규: victoryPanel 활성화 + `_isVictory = true` + `PlayerPrefs.SetInt("Cleared_" + sceneName, 1)` 저장
  - `Update()`: `_isVictory && MouseButtonUp` 감지 → `SceneManager.LoadScene("StageSelect")`
  - 이벤트 팝업이 열려 있으면 팝업 먼저 닫고, 다음 클릭에 이동

#### 스테이지 선택 화면 클리어 배지

- **`StageSelectUI.cs`** 수정
  - `Start()`에서 `PlayerPrefs.GetInt("Cleared_Stage01")` 확인
  - 클리어된 스테이지 버튼에 **"★ CLEAR"** (금색 Bold) 배지 동적 생성
  - 기존 버튼 TMP의 폰트를 자동 상속

#### PlayerPrefs 클리어 저장 방식

- 씬 이름 기반: `"Cleared_" + SceneManager.GetActiveScene().name`
- StageSelect에서 같은 키로 읽어서 배지 표시
- 개발 중 초기화: **WarGame > Clear Save Data (PlayerPrefs)** 메뉴 (`Assets/Editor/ClearSaveData.cs`)

#### 설정 패널 (Stage01 씬, 우상단)

- **`GameUI.cs`** 수정
  - 필드 추가: `settingsToggleButton`, `settingsPanel`, `volumeSlider`
  - `IsSettingsOpen` 프로퍼티 추가
  - `ToggleSettings()` — 패널 열기/닫기 토글
  - `ExitToStageSelect()` — StageSelect 씬 로드
  - Start()에서 volumeSlider → `AudioListener.volume` 연동
- **씬 UI 구조** (`UI/SettingsButton`, `UI/SettingsPanel`):
  ```
  UI/SettingsButton          설정 버튼 (우상단 56×56)
  UI/SettingsPanel           설정 패널 (280×220, 기본 비활성)
    ├─ TopBar
    │   ├─ TitleText         "설정"
    │   └─ CloseButton (✕)   패널 닫기
    ├─ VolumeLabel           "음량"
    ├─ VolumeSlider          AudioListener.volume 연동
    └─ ExitButton            "스테이지 선택으로" → StageSelect
  ```
- **에디터 스크립트**: `Assets/Editor/CreateSettingsPanel.cs`, `Assets/Editor/FixSettingsButton.cs`
  - Button targetGraphic 미설정 버그 수정 포함

#### 설정 패널 열림 시 입력 차단

- **`CameraDragController.cs`** 수정: `IsSettingsOpen` 시 Update() 즉시 리턴 → 카메라 드래그 차단
- **`PlayerInputHandler.cs`** 수정: `IsSettingsOpen` 시 타일 클릭 차단

#### 버그 수정

- **`PlayerInputHandler.cs`** — `RefreshHighlights()` 진입 시 `if (selectedUnit == null) return;` 추가
  - 원인: `MoveAndFollowUp` 코루틴 yield 중 유닛 소멸 → selectedUnit이 fake-null 상태로 CanMove 접근 시 NullReferenceException

---

### 2026-04-03 (E) — 이벤트 시스템 완성 / 씬 버그 수정

#### 스테이지 선택 화면 신규 추가

- **`StageSelectUI.cs`** (`Scripts/UI/`) 신규
  - 스테이지 1~5 카드 버튼 (스테이지 1만 활성, 나머지 "준비중")
  - "메인화면으로" 버튼 → MainMenu 씬 로드
- **`BuildStageSelectScene.cs`** (`Assets/Editor/`) — 씬 자동 생성
- **`MainMenuUI.cs`** 수정 — 시작 버튼 → `Stage01` 대신 `StageSelect` 로드
- 빌드 세팅 index: MainMenu=0, StageSelect=1, Stage01=2

#### Stage01 씬 누락 컴포넌트 추가

- `Main Camera` → `CameraDragController` 컴포넌트 추가
- `Managers/EventTriggerManager` GameObject 신규 추가

#### 이벤트 트리거 조건 2개 추가

- **`OnStageStart`** — 스테이지 로드 직후 1회 발동
- **`OnVictory`** — 승리 조건 달성 시 발동
- `Enums.cs` `EventConditionType` 에 두 값 추가

#### 이벤트 텍스트 팝업 UX 변경

- 확인 버튼 제거 → 화면 아무 곳 클릭으로 닫힘
- `GameUI.Update()` 에서 `IsEventShowing && Input.GetMouseButtonUp(0)` 감지

---

### 2026-04-03 (D) — 메인 메뉴 씬

- **`MainMenuUI.cs`** (`Scripts/UI/`) 신규
- **`BuildMainMenuScene.cs`** (`Assets/Editor/`) — 씬 자동 생성
- 빌드 세팅에 MainMenu 씬 index 0으로 추가

---

### 2026-04-03 (C) — UI 배치 / 폰트 / 이벤트 트리거 범위

- 성 선택 시 패널 분리: unitInfoPanel → 좌하단 / buildingPanel → 중앙 하단
- `Assets/Fonts/경기천년제목OTF_Bold SDF.asset` 추가, TMP 기본 폰트 설정
- `EventTrigger` 단일 좌표 → 직사각형 범위 (x1,y1,x2,y2)

---

### 2026-04-03 (B) — 이벤트 트리거 / 체력바 / 언락 시스템

- `EventTriggerManager.cs` 신규 (OnTileEnter / OnTurnStart / OnStageStart / OnVictory)
- `HealthBar.cs`, `DamageNumber.cs` 신규
- 유닛 언락 시스템 (ArcheryRange→Archer, Cathedral→Monk)

---

### 2026-04-03 (A) — UI 및 유닛 색상

- 유닛 생산 패널 화면 중앙 고정
- `Unit.RefreshColor()`, `HasAttackableTarget()` 추가

---

### 2026-04-02 — Slope 지형 시스템

- Slope 배경 레이어 자동 생성
- `GridManager.CanTransition()` public static

---

### 2026-04-01 — 맵 에디터 / 카메라 / AI / 승리조건

- 맵 에디터 버그 수정
- `CameraDragController.cs` 신규
- `VictoryCondition` enum: `AnnihilateEnemy` / `CaptureEnemyCastle`

---

## 이벤트 트리거 조건 전체 목록

| 조건 | 발동 시점 | 추가 파라미터 |
|---|---|---|
| `OnTileEnter` | 유닛이 지정 범위 진입 | x1,y1,x2,y2 범위 |
| `OnTurnStart` | 플레이어 턴 N 시작 | turnNumber |
| `OnStageStart` | 스테이지 로드 직후 | 없음 |
| `OnVictory` | 승리 조건 달성 | 없음 |

## 트리거 액션 전체 목록

| 액션 | 동작 |
|---|---|
| `ShowText` | 전체화면 팝업 텍스트 표시, 클릭으로 닫힘 |
| `SpawnUnit` | 지정 좌표에 유닛 소환 |

---

## 유닛 스탯

| 유닛 | HP | 이동 | 방어 | 사거리 | 공격 | 비용 |
|---|---|---|---|---|---|---|
| 검사 (Warrior) | 100 | 3 | 5 | 1 | 15 | 50G |
| 궁수 (Archer) | 60 | 2 | 0 | 3 | 20 | 60G |
| 창병 (Lancer) | 80 | 3 | 3 | 1 | 10 | 40G |
| 회복사 (Monk) | 40 | 2 | 0 | 2 | 10(회복) | 30G |

---

## 알려진 이슈

1. **미커밋 파일 다수** — 신규 스크립트 및 에셋 다수 스테이징 안 됨.

---

## 다음 작업 제안

- [ ] Stage01 맵 완성 — 건물/유닛 초기 배치 확정
- [ ] 승리/패배 UI 연출 개선
- [ ] 2스테이지 이상 씬/MapData 추가
- [ ] 미커밋 파일 정리 후 커밋
- [ ] 설정 패널 — 음량 설정 PlayerPrefs 저장/로드 (현재 임시)

---

## 에디터 사용법 요약

```
Window > WarGame > Map Editor                  # 맵 타일/건물/유닛/트리거 수동 편집
Window > WarGame > Build Stage Select Scene    # StageSelect 씬 재생성
Window > WarGame > Create Event Panel          # Stage01 EventPanel 재생성 및 GameUI 연결 (하단 대사창 스타일)
Window > WarGame > Create Death Effect Prefab  # DeathEffect 프리팹 생성 및 GameSettings 연결
Window > WarGame > Fix Stage01 Missing Managers # EventTriggerManager 누락 시 추가
WarGame > Clear Save Data (PlayerPrefs)        # 세이브 데이터 전체 초기화 (개발용)
```

테스트: **Window > General > Test Runner** (테스트 스크립트 미작성 상태)
