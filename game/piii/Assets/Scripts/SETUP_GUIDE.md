# Guia de Configuração Rápida - Dekora

Este guia ajuda você a configurar o autômato do jogo Dekora no Unity em poucos minutos.

## ⚡ Setup Rápido (5 minutos)

### Passo 1: Importar os Scripts

1. Abra o projeto Unity `game/piii`
2. Todos os scripts já estão em `Assets/Scripts/`
3. Aguarde o Unity compilar

### Passo 2: Criar a Cena Base

1. Crie uma nova cena: `File > New Scene`
2. Salve como `_PersistentManagers` (ou outro nome)
3. Delete a Main Camera e Directional Light (não são necessários aqui)

### Passo 3: Adicionar o GameManager

1. No Hierarchy, clique direito e escolha `Create Empty`
2. Renomeie para `GameManager`
3. Com o GameObject selecionado, clique em `Add Component`
4. Digite `GameManager` e adicione o script

### Passo 4: Adicionar os Outros Managers

Repita o processo para cada manager:

1. **LevelManager**
   - Create Empty → Renomear → Adicionar script `LevelManager`

2. **SaveManager**
   - Create Empty → Renomear → Adicionar script `SaveManager`
   - No Inspector, defina o nome do arquivo de save (padrão: `dekora_save.json`)

3. **AudioManager**
   - Create Empty → Renomear → Adicionar script `AudioManager`
   - Ajuste volumes iniciais (Music: 0.7, SFX: 0.8)

Sua hierarquia deve ficar assim:

```
Hierarchy
├── GameManager
├── LevelManager
├── SaveManager
└── AudioManager
```

### Passo 5: Configurar a Cena de Menu

1. Crie outra cena: `Scenes/MainMenu.unity`
2. Adicione UI básica (Canvas, Text, Buttons)
3. Adicione ao Build Settings

### Passo 6: Criar uma Cena de Nível de Teste

1. Crie uma cena: `Scenes/TestLevel.unity`
2. Crie um GameObject vazio chamado `Level`
3. Adicione o script `Level` ao GameObject
4. Configure no Inspector:
   ```
   Level Name: Teste
   Level Index: 0
   ```
5. O GridSystem será adicionado automaticamente

### Passo 7: Adicionar um Objeto Decorável de Teste

1. Crie um Cube no Unity: `GameObject > 3D Object > Cube`
2. Posicione na cena
3. Adicione um Collider se não tiver (Cube já vem com BoxCollider)
4. Adicione o script `DecorativeObject`
5. Configure:
   ```
   Object Name: Cubo Teste
   Category: Decoração
   Can Rotate: ✓
   ```
6. Arraste o Cube para ser filho de `Level` no Hierarchy

### Passo 8: Configurar Build Settings

1. Abra `File > Build Settings`
2. Adicione as cenas nesta ordem:
   - `_PersistentManagers` (índice 0)
   - `MainMenu` (índice 1)
   - `TestLevel` (índice 2)
3. Clique em `Close`

### Passo 9: Testar

1. Abra a cena `_PersistentManagers`
2. Pressione Play
3. Observe o Console:
   ```
   [GameManager] Inicializado com sucesso.
   [GameManager] Transição: Initialization -> MainMenu
   [LevelManager] Inicializado.
   [SaveManager] Inicializado.
   [AudioManager] Inicializado.
   ```

🎉 **Pronto!** O autômato está funcionando!

## 🔧 Configuração Avançada

### Adicionar EventSystem (para UI interativa)

Se você vai usar UI com botões:

1. No Hierarchy, clique direito
2. `UI > Event System`
3. Isso é necessário para detectar cliques

### Configurar Input para Pausa

1. Abra `Edit > Project Settings > Input Manager`
2. Adicione um novo eixo chamado `Pause`
3. Associe à tecla `Escape`

Depois, em algum script de controle:

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.PauseGame();
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            GameManager.Instance.ResumeGame();
        }
    }
}
```

### Adicionar Áudio de Teste

1. Importe alguns AudioClips para o projeto
2. Selecione o `AudioManager` no Hierarchy
3. No Inspector, expanda `Music Library`
4. Aumente o Size e arraste seus clips
5. Dê nomes como "menu_theme", "gameplay_theme"
6. Faça o mesmo para `SFX Library` com sons como "click", "place"

### Script de Teste para Transições

Crie um script simples para testar transições:

```csharp
using UnityEngine;
using Dekora.Core;

public class TestControls : MonoBehaviour
{
    void Update()
    {
        // Teclas de teste
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.Instance.TransitionToState(GameState.MainMenu);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.Instance.TransitionToState(GameState.LevelSelection);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            LevelManager.Instance.LoadLevel("TestLevel");
            GameManager.Instance.TransitionToState(GameState.Playing);
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.Instance.PauseGame();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.ResumeGame();
        }
    }
}
```

Adicione esse script ao GameManager e teste as teclas:
- **1**: Ir para Menu
- **2**: Ir para Seleção de Níveis
- **3**: Carregar TestLevel
- **P**: Pausar
- **R**: Retomar

## 📊 Verificação Visual

### No Inspector do GameManager

Enquanto o jogo está rodando, selecione o GameManager e observe:

```
Game Manager (Script)
├── Current State: Playing      ← Estado atual
└── Previous State: MainMenu    ← Estado anterior
```

Isso muda em tempo real conforme você interage!

### No Console

Você deve ver logs claros:

```
[GameManager] Transição: MainMenu -> LevelSelection
[LevelManager] Iniciando carregamento do nível 'TestLevel'
[GameManager] Transição: LevelSelection -> LoadingLevel
[GameManager] Transição: LoadingLevel -> Playing
[Level] 'Teste' inicializado com 1 objetos decoráveis.
```

## 🐛 Problemas Comuns

### "Type or namespace 'Dekora' could not be found"

**Causa:** Scripts não estão no namespace correto ou não foram compilados.

**Solução:**
1. Verifique que todos os scripts têm `namespace Dekora.Core`, `Dekora.Managers` ou `Dekora.Gameplay`
2. Force recompilação: `Assets > Reimport All`

### "NullReferenceException: Object reference not set"

**Causa:** Tentando acessar um Manager antes dele ser criado.

**Solução:**
1. Use `Instance` apenas em `Start()` ou depois, nunca em `Awake()`
2. Verifique se o GameObject do Manager existe na cena

### Objeto não responde a cliques

**Causa:** Falta Collider ou EventSystem.

**Solução:**
1. Adicione um Collider ao objeto
2. Certifique-se de ter um EventSystem na cena
3. Verifique se o estado é `Playing`

## 🎯 Próximos Passos

Agora que o autômato está configurado:

1. ✅ Crie mais níveis
2. ✅ Adicione mais objetos decoráveis
3. ✅ Implemente UI completa (menus, HUD)
4. ✅ Adicione áudio (música e efeitos)
5. ✅ Implemente sistema de save/load
6. ✅ Adicione efeitos visuais (partículas, animações)
7. ✅ Crie tutoriais dentro do jogo
8. ✅ Teste extensivamente

## 📚 Recursos Adicionais

- **Documentação completa:** `Assets/Scripts/README.md`
- **Diagrama do autômato:** `arquitetura/AUTOMATO_DEKORA.md`
- **Exemplos de uso:** Veja comentários nos scripts

## ✅ Checklist de Configuração

Marque conforme completa:

- [ ] Scripts compilaram sem erros
- [ ] GameManager criado e funcionando
- [ ] LevelManager, SaveManager, AudioManager criados
- [ ] Cena de teste com Level criada
- [ ] Objeto decorável de teste funciona
- [ ] Transições de estado funcionando
- [ ] Console mostra logs claros
- [ ] EventSystem adicionado para UI
- [ ] Build Settings configurado

---

**Tempo estimado:** 5-10 minutos  
**Dificuldade:** Iniciante  
**Última atualização:** 07/11/2025

