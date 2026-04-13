# WarGame Handoff — 2026-04-11

## 이번 세션 작업 내용

### 1. Stage01 Canvas UI 신규 생성

**배경:**
- `Assets/Stage01.unity`에 Canvas 오브젝트가 전혀 없어 `GameUI.Instance == null` 상태였음
- EventTrigger ShowText 호출 시 NullReferenceException 크래시 발생
- `WarGameSceneSetup.cs`가 `Assets/Scenes/Stage01.unity`에만 씬을 생성해 현재 씬에는 적용된 적 없음

**해결:**
- `Assets/Scripts/Editor/BuildGameUI.cs` 에디터 스크립트 신규 작성
- 스크립트 실행으로 Canvas UI 전체 생성 + GameUI 필드 연결

**생성된 UI 구성:**

| 오브젝트 | 설명 |
|---|---|
| `UI` (Canvas) | ScreenSpaceOverlay, sortingOrder=10, ScaleWithScreenSize 1080×1920 |
| `UI/HUD` | 상단바 — SpecialPaper_0 Sliced 배경 (금장 테두리), 높이 120 |
| `UI/HUD/TurnText` | 좌측, 폰트 36px |
| `UI/HUD/GoldText` | 중앙, 폰트 36px |
| `UI/HUD/SettingsButton` | 우측, Icon_10 톱니바퀴 아이콘 (텍스트 없음), 80×80 |
| `UI/EndTurnButton` | 우하단, SmallBlueSquareButton_Regular_0 Sliced, 160×160 |
| `UI/UnitInfoPanel` | 좌하단, 320×170, 유닛 선택 시 표시 |
| `UI/BuildingPanel` | 중앙 y=-300, 770×280, 성 클릭 시 표시 |
| `UI/VictoryPanel` | 중앙, 600×320, 승리 시 표시 |
| `UI/DefeatPanel` | 중앙, 600×320, 패배 시 표시 |
| `UI/SettingsPanel` | 중앙, 500×300, 볼륨 슬라이더 + 나가기 버튼 |
| `UI/EventPanel` | 전체화면 반투명 오버레이, 이벤트 팝업 |

---

### 2. GameUI.cs 수정

- `PositionBottomCenter()` — 앵커 중앙 기준 y=-300으로 변경 (런타임 위치 덮어쓰기 방지)

---

### 3. 사용된 스프라이트

| 스프라이트 | 용도 |
|---|---|
| `Tiny Swords/UI Elements/Papers/SpecialPaper.png` → `SpecialPaper_0` | HUD 상단바 배경 |
| `Tiny Swords/UI Elements/Buttons/SmallBlueSquareButton_Regular.png` → `SmallBlueSquareButton_Regular_0` | End Turn 버튼 |
| `Tiny Swords/UI Elements/Icons/Icon_10.png` | 설정 버튼 아이콘 |

---

## 미해결 이슈

| 이슈 | 원인 | 권장 해결책 |
|---|---|---|
| 음량 슬라이더 PlayerPrefs 저장 | 미구현 | PlayerPrefs 연동 |

---

---

## 이번 세션 작업 내용 (2026-04-12) — 2차

### 1. UI 전체 한글화 (`GameUI.cs`)

| 영어 | 한글 |
|---|---|
| `Turn X - Player/Ally/Enemy` | `X턴 - 플레이어/동맹/적` |
| `Gold: N` | `골드: N` |
| `ATK/DEF/MOV/RNG/[R:]` | `공격/방어/이동/사거리/[등급:]` |
| `Exhausted` | `행동 불가` |
| `Moved / Can Act` | `이동 완료 / 행동 가능` |
| `Can Move / Cannot Act` | `이동 가능 / 행동 불가` |
| `Ready` | `대기 중` |
| `Gold/Turn:` | `골드/턴:` |
| `Faction:` | `진영:` |

`BuildGameUI.cs` 초기값도 동일하게 한글화 (턴 종료, 유닛 훈련, 승리!, 패배..., 설정, 음량, 스테이지 선택)

---

### 2. UI 패널 스프라이트 변경 (`BuildGameUI.cs`)

- **UnitInfoPanel** → `RegularPaper_0` (Sliced), 텍스트 검은색, 크기 320×170
- **BuildingPanel / VictoryPanel / DefeatPanel / SettingsPanel** → `SpecialPaper_0` (Sliced)
- `SetImgSliced()` 헬퍼 메서드 추가

---

### 3. UnitButton 프리팹 개선

- 배경 스프라이트 → `RegularPaper_0` (Sliced) (`SetUnitButtonSprite.cs`)
- 폰트 → `경기천년제목OTF_Bold SDF` (`FixUnitButton.cs`)
- 아웃라인 머티리얼 적용 → `경기천년제목OTF_Bold SDF - Outline.mat`
- UnitName 텍스트 색상 → 검은색 (잠금 시 어두운 회색)
- UnitCost 텍스트 색상 → 어두운 노란색 (골드 부족 시 어두운 적색)

---

### 4. TMP 아웃라인 머티리얼 생성

- 경로: `Assets/Fonts/경기천년제목OTF_Bold SDF - Outline.mat`
- OutlineColor: 검은색, OutlineWidth: 0.3
- Canvas 내 모든 TMP에 일괄 적용 (BuildGameUI.Execute 실행 시 자동)
- UnitButton 프리팹 TMP(UnitName, UnitCost)에도 적용

---

### 신규 Editor 스크립트

| 파일 | 역할 |
|---|---|
| `Assets/Scripts/Editor/SetUnitButtonSprite.cs` | UnitButton 프리팹 폰트+아웃라인 적용 |
| `Assets/Scripts/Editor/FixUnitButton.cs` | UnitButton 프리팹 폰트+아웃라인 일괄 Fix |
| `Assets/Scripts/Editor/DiagnoseOutline.cs` | TMP 아웃라인 적용 상태 진단용 |

---

## 이번 세션 작업 내용 (2026-04-12) — 1차

### 타워 체력바 위치/크기 보정

- **원인:** Tower 초기화 시 `transform.localScale = (0.5, 0.5, 1)` 설정으로 자식인 HealthBarRoot 월드 크기가 절반으로 줄어들고, anchoredPosition.y도 절반 높이로 렌더링됨
- **수정:** `Unit.Initialize()` Tower 분기에서 HealthBarRoot 보정
  - `localScale`: 0.01 → 0.02 (부모 scale 0.5 상쇄, 월드 크기 일반 유닛과 동일)
  - `anchoredPosition.y`: 0.6 → 0.7 (타워 스프라이트 높이 반영)
- **변경 파일:** `Assets/Scripts/Units/Unit.cs`

---

## 변경된 파일 목록

| 파일 | 변경 내용 |
|---|---|
| `Assets/Stage01.unity` | Canvas UI 추가 (GameUI 포함 전체 구성) |
| `Assets/Scripts/UI/GameUI.cs` | 한글화, UnitButton 텍스트 색상 변경, PositionBottomCenter() 수정 |
| `Assets/Scripts/Editor/BuildGameUI.cs` | 한글화, 패널 스프라이트 변경, 아웃라인 머티리얼 적용 |
| `Assets/Fonts/경기천년제목OTF_Bold SDF - Outline.mat` | 신규 — TMP 아웃라인 머티리얼 |
| `Assets/Prefabs/UnitButton.prefab` | 스프라이트, 폰트, 아웃라인 적용 |
| `Assets/Scripts/Editor/SetUnitButtonSprite.cs` | 신규 |
| `Assets/Scripts/Editor/FixUnitButton.cs` | 신규 |
| `Assets/Scripts/Editor/DiagnoseOutline.cs` | 신규 (진단용) |

---

## 이전 세션 작업 내용 (2026-04-10)

### 체력바 스프라이트 교체 (`Unit.prefab`)
- Background: `SmallBar_Base_0` (Sliced, 흰색)
- Fill: `SmallBar_Fill` (흰색, scale y=7, SizeDelta.x=-20)
- HealthBarRoot: scale 0.016, SizeDelta (94,19), anchoredPosition.y=0.7
- HealthBar.cs: offsetMax 리셋 코드 제거

---

## 현재 알려진 미완성 사항
- 음량 슬라이더 PlayerPrefs 저장 미구현
- Stage01 외 추가 스테이지 없음
- 테스트 스크립트 없음
