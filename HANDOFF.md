# WarGame Handoff — 2026-05-05 (2차)

## 이번 세션 작업 내용

### 1. 이벤트 트리거 — OnBuildingCapture 조건 추가

특정 건물을 점령했을 때 이벤트가 발동되는 트리거 조건 추가.

**변경 파일:**

| 파일 | 변경 내용 |
|---|---|
| `Assets/Scripts/Data/Enums.cs` | `EventConditionType.OnBuildingCapture` 추가 |
| `Assets/Scripts/Data/MapData.cs` | `EventTrigger`에 `Faction captureByFaction` 필드 추가 |
| `Assets/Scripts/Core/EventTriggerManager.cs` | `OnBuildingCaptured(Building, Faction)` 메서드 추가 |
| `Assets/Scripts/Core/GameManager.cs` | `CaptureBuilding()` 내 `OnBuildingCaptured()` 호출 추가 |
| `Assets/Scripts/Editor/MapEditorWindow.cs` | 에디터 UI, 그리드 파란색 시각화(`C{id}`) 추가 |

**동작 방식:**
- 건물 위치: `EventTrigger.x1, y1` 재사용
- `captureByFaction`: 어떤 팩션이 점령할 때 발동할지 지정
  - `Neutral` = 어떤 팩션이든 상관없음 (센티넬 값)
  - `Player` / `Enemy` = 해당 팩션만
- `fireOnce = true` 시 최초 점령 시 1회만 발동

---

### 2. 유닛 생산 버그 수정

**문제 1 — 다중 성 시 항상 첫 번째 성 기준으로 스폰**

`OnProduceUnit(UnitType type)` → `OnProduceUnit(UnitType type, Building castle)`로 변경.
`GameUI.ShowBuildingPanel`에서 클릭한 건물을 클로저로 전달.

**문제 2 — 스폰 타일 막힘 시 조용히 실패 (피드백 없음)**

성 인접 4칸 탐색(`FindEmptyAdjacent`) 방식 → 성 타일 자체에 스폰으로 변경.
성 타일에 유닛이 있으면 골드 환불 + `unitStatusText`에 "성 위에 유닛이 있어 생산할 수 없습니다." 표시.

**변경 파일:**

| 파일 | 변경 내용 |
|---|---|
| `Assets/Scripts/Input/PlayerInputHandler.cs` | `OnProduceUnit` 시그니처 변경, `FindEmptyAdjacent` 제거, 성 타일 직접 스폰 |
| `Assets/Scripts/UI/GameUI.cs` | `ShowNotice(string)` 메서드 추가, `OnProduceUnit` 호출 시 building 전달 |

---

## 미완성 / 주의사항

| 항목 | 상태 |
|---|---|
| `GameUI.conditionsOfVictoryText` Stage01 Inspector 미연결 | **연결 필요** |
| `GameUI.unitStatusText` Stage01 Inspector 미연결 | **연결 필요** (ShowNotice 표시에 필요) |
| Stage02 MapData → 씬 생성됨 (Stage Creator로 재생성 권장) | 확인 필요 |
| 테스트 스크립트 없음 | 미해결 |

---

---

# WarGame Handoff — 2026-05-05

## 이번 세션 작업 내용

### 1. 불필요 에디터 스크립트 대량 삭제

개발 중 1회성으로 생성된 에디터 스크립트 97개(cs + meta) 삭제.

**남긴 파일 (4개):**

| 파일 | 위치 | 용도 |
|---|---|---|
| `MapEditorWindow.cs` | Scripts/Editor | 맵 에디터 창 (유일한 EditorWindow) |
| `WarGameStageSetup.cs` | Scripts/Editor | Setup Stage01 — 씬 전체 재생성 |
| `BuildGameUI.cs` | Scripts/Editor | WarGameStageSetup 내부 호출용 UI 빌더 |
| `ClearSaveData.cs` | Assets/Editor | PlayerPrefs 초기화 |

**삭제 후 발생한 컴파일 오류 수정:**
- `BuildGameUI.cs` 174번 줄이 삭제된 `CreateSettingsPanelPrefab.Execute()` 호출 → `SettingsPanel.prefab`이 없을 때 경고 로그 후 return으로 교체

---

### 2. StageSelectUI.cs — SceneNames 하드코딩 제거

**파일:** `Assets/Scripts/UI/StageSelectUI.cs`

- `private static readonly string[] SceneNames = { "Stage01", null, null, null, null }` 삭제
- `SceneNameAt(int i)` 메서드로 대체
  - `stageDatas[i] != null` 이면 `$"Stage{(i+1):D2}"` 반환, 없으면 null
- 효과: `stageDatas[]` 배열에 MapData만 추가하면 버튼 자동 활성화. 코드 수정 불필요.

---

### 3. StageCreatorWindow.cs 신규 생성

**파일:** `Assets/Scripts/Editor/StageCreatorWindow.cs`  
**메뉴:** `Window > WarGame > Stage Creator`

**기능:**
1. 입력 필드: 스테이지 번호, 제목, 맵 크기(너비×높이), 플레이어/적 초기 골드
2. **MapData 자동 생성** → `Assets/GameData/Maps/StageXX.asset`
3. **씬 자동 생성** → `Stage01.unity`를 복사 후 `GameManager.currentMap`만 교체
   - 코드로 UI를 재생성하지 않으므로 Stage01과 완전히 동일한 UI 보장
4. **StageSelect 자동 연결** → `StageSelect.unity`를 애디티브로 열어 `StageSelectUI.stageDatas[N]` 연결 후 저장
5. **Build Settings 자동 등록**

**주의:**
- `EditorApplication.delayCall` 사용 — OnGUI 중 씬 전환 시 발생하는 `EndLayoutGroup` 오류 방지
- StageSelect에 버튼이 부족하면 팝업 경고 표시 후 수동 연결 안내

---

### 4. BuildGameUI.cs — GameUI 미연결 필드 2개 보완

**파일:** `Assets/Scripts/Editor/BuildGameUI.cs`

| 필드 | 연결 방법 |
|---|---|
| `gameUI.dialogueSpeakerText` | EventPanel 내 SpeakerBox/SpeakerName TMP |
| `gameUI.conditionsOfVictoryText` | SettingsPanel 프리팹 내 `ConditionsOfVictoryText/COV` TMP 탐색 |

**EventPanel 구조 변경** (전체화면 오버레이 → 하단 고정 바):

| 항목 | 이전 | 이후 |
|---|---|---|
| 앵커 | 전체 화면 stretch | 하단 3%~97%, height 185px |
| 자식 구조 | EventTextBox 단일 | BorderTop + SpeakerBox + EventText + HintText |
| Canvas override | 없음 | sortingOrder 999 |

**CanvasScaler 해상도 수정:**
- 오류: `1080 × 1920` (세로 모바일)
- 수정: `1920 × 1080` (Stage01 기준)

---

## 미완성 / 주의사항

| 항목 | 상태 |
|---|---|
| `GameUI.conditionsOfVictoryText` Stage01 Inspector 미연결 | **연결 필요** (Stage01은 BuildGameUI로 재생성 전까지 수동 연결) |
| `GameUI.unitStatusText` Stage01 Inspector 미연결 | **연결 필요** |
| Stage02 MapData → 씬 생성됨 (Stage Creator로 재생성 권장) | 확인 필요 |
| 테스트 스크립트 없음 | 미해결 |

> **Stage01 미연결 필드 해결 방법:**  
> `Window > WarGame > Setup Stage01` 재실행 시 전체 재생성되어 자동 연결됨 (단, 수동으로 편집한 타일/유닛/건물 배치가 초기화됨).  
> 또는 Stage01 씬 Inspector에서 직접 연결:
> - `GameUI.conditionsOfVictoryText` → `UI/SettingsPanel/ConditionsOfVictoryText/COV`
> - `GameUI.unitStatusText` → `UI/UnitInfoPanel/UnitStatus`

---

---

# WarGame Handoff — 2026-05-01

## 이번 세션 작업 내용

### 1. MapData 필드 정리

**파일:** `Assets/Scripts/Data/MapData.cs`

| 변경 | 내용 |
|---|---|
| `scenarioName` 삭제 | 미사용 필드 제거 |
| `scenarioStory` → `winLossDescription` | 용도 변경: 승패 조건 설명 텍스트, 헤더 "승패 조건" |

**함께 수정된 참조 파일:**

| 파일 | 변경 내용 |
|---|---|
| `Scripts/Core/GameManager.cs` | Debug.Log `scenarioName` → `stageTitle` |
| `Scripts/Editor/MapEditorWindow.cs` | `scenarioName` → `stageTitle` (2곳) |
| `Scripts/Editor/WarGameStageSetup.cs` | `scenarioName` 삭제, `scenarioStory` → `winLossDescription` |
| `Scripts/Editor/WarGameStageCreator.cs` | `scenarioName` 삭제, `scenarioStory` → `winLossDescription` |
| `Assets/Editor/ReapplyGrass2BgLayer.cs` | Debug.Log `scenarioName` → `stageTitle` |

---

### 2. 승패조건 텍스트 → 설정창 UI 연동

**파일:** `Assets/Scripts/UI/GameUI.cs`

- `[Header("Settings")]` 아래 `conditionsOfVictoryText` (TextMeshProUGUI) 필드 추가
- `Start()`에서 `GameManager.Instance.currentMap.winLossDescription` 자동 적용

**Unity 에디터에서 해야 할 작업:**
- Stage01 씬 `GameUI` Inspector → `Conditions Of Victory Text` 슬롯에 설정창 자식 `ConditionsOfVictoryText` 오브젝트 연결 필요

---

### 3. 스테이지 생성기 시도 → 전량 롤백

`StageCreatorWindow.cs` 생성 및 `BuildGameUI.cs` 수정(UI 크기/위치 전면 변경 포함)을 시도했으나 사용자 요청으로 전체 롤백.

- `StageCreatorWindow.cs` 삭제
- `BuildGameUI.cs` 마지막 커밋 상태로 복원
- `Stage02.unity` 삭제 (생성기 테스트 중 생성된 파일)

---

### 4. Stage02.asset 복원

롤백 과정에서 Stage02.asset을 실수로 삭제 → 빈 MapData로 재생성.

- 경로: `Assets/GameData/Maps/Stage02.asset`
- 내용: 기본값 (title="새 스테이지", 10×8, 골드100/80, 지형/유닛/건물/이벤트 비어있음)
- **주의:** GUID가 원본과 달라졌음. 기존에 참조된 곳이 있다면 재연결 필요 (현재 씬/SceneNames 미연결 상태였으므로 실질적 영향 없음)

---

## 미완성 / 주의사항

| 항목 | 상태 |
|---|---|
| `GameUI.conditionsOfVictoryText` Inspector 미연결 | **연결 필요** |
| `unitStatusText` GameUI 미연결 | 미해결 |
| Stage02 MapData 생성됨 → 씬/SceneNames 연결 미완 | 미해결 |
| 테스트 스크립트 없음 | 미해결 |

---

---

# WarGame Handoff — 2026-04-30

## 이번 세션 작업 내용

### 1. 중복 씬 파일 삭제

잘못 생성된 루트 위치 씬 파일 2개 삭제.

| 삭제 파일 | 올바른 파일 |
|---|---|
| `Assets/Stage01.unity` (+.meta) | `Assets/Scenes/Stage01.unity` |
| `Assets/StageSelect.unity` (+.meta) | `Assets/Scenes/StageSelect.unity` |

에디터 스크립트 실행 중 경로 오지정으로 `Assets/` 루트에 중복 생성되었던 파일. Build Settings에 미등록 상태였으며 실제 사용되지 않던 파일.

---

### 2. 근접 유닛 층 간 공격 차단

**문제:** 근접 유닛(attackRange=1)이 Grass2(2층 잔디) ↔ Grass(1층 잔디) 간 공격 가능했던 버그.

**원인:** `GetAttackRange()`, `InAttackRange()`, `CanCounter()` 등 모든 공격 판정이 맨해튼 거리만 체크하고 타일 높이를 무시.

**수정 파일:**

| 파일 | 변경 내용 |
|---|---|
| `Scripts/Grid/GridManager.cs` | `CanAttackBetween(from, to, attackRange)` 헬퍼 추가 |
| `Scripts/Units/Unit.cs` | `CanCounter()`, `HasAttackableTarget()` 에 높이 체크 적용 |
| `Scripts/Input/PlayerInputHandler.cs` | `RefreshHighlights()` attackTiles 필터에 높이 체크 적용 |
| `Scripts/AI/EnemyAI.cs` | `InAttackRange()` 에 높이 체크 적용 |
| `Scripts/AI/AllyAI.cs` | `InAttackRange()` 에 높이 체크 적용 |

**규칙:**
- `attackRange == 1` (근접): Grass ↔ Grass2 공격 불가, Slope 타일은 양쪽 허용
- `attackRange > 1` (원거리/궁수): 높이 무관 모두 공격 가능
- 반격(`CanCounter`)도 동일 규칙 적용

---

## 미완성 / 주의사항

| 항목 | 상태 |
|---|---|
| `unitStatusText` GameUI 미연결 | 미해결 |
| Stage02 MapData 생성됨 → 씬/SceneNames 연결 미완 | 미해결 |
| 테스트 스크립트 없음 | 미해결 |

---

---

# WarGame Handoff — 2026-04-16

## 이번 세션 작업 내용

### 1. StageSelect 씬 — Inspector 연결 전면 수정 (씬 파일 직접 수정)

**파일:** `Assets/Scenes/StageSelect.unity`

기존에 에디터 스크립트로 연결을 시도했으나 씬 파일에 실제로 반영되지 않았던 필드들을 직접 수정.

| 필드 | 이전 | 수정 후 |
|---|---|---|
| `selectionCorners[0]` | 미연결 | Cursor_04_0 (좌상) |
| `selectionCorners[1]` | 미연결 | Cursor_04_1 (우상) |
| `selectionCorners[2]` | 미연결 | Cursor_04_2 (좌하) |
| `selectionCorners[3]` | 미연결 | Cursor_04_3 (우하) |
| `stageTitleText` | `{fileID: 0}` | `stagename` TMP (fileID 1381009793) |
| `stageOverviewText` | `{fileID: 0}` | `content` TMP (fileID 908503745) |
| `stageDatas[0]` | `[]` (비어있음) | `Stage01.asset` |
| `startButton` | `{fileID: 0}` | `StartButton` Button (fileID 437202648) |

**Cursor_04 스프라이트 슬라이스 구조 (128×128, 64×64 4분할):**
- `Cursor_04_0` fileID: 6574313578489692094 — 좌상
- `Cursor_04_1` fileID: 7132572721160946631 — 우상
- `Cursor_04_2` fileID: -5929678396764019378 — 좌하
- `Cursor_04_3` fileID: 9109851116447529100 — 우하
- 공통 guid: `148e1ab80b2b14ce5858df57aeae24e0`

**확인 사항:** `Stage01.asset`의 `stageTitle`("약자의 첫 승리"), `stageOverview`는 이미 입력되어 있었음.

---

## 미완성 / 주의사항

| 항목 | 상태 |
|---|---|
| `unitStatusText` GameUI 미연결 | 미해결 |
| `Assets/Stage01.unity` (루트 잘못 생성된 파일) | **삭제 필요** |
| Stage02 MapData 생성됨 → 씬/SceneNames 연결 미완 | 미해결 |
| 테스트 스크립트 없음 | 미해결 |

---

---

# WarGame Handoff — 2026-04-14

## 이번 세션 작업 내용

### 1. MapData에 스테이지 정보 필드 추가

**파일:** `Assets/Scripts/Data/MapData.cs`

- `stageTitle` (string) — 스테이지 제목
- `stageOverview` (TextArea 2~5줄) — 스테이지 개요
- Inspector 헤더 구분: `스테이지 정보` / `시나리오` / `초기 자원`

---

### 2. StageSelect 씬 — 스테이지 선택 UX 전면 개편

**파일:** `Assets/Scripts/UI/StageSelectUI.cs`

**변경 사항:**
- 스테이지 버튼 클릭 → 바로 씬 로드 ❌ → **선택 상태 유지** 후 시작 버튼으로 진입
- 스테이지 버튼 호버/선택 색 변화 없음 (`highlightedColor = normalColor`)
- 선택 인디케이터: `Cursor_04.png` 슬라이스 4개를 각 코너에 배치 (50×50px, 회전 없이 순서대로 좌상/우상/좌하/우하)
- 씬 진입 시 아무 스테이지도 선택되지 않은 상태로 시작
- `Stage Description` 패널: 스테이지 선택 시 MapData의 `stageTitle` → stagename, `stageOverview` → content 표시

**새 Inspector 필드:**
- `selectionCorners` (Sprite[4]) — 코너 브라켓 스프라이트 배열
- `stageDatas` (MapData[]) — 스테이지별 MapData 연결
- `stageTitleText` / `stageOverviewText` — Stage Description TMP 연결
- `startButton` — 시작 버튼 연결

---

### 3. StartButton 추가 (StageSelect 씬)

**오브젝트:** `Canvas/StartButton`

- 위치: 우측 하단, BackButton과 대칭 (anchor 우하단, 220×56px)
- 스프라이트: `TinySquareRedButton`
- 폰트: 경기천년제목OTF_Bold, "시작" 텍스트
- **스테이지 미선택 시:** 어두운 회색 + interactable=false
- **스테이지 선택 시:** 밝은 흰색 + interactable=true
- 클릭 → 선택된 스테이지 씬 로드

---

### 4. Inspector 연결 완료 (스크립트로 처리)

| 필드 | 연결값 |
|---|---|
| `startButton` | `Canvas/StartButton` |
| `stageTitleText` | `Canvas/Stage Description/stagename` |
| `stageOverviewText` | `Canvas/Stage Description/content` |
| `stageDatas[0]` | `Assets/GameData/Maps/Stage01.asset` |
| `selectionCorners[0~3]` | `Cursor_04_0 ~ Cursor_04_3` |

---

### 5. 신규 에디터 스크립트 (1회성)

| 파일 | 용도 |
|---|---|
| `FixStartButton.cs` | StartButton TMP 교체 및 색상 설정 |
| `SetSelectionCorners.cs` | selectionCorners 스프라이트 배열 연결 |
| `ConnectStageData.cs` | stageDatas[0] → Stage01.asset 연결 |

---

---

---

# WarGame Handoff — 2026-04-15

## 이번 세션 작업 내용

### 1. 이벤트 트리거 — OnStageStart 조건 추가

**변경 파일:**
- `Assets/Scripts/Data/Enums.cs` — `EventConditionType.OnStageStart` 추가
- `Assets/Scripts/Core/EventTriggerManager.cs` — `OnStageStart()` 메서드 추가
- `Assets/Scripts/Core/GameManager.cs` — `Initialize()` 직후, `StartGame()` 직전에 `OnStageStart()` 호출
- `Assets/Scripts/Editor/MapEditorWindow.cs` — 조건 레이블 + HelpBox 추가

---

### 2. 대화창 시스템 전면 개편

**목적:** 이벤트 ShowText를 하단 고정 RPG 대화창 스타일 + 연속 출력으로 교체

**`Assets/Scripts/Data/MapData.cs` — `TriggerAction` 필드 추가:**
- `string speakerName` — 화자 이름 (비워두면 숨김)
- `string[] dialogueLines` — 연속 대사 배열 (클릭마다 한 줄씩 진행)
- 기존 `string text` 유지 (레거시 폴백)

**`Assets/Scripts/UI/GameUI.cs`:**
- `dialogueSpeakerText` 필드 추가
- `ShowDialogue(lines, speakerName, onConfirm)` 추가
- `AdvanceDialogue()` — 클릭 시 다음 줄, 마지막 줄에서 닫힘
- `ShowEventText()` — 레거시 래퍼로 유지

**`Assets/Scripts/Core/EventTriggerManager.cs`:**
- `ShowText` 액션: `dialogueLines` 있으면 사용, 없으면 `text` 폴백
- `GameUI.Instance.ShowDialogue()` 호출로 변경

**`Assets/Scripts/Editor/BuildGameUI.cs`:**
- 전체화면 오버레이 → **하단 고정 대화창** (높이 185px, 너비 94%)
- 구성: 황금색 상단 테두리 / SpeakerBox 탭 / 본문 TMP / "▼ 클릭하여 계속" 우하단

**`Assets/Scripts/Editor/MapEditorWindow.cs`:**
- ShowText 편집 UI → 화자 필드 + 대사 목록 (줄 추가·삭제) 편집기로 교체

**씬 직접 적용 (Stage01.unity):**
- `RebuildDialogueBox.cs`로 기존 EventPanel 구조 수정 후 스크립트 삭제
- `UI/EventPanel` → 하단 바, `SpeakerBox`, `BorderTop` 추가, `HintText` → "▼ 클릭하여 계속"
- `GameUI.dialogueSpeakerText` 연결 완료

---

## 미완성 / 주의사항

| 항목 | 상태 |
|---|---|
| `Stage01.asset` stageTitle / stageOverview 미입력 | 미해결 |
| `unitStatusText` GameUI 미연결 | 미해결 |
| StageSelect 씬 UI 스프라이트 미적용 | 미해결 |
| Stage02 MapData 생성됨 → 씬/SceneNames 연결 미완 | 미해결 |
| `Assets/Stage01.unity` (루트) 잘못 생성된 파일 | **삭제 필요** |
| 테스트 스크립트 | 없음 |

---

---

## 이전 세션 작업 내용 (2026-04-14 오전)

### 1. 에디터 도구 통합 (`WarGameStageSetup.cs` 신규)

- `Window > WarGame > Setup Stage01` 하나로 전체 셋업 자동화
- UnitData 6종, BuildingData 4종, GameSettings, Stage01 MapData 생성
- 씬 신규 생성 + 카메라/Grid/Tilemap 셋업 + 매니저 전체 배치
- `BuildGameUI.Execute()` 호출로 Canvas UI 생성까지 자동화
- 기존 `WarGameStageCreator.cs`, `WarGameSceneSetup.cs` — `[MenuItem]` 주석 처리

### 2. SettingsPanel 프리팹화

- `Assets/Prefabs/SettingsPanel.prefab` 생성
- 구성: 설정 타이틀 / 음량 라벨 / 볼륨 슬라이더 / 스테이지 선택 버튼(빨간색)

### 3. 설정창 기능 완성

- 볼륨 슬라이더 → `PlayerPrefs("Volume")` 저장/불러오기
- `Start()`에서 ExitButton → `ExitToStageSelect()` 런타임 연결
- `Start()`에서 CloseButton → `ToggleSettings()` 런타임 연결

### 4. UI 스프라이트 용도별 적용

| 요소 | 적용 스프라이트 |
|---|---|
| HUD 배경 | `SpecialPaper_0` |
| 턴 종료 버튼 | `SmallBlueSquareButton_Regular_0` |
| 설정창 나가기 버튼 | `SmallRedSquareButton_Regular` |
| 설정 버튼 | `Icon_10` (톱니바퀴) |
| 닫기 버튼 | `Icon_09` (빨간 X) |

---

## 현재 알려진 미완성 사항
- Stage01MapData `stageTitle` / `stageOverview` 값 미입력
- Stage01 외 추가 스테이지 없음
- 테스트 스크립트 없음
- StageSelect 씬 UI 스프라이트 미적용
- `unitStatusText` GameUI 미연결
