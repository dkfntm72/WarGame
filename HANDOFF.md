# WarGame Handoff — 2026-04-13

## 이번 세션 작업 내용

### 1. `settingsPanel` UnassignedReferenceException 수정

**원인:** `BuildGameUI.cs`의 `Execute()` 메서드에 `[MenuItem]` 속성이 없어 에디터 메뉴에서 실행 불가 → 씬의 `GameUI` 컴포넌트에 `settingsPanel` 참조가 깨진 상태로 남아있었음

**수정:** `BuildGameUI.Execute()`에 `[MenuItem("Window/WarGame/Build Stage01 UI")]` 추가

---

### 2. Y-소팅 방식 변경 (sortingOrder → Z값)

**배경:**
- 기존: `LateUpdate()`에서 매 프레임 `sortingOrder = (100 - y) * 4` 계산
- 타워 `ArcherOverlay`(order+1)와 바로 아래 타일 유닛(order+1)이 동일 sortingOrder → 애니메이션 중 깜빡임 발생

**변경 방식:**
- `LateUpdate()` → Z값만 설정 (`transform.position.z = y * 0.01f`)
- Y가 높을수록 Z가 커져 카메라(Z=-10)에서 멀어짐 → 자동으로 뒤에 렌더링
- sortingOrder는 `Initialize()`에서 1회 고정

**sortingOrder 고정값:**

| 레이어 | sortingOrder |
|---|---|
| 유닛 SpriteRenderer | 5 |
| ArcherOverlay SpriteRenderer | 5 |
| HealthBar Canvas | 10 |
| RankBar Canvas | 10 |
| DamageNumber MeshRenderer | 15 |

**ArcherOverlay Z 오프셋:**
- `localPosition = (0, 0.6, -0.01f)` → 타워 본체보다 카메라에 가까워 항상 앞에 렌더링

**변경 파일:** `Assets/Scripts/Units/Unit.cs`

---

### 3. BuildGameUI.Execute() 관련 진단

**확인 내용:**
- `BuildGameUI.Execute()`는 자동 실행 코드 없음 (씬 진입 시 실행 안 됨)
- 실행 시 기존 `UI` 오브젝트 전체 삭제 후 재생성 → Inspector 수동 변경값 초기화
- 이전 세션에서 Coplay MCP가 실행한 것으로 추정 → 유저 수동 변경 UI 롤백됨
- Unity 백업(`Temp/__Backupscenes/0.backup`)은 바이너리 형식, 복구 불가

**역할 정리:**
- 스테이지 UI 초기 골격(Canvas 구조 + GameUI 참조 연결)을 코드로 자동 생성하는 1회성 도구
- Inspector 수동 세부 설정은 반영 안 됨 → 실행하면 초기화됨

---

## 다음 할 일

| 항목 | 내용 |
|---|---|
| `BuildGameUI.cs` MenuItem 주석 처리 | `[MenuItem]` 제거해서 실수로 실행하는 것 방지. 코드는 보존 |
| 스테이지 선택 UI 스프라이트 적용 | StageSelect 씬 UI에 Tiny Swords 스프라이트 적용하는 에디터 스크립트 없음 |
| 음량 슬라이더 PlayerPrefs 저장 | AudioListener.volume 연동만 됨, 재시작 시 초기화 |

---

## 미해결 이슈 (이전 세션 이월)

| 이슈 | 상태 |
|---|---|
| 음량 슬라이더 PlayerPrefs 저장 미구현 | 미해결 |
| Stage01 외 추가 스테이지 없음 | 미해결 |
| 테스트 스크립트 없음 | 미해결 |

---

## 이전 세션 작업 내용 (2026-04-12) — 2차

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

## 이전 세션 작업 내용 (2026-04-11)

### Stage01 Canvas UI 신규 생성

- `Assets/Scripts/Editor/BuildGameUI.cs` 에디터 스크립트 신규 작성
- Canvas UI 전체 생성 + GameUI 필드 연결

**생성된 UI 구성:**

| 오브젝트 | 설명 |
|---|---|
| `UI` (Canvas) | ScreenSpaceOverlay, sortingOrder=10, ScaleWithScreenSize 1080×1920 |
| `UI/HUD` | 상단바 — SpecialPaper_0 Sliced 배경 (금장 테두리), 높이 120 |
| `UI/HUD/TurnText` | 좌측, 폰트 36px |
| `UI/HUD/GoldText` | 중앙, 폰트 36px |
| `UI/HUD/SettingsButton` | 우측, Icon_10 톱니바퀴 아이콘, 80×80 |
| `UI/EndTurnButton` | 우하단, SmallBlueSquareButton_Regular_0 Sliced, 160×160 |
| `UI/UnitInfoPanel` | 좌하단, 320×170, 유닛 선택 시 표시 |
| `UI/BuildingPanel` | 중앙 y=-300, 770×280, 성 클릭 시 표시 |
| `UI/VictoryPanel` | 중앙, 600×320, 승리 시 표시 |
| `UI/DefeatPanel` | 중앙, 600×320, 패배 시 표시 |
| `UI/SettingsPanel` | 중앙, 500×300, 볼륨 슬라이더 + 나가기 버튼 |
| `UI/EventPanel` | 전체화면 반투명 오버레이, 이벤트 팝업 |

---

## 이전 세션 작업 내용 (2026-04-10)

### 체력바 스프라이트 교체 (`Unit.prefab`)
- Background: `SmallBar_Base_0` (Sliced, 흰색)
- Fill: `SmallBar_Fill` (흰색, scale y=7, SizeDelta.x=-20)
- HealthBarRoot: scale 0.016, SizeDelta (94,19), anchoredPosition.y=0.7
- HealthBar.cs: offsetMax 리셋 코드 제거

---

## 현재 알려진 미완성 사항
- `BuildGameUI.cs` `[MenuItem]` 주석 처리 필요 (다음 할 일)
- 음량 슬라이더 PlayerPrefs 저장 미구현
- Stage01 외 추가 스테이지 없음
- 테스트 스크립트 없음
- StageSelect 씬 UI 스프라이트 미적용
