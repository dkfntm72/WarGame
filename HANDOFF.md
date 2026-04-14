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

## 다음 할 일

| 항목 | 내용 |
|---|---|
| Stage01MapData 내용 입력 | `stageTitle`, `stageOverview` 값 채우기 |
| StageSelect 씬 UI 스프라이트 | 미적용 상태 |
| `unitStatusText` | GameUI 필드 미연결 |
| Stage01 외 추가 스테이지 | 없음 |
| 테스트 스크립트 | 없음 |

---

## 미해결 이슈

| 이슈 | 상태 |
|---|---|
| `unitStatusText` GameUI 미연결 | 미해결 |
| StageSelect 씬 UI 스프라이트 미적용 | 미해결 |
| Stage01 외 추가 스테이지 없음 | 미해결 |

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
