# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**WarGame** is a Unity 6 game project. The Unity project lives in `Project_WarGame/`.

- **Unity version:** 6000.0.58f2 (Unity 6)
- **Render pipeline:** Universal Render Pipeline (URP), configured for 2D
- **Input system:** Unity's New Input System

## Development Setup

Open `Project_WarGame/Project_WarGame.sln` in Visual Studio (requires the "Game development with Unity" workload) or JetBrains Rider. The Unity Editor must be installed at version **6000.0.58f2**.

There is no CLI build command — building is done through the Unity Editor (`File > Build Settings`). The Coplay package (`com.coplaydev.coplay`) provides AI-assisted development tooling inside the Unity Editor.

## Project Structure

```
Project_WarGame/
├── Assets/
│   ├── Scenes/SampleScene.unity    # Default scene (only scene in build)
│   ├── Settings/                   # URP renderer and scene template assets
│   ├── InputSystem_Actions.inputactions  # Player input bindings
│   └── DefaultVolumeProfile.asset  # URP post-processing volume
├── Packages/
│   ├── manifest.json               # Direct package dependencies
│   └── Coplay/                     # Local copy of Coplay AI plugin (git dep)
└── ProjectSettings/                # Unity project-wide settings
```

## Key Packages

| Package | Purpose |
|---|---|
| `com.unity.feature.2d` | 2D feature set (physics, sprites, tilemaps) |
| `com.unity.render-pipelines.universal` | URP graphics |
| `com.unity.inputsystem` | New Input System |
| `com.unity.test-framework` | Unity Test Runner |
| `com.unity.timeline` | Timeline / cutscene system |
| `com.coplaydev.coplay` | AI dev tooling (Claude integration in-editor) |

## Input Actions

Defined in `Assets/InputSystem_Actions.inputactions` under the **Player** action map:

- **Move** (Vector2) — character movement
- **Look** (Vector2) — camera / aim direction
- **Attack** (Button) — attack
- **Interact** (Button, Hold) — interact / use
- **Crouch** (Button)
- **Jump** (Button)
- **Previous / Next** (Button) — inventory or UI navigation

## Testing

Unity Test Framework is installed. Tests are run from the Unity Editor via **Window > General > Test Runner**. No test scripts exist yet.

## Git Workflow

GitHub Actions workflows for Claude Code Review and Claude PR Assistant are configured (see `.github/workflows/`).

## 행동 규칙

- **스킬(Skill)은 사용자가 명시적으로 요청할 때만 실행한다.** 맥락으로 의도를 추측해서 자의적으로 스킬을 호출하지 않는다.
- **UI 폰트는 항상 `Assets/Fonts/경기천년제목OTF_Bold SDF.asset`만 사용한다.** 새 TMP 텍스트 생성 시 이 폰트를 명시적으로 지정한다.
