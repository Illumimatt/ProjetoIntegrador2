# Scripts do Dekora - Guia de Uso

Este diretório contém todos os scripts C# que implementam a lógica do jogo Dekora.

## Estrutura de Pastas

```
Scripts/
├── Core/               → Classes fundamentais do sistema
│   ├── GameState.cs   → Enum com estados do autômato
│   └── GameManager.cs → Gerenciador central com máquina de estados
│
├── Managers/          → Gerenciadores de sistemas específicos
│   ├── LevelManager.cs    → Carregamento de níveis
│   ├── SaveManager.cs     → Sistema de save/load
│   └── AudioManager.cs    → Gerenciamento de áudio
│
└── Gameplay/          → Lógica de gameplay
    ├── Level.cs           → Representa um nível/fase
    ├── DecorativeObject.cs → Objeto decorável (interativo)
    └── GridSystem.cs      → Sistema de grade para snapping

```

## Como Configurar o Projeto Unity

### 1. Setup Inicial dos Gerenciadores

1. Crie um GameObject vazio na cena inicial (ou use uma cena persistente)
2. Renomeie para `GameManager`
3. Adicione o componente `GameManager.cs`
4. O GameManager criará automaticamente os outros managers (ou você pode criá-los manualmente)

**Hierarquia recomendada:**
```
Hierarchy:
├── GameManager          [GameManager.cs]
├── LevelManager         [LevelManager.cs]
├── SaveManager          [SaveManager.cs]
└── AudioManager         [AudioManager.cs]
```

### 2. Configurar um Nível

1. Crie uma nova cena para seu nível
2. Crie um GameObject vazio e renomeie para `Level`
3. Adicione o componente `Level.cs`
4. Configure no Inspector:
   - Nome do nível
   - Descrição
   - Índice do nível

5. O componente `GridSystem.cs` será adicionado automaticamente

### 3. Adicionar Objetos Decoráveis

1. Importe seu modelo 3D para o Unity
2. Adicione um Collider ao objeto (BoxCollider, MeshCollider, etc.)
3. Adicione o componente `DecorativeObject.cs`
4. Configure no Inspector:
   - Nome do objeto
   - Categoria
   - Permitir rotação

5. Coloque o objeto como filho do GameObject `Level`

### 4. Configurar o AudioManager

1. Selecione o GameObject `AudioManager`
2. No Inspector, configure:
   - Volume da música (Music Volume)
   - Volume dos efeitos (SFX Volume)
3. Adicione AudioClips nas listas:
   - Music Library: arraste suas músicas
   - SFX Library: arraste seus efeitos sonoros
4. Dê nomes únicos para cada clip

## API Rápida

### Controle de Estados

```csharp
// Acessar o GameManager
GameManager.Instance.TransitionToState(GameState.Playing);

// Atalhos úteis
GameManager.Instance.PauseGame();
GameManager.Instance.ResumeGame();
GameManager.Instance.ReturnToMainMenu();
GameManager.Instance.QuitGame();

// Verificar estado atual
if (GameManager.Instance.CurrentState == GameState.Playing)
{
    // Lógica durante gameplay
}

// Escutar mudanças de estado
GameManager.Instance.OnStateChanged += OnGameStateChanged;

void OnGameStateChanged(GameState oldState, GameState newState)
{
    Debug.Log($"Estado mudou: {oldState} -> {newState}");
}
```

### Gerenciamento de Níveis

```csharp
// Carregar um nível
LevelManager.Instance.LoadLevel(0); // Por índice
LevelManager.Instance.LoadLevel("Quarto_1"); // Por nome

// Verificar nível atual
int currentLevel = LevelManager.Instance.CurrentLevelIndex;
bool hasLevel = LevelManager.Instance.IsLevelLoaded;

// Recarregar nível
LevelManager.Instance.ReloadCurrentLevel();

// Escutar eventos
LevelManager.Instance.OnLevelLoadStarted += (index) => { /* ... */ };
LevelManager.Instance.OnLevelLoadCompleted += (index) => { /* ... */ };
LevelManager.Instance.OnLevelLoadProgress += (progress) => { /* ... */ };
```

### Sistema de Save

```csharp
// Salvar progresso
SaveManager.Instance.SaveGame();

// Carregar progresso
SaveManager.Instance.LoadGame();

// Marcar nível como completo
SaveManager.Instance.CompleteLevel(0);

// Verificar se nível foi completado
bool completed = SaveManager.Instance.IsLevelCompleted(0);

// Acessar dados do save
GameSaveData data = SaveManager.Instance.CurrentSaveData;
int highestLevel = SaveManager.Instance.GetHighestLevelReached();

// Resetar/deletar save
SaveManager.Instance.ResetSave();
SaveManager.Instance.DeleteSave();
```

### Gerenciamento de Áudio

```csharp
// Tocar música
AudioManager.Instance.PlayMusic("menu_theme");
AudioManager.Instance.PlayMusic("gameplay_theme", fadeIn: true, fadeDuration: 2f);

// Controlar música
AudioManager.Instance.StopMusic(fadeOut: true, fadeDuration: 1f);
AudioManager.Instance.PauseMusic();
AudioManager.Instance.ResumeMusic();

// Tocar efeitos sonoros
AudioManager.Instance.PlaySFX("click");
AudioManager.Instance.PlaySFX("place_object", volumeScale: 0.8f);

// Ajustar volumes
AudioManager.Instance.MusicVolume = 0.5f; // 0 a 1
AudioManager.Instance.SfxVolume = 0.8f;   // 0 a 1
```

### Trabalhando com Níveis

```csharp
// Obter referência ao nível atual
Level currentLevel = FindObjectOfType<Level>();

// Acessar informações
string name = currentLevel.LevelName;
int index = currentLevel.LevelIndex;
float completion = currentLevel.CompletionPercentage;

// Acessar objetos decoráveis
List<DecorativeObject> objects = currentLevel.DecorativeObjects;

// Acessar grid system
GridSystem grid = currentLevel.GridSystem;
Vector3 snapped = grid.SnapToGrid(transform.position);

// Resetar nível
currentLevel.ResetLevel();
```

### Objetos Decoráveis

```csharp
// Obter referência a um objeto
DecorativeObject obj = GetComponent<DecorativeObject>();

// Verificar estado
bool placed = obj.IsPlaced;
bool dragging = obj.IsDragging;

// Resetar posição
obj.ResetToInitialPosition();

// Rotacionar objeto (90 graus)
obj.RotateObject(90f);
```

## Ordem de Execução Recomendada

1. **Initialization** - GameManager acorda primeiro
2. **Awake** - Todos os managers se registram como Singleton
3. **Start** - GameManager inicia transição para MainMenu
4. **Durante o jogo** - Estados transitam conforme interação do jogador
5. **OnDestroy/Quit** - SaveManager salva progresso final

## Debugging

### Verificar Estado Atual

No Inspector, com o GameManager selecionado, você pode ver:
- Current State (estado atual)
- Previous State (estado anterior)

### Logs Úteis

Todos os managers fazem log de eventos importantes:
```
[GameManager] Transição: MainMenu -> Playing
[LevelManager] Nível 0 carregado: Quarto_1
[SaveManager] Jogo salvo em: C:/Users/.../dekora_save.json
[AudioManager] Tocando música: gameplay_theme
```

### Visualização do Grid

No GameObject com `GridSystem`:
- Marque "Show Grid" para ver a grade na Scene View
- Ajuste "Grid Area" para modificar o tamanho da visualização
- A grade aparece em verde por padrão

### Gizmos dos Objetos Decoráveis

Objetos decoráveis mostram diferentes cores:
- **Branco**: Não colocado
- **Amarelo**: Sendo arrastado
- **Verde**: Colocado com sucesso

## Dicas e Best Practices

### 1. Sempre use os Managers via Instance
```csharp
// ✓ Correto
GameManager.Instance.PauseGame();

// ✗ Errado
GameManager gm = FindObjectOfType<GameManager>();
gm.PauseGame();
```

### 2. Verifique o estado antes de ações
```csharp
if (GameManager.Instance.CurrentState == GameState.Playing)
{
    // Só permite interação durante gameplay
    obj.RotateObject(90f);
}
```

### 3. Use eventos para desacoplar sistemas
```csharp
// ✓ Correto - Usa eventos
GameManager.Instance.OnStateChanged += UpdateUI;

// ✗ Errado - Chama diretamente
// uiManager.UpdateForState(GameState.Playing);
```

### 4. Salve progresso em momentos apropriados
```csharp
// Salvar ao completar nível
SaveManager.Instance.CompleteLevel(levelIndex);
SaveManager.Instance.SaveGame();

// Salvar ao sair
void OnApplicationQuit()
{
    SaveManager.Instance.SaveGame();
}
```

### 5. Gerencie o tempo corretamente
```csharp
// O GameManager já controla Time.timeScale
// Paused: Time.timeScale = 0
// Playing: Time.timeScale = 1

// Use Time.unscaledDeltaTime para UI que precisa animar durante pausa
```

## Arquivos de Configuração Unity

### Build Settings

Adicione suas cenas ao Build Settings na ordem:
1. Cena de inicialização (com GameManager)
2. Menu principal
3. Cena de seleção de níveis
4. Cenas de níveis (Quarto_1, Quarto_2, etc.)

### Project Settings

Certifique-se de configurar:
- **Tags**: Adicione tags relevantes (`Player`, `Interactable`, etc.)
- **Layers**: Configure layers para raycasting
- **Input**: Configure botões de pausa, rotação, etc.

## Solução de Problemas Comuns

### Problema: "Instance is null"
**Solução:** Certifique-se de que o GameManager existe na cena inicial e persiste entre cenas (DontDestroyOnLoad).

### Problema: Transição de estado falha
**Solução:** Verifique os logs. Transições inválidas são bloqueadas. Consulte o diagrama de estados válidas.

### Problema: Objetos não respondem a cliques
**Solução:** 
- Adicione um Collider ao objeto
- Certifique-se de que há um EventSystem na cena
- Verifique se está no estado `Playing`

### Problema: Grid não aparece
**Solução:** 
- Marque "Show Grid" no GridSystem
- A grade só aparece na Scene View, não no Game View
- Verifique se Gizmos estão ativos

### Problema: Áudio não toca
**Solução:**
- Verifique se os AudioClips foram adicionados às listas
- Certifique-se de que os nomes estão corretos (case-sensitive)
- Verifique os níveis de volume

## Próximos Passos

1. Implemente a UI (UIManager, UIScreen, etc.)
2. Crie sistemas de input (mouse, touch, teclado)
3. Adicione mais mecânicas de decoração
4. Implemente sistema de conquistas/achievements
5. Adicione efeitos visuais e polish

## Contato e Suporte

Para dúvidas sobre a implementação, consulte:
- **Documentação completa:** `arquitetura/AUTOMATO_DEKORA.md`
- **Diagrama de classes:** `arquitetura/DiagramaDeClassesDekora.png`
- **Repositório:** https://github.com/Illumimatt/ProjetoIntegrador2

---

**Última atualização:** 07/11/2025  
**Versão dos scripts:** 1.0

