# WarGame Handoff — 2026-04-09

## 이번 세션 작업 내용

### 1. HealthBar 자동 숨김 (`HealthBar.cs`)
- 데미지를 받으면 체력바가 나타나고 **2초 후 자동으로 사라지는** 코루틴 추가
- 그 사이 추가 데미지를 받으면 타이머 리셋
- `SetSortingOrder(int order)` 메서드 추가 → Unit LateUpdate에서 동기화

### 2. Tower 체력바 크기 수정 (`Unit.cs`)
- 기존: Tower 스케일 0.5x를 상쇄하려 `hbRoot.localScale = (2, 2, 1)` 적용 → 과도하게 크게 보임
- 수정: 보정 코드 제거. Tower 스케일에 비례해 자연스럽게 0.5x로 표시

### 3. Y-소팅 (스프라이트 겹침 해결) (`Unit.cs`)
- `LateUpdate()`에서 `sortingOrder = 100 - (int)position.y` 로 매 프레임 갱신
- Y가 낮을수록(화면 아래) 앞에 렌더링 → 유닛끼리 자연스럽게 겹침
- Tower의 ArcherOverlay (`_overlaySr`)도 `order + 1`로 동기화
- HealthBar Canvas → `order + 2`, RankBar Canvas → `order + 2`로 항상 유닛 위에 표시

### 4. DamageNumber 렌더링 버그 수정 (`Unit.cs`)
- **원인**: Y-소팅으로 유닛 sortingOrder가 대폭 상승(10000+)하면서 DamageNumber(order 10)가 유닛 뒤에 숨음
- **수정**: sortingOrder 기준값을 `100`으로 낮추고, DamageNumber 생성 시 `sr.sortingOrder + 3`으로 설정

### 5. RankBar 레이아웃 변경 (`RankBar.cs`)
- 기존: 가로 막대 5개를 유닛 하단에 가로 배치
- 변경: **가로로 긴 직사각형 바(28x9) 5개를 유닛 오른쪽에 세로로 쌓기**
- 위치: `localPosition = (0.35f, 0.2f, 0f)`
- 크기: `localScale = 0.007f` (기본 0.01의 0.7배)
- sortingOrder: LateUpdate에서 유닛과 동기화 (`SetSortingOrder` 메서드)

---

## 변경된 파일 목록

| 파일 | 변경 내용 |
|---|---|
| `Assets/Scripts/Units/HealthBar.cs` | 자동 숨김 코루틴, SetSortingOrder 추가 |
| `Assets/Scripts/Units/Unit.cs` | Tower 체력바 보정 제거, _overlaySr 추가, LateUpdate Y-소팅, DamageNumber sortingOrder 수정 |
| `Assets/Scripts/Units/RankBar.cs` | 가로 바 세로 쌓기 레이아웃, SetSortingOrder 추가 |

---

## 현재 알려진 미완성 사항
- 음량 슬라이더 PlayerPrefs 저장 미구현
- Stage01 외 추가 스테이지 없음
- 테스트 스크립트 없음
