# 🎮 Dekora - Referência Rápida do Autômato

## Estados do Jogo

| # | Estado | Descrição | Time Scale |
|---|--------|-----------|------------|
| 1 | `INITIALIZATION` | Carregamento inicial | 1.0 |
| 2 | `MAIN_MENU` | Menu principal | 1.0 |
| 3 | `LEVEL_SELECTION` | Escolha de fase | 1.0 |
| 4 | `LOADING_LEVEL` | Carregando nível | 1.0 |
| 5 | `PLAYING` | ⭐ Gameplay ativo | 1.0 |
| 6 | `PAUSED` | Jogo pausado | 0.0 |
| 7 | `LEVEL_REVIEW` | Revisão da decoração | 1.0 |
| 8 | `LEVEL_COMPLETE` | Nível concluído | 1.0 |
| 9 | `SETTINGS` | Configurações | (mantém) |
| 10 | `EXITING` | Encerrando | N/A |

## API Rápida

### GameManager

```csharp
// Acessar
GameManager.Instance

// Verificar estado
CurrentState  // GameState atual
PreviousState // GameState anterior

// Transições comuns
.PauseGame()
.ResumeGame()
.ReturnToMainMenu()
.StartLevel(int)
.CompleteCurrentLevel()
.OpenSettings()
.CloseSettings()
.QuitGame()

// Eventos
.OnStateChanged += (oldState, newState) => { }
```

### LevelManager

```csharp
// Acessar
LevelManager.Instance

// Carregar níveis
.LoadLevel(int levelIndex)
.LoadLevel(string levelName)
.ReloadCurrentLevel()

// Info
.CurrentLevelIndex    // int
.CurrentLevelName     // string
.IsLevelLoaded        // bool
.GetTotalLevels()     // int

// Eventos
.OnLevelLoadStarted += (int) => { }
.OnLevelLoadCompleted += (int) => { }
.OnLevelLoadProgress += (float) => { }
```

### SaveManager

```csharp
// Acessar
SaveManager.Instance

// Salvar/Carregar
.SaveGame()
.LoadGame()
.ResetSave()
.DeleteSave()

// Progresso
.CompleteLevel(int)
.IsLevelCompleted(int)
.GetHighestLevelReached()

// Dados
.CurrentSaveData  // GameSaveData
```

### AudioManager

```csharp
// Acessar
AudioManager.Instance

// Música
.PlayMusic(string name, fadeIn, duration)
.StopMusic(fadeOut, duration)
.PauseMusic()
.ResumeMusic()

// SFX
.PlaySFX(string name, volumeScale)

// Volume
.MusicVolume  // 0-1
.SfxVolume    // 0-1
```

## Transições Válidas

```
INIT → MENU
MENU → SELECTION, SETTINGS, EXIT
SELECTION → MENU, LOADING
LOADING → PLAYING
PLAYING → MENU, PAUSED, REVIEW
PAUSED → PLAYING, MENU, SETTINGS
REVIEW → PLAYING, COMPLETE
COMPLETE → MENU, SELECTION
SETTINGS → [volta ao anterior]
EXIT → [fim]
```

## Arquivos Principais

```
Scripts/
├── Core/
│   ├── GameState.cs      # Enum de estados
│   └── GameManager.cs    # Autômato principal
├── Managers/
│   ├── LevelManager.cs   # Níveis
│   ├── SaveManager.cs    # Save/Load
│   └── AudioManager.cs   # Áudio
└── Gameplay/
    ├── Level.cs          # Fase
    ├── DecorativeObject.cs # Objeto decorável
    └── GridSystem.cs     # Grid/Snap
```

## Exemplos Rápidos

### Pausar/Despausar

```csharp
void Update() {
    if (Input.GetKeyDown(KeyCode.Escape)) {
        if (GameManager.Instance.CurrentState == GameState.Playing)
            GameManager.Instance.PauseGame();
        else if (GameManager.Instance.CurrentState == GameState.Paused)
            GameManager.Instance.ResumeGame();
    }
}
```

### Carregar Nível

```csharp
public void OnLevelButtonClick(int level) {
    LevelManager.Instance.LoadLevel(level);
    GameManager.Instance.TransitionToState(GameState.LoadingLevel);
}
```

### Reagir a Mudanças de Estado

```csharp
void Start() {
    GameManager.Instance.OnStateChanged += OnStateChange;
}

void OnStateChange(GameState old, GameState new) {
    switch (new) {
        case GameState.Playing:
            // Ativar controles
            break;
        case GameState.Paused:
            // Mostrar UI de pausa
            break;
    }
}
```

### Completar Nível

```csharp
void OnLevelFinished() {
    SaveManager.Instance.CompleteLevel(currentLevel);
    SaveManager.Instance.SaveGame();
    GameManager.Instance.CompleteCurrentLevel();
}
```

### Tocar Áudio

```csharp
// Música de fundo
AudioManager.Instance.PlayMusic("gameplay_theme", fadeIn: true, 2f);

// Efeito sonoro
AudioManager.Instance.PlaySFX("place_object");
```

## Setup Rápido

1. Criar GameObjects vazios: GameManager, LevelManager, SaveManager, AudioManager
2. Adicionar scripts correspondentes
3. Criar cena de nível com GameObject "Level" + script Level.cs
4. Adicionar objetos com DecorativeObject.cs
5. Pressionar Play!

## Documentação Completa

- 📘 **Técnica:** `arquitetura/AUTOMATO_DEKORA.md`
- 📗 **Uso:** `game/piii/Assets/Scripts/README.md`
- 📕 **Setup:** `game/piii/Assets/Scripts/SETUP_GUIDE.md`
- 📙 **Resumo:** `AUTOMATO_IMPLEMENTADO.md`
- 📄 **Diagrama:** `arquitetura/DIAGRAMA_ESTADOS_SIMPLES.txt`

---

**Status:** ✅ Completo | **Versão:** 1.0 | **Data:** 07/11/2025

