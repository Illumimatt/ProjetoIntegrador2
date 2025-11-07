# Autômato de Estados do Jogo Dekora

## Visão Geral

Este documento descreve o **autômato finito determinístico (DFA)** que controla o fluxo de estados do jogo Dekora. O autômato é implementado na classe `GameManager` e garante transições válidas e previsíveis entre os diferentes estados do jogo.

## Diagrama de Estados

```
┌─────────────────────────────────────────────────────────────────────┐
│                        AUTÔMATO DO DEKORA                            │
└─────────────────────────────────────────────────────────────────────┘

                          [INÍCIO]
                             │
                             ▼
                    ┌────────────────┐
                    │ INITIALIZATION │ (Estado Inicial)
                    └────────────────┘
                             │
                             │ (auto)
                             ▼
                    ┌────────────────┐
              ┌────▶│   MAIN MENU    │◀────┐
              │     └────────────────┘     │
              │              │             │
              │              │ (iniciar)  │ (voltar)
              │              ▼             │
              │     ┌────────────────┐     │
              │     │LEVEL SELECTION │─────┤
              │     └────────────────┘     │
              │              │             │
              │              │ (selecionar)│
              │              ▼             │
              │     ┌────────────────┐     │
              │     │ LOADING LEVEL  │     │
              │     └────────────────┘     │
              │              │             │
              │              │ (carregado)│
              │              ▼             │
              │     ┌────────────────┐     │
         (sair)     │    PLAYING     │─────┘
              │     └────────────────┘
              │         │    │    │
              │         │    │    └─────────┐
              │  (pausar)  │            (revisar)
              │         │    │                 │
              │         ▼    │                 ▼
              │   ┌──────────┐         ┌──────────────┐
              │   │  PAUSED  │         │ LEVEL REVIEW │
              │   └──────────┘         └──────────────┘
              │    │        │               │       │
              │    │   (continuar)    (voltar)     │
              │    │        │               │       │
              │    │        └───────────────┘       │
              │    │                           (completar)
              │    │                                │
              │    │ (config)                       ▼
              │    │                        ┌───────────────┐
              │    ▼                        │LEVEL COMPLETE │
              │ ┌──────────┐                └───────────────┘
              └─┤ SETTINGS │                        │
                └──────────┘                        │
                    │ │                             │
                    │ └────────────(voltar)─────────┤
                    │                               │
                    └───────────────────────────────┘
                                                    │
                                          ┌─────────┘
                                          │
                                          ▼
                                   ┌────────────┐
                                   │  EXITING   │ (Estado Final)
                                   └────────────┘
                                          │
                                          ▼
                                      [FIM]
```

## Estados do Autômato

### 1. INITIALIZATION (Estado Inicial)
- **Descrição:** Estado de inicialização do jogo, carregamento de recursos básicos
- **Transições possíveis:**
  - → `MAIN MENU` (automática após carregar recursos)

### 2. MAIN MENU
- **Descrição:** Menu principal do jogo
- **Transições possíveis:**
  - → `LEVEL SELECTION` (jogador clica em "Jogar")
  - → `SETTINGS` (jogador clica em "Configurações")
  - → `EXITING` (jogador clica em "Sair")

### 3. LEVEL SELECTION
- **Descrição:** Tela de seleção de níveis/fases
- **Transições possíveis:**
  - → `LOADING LEVEL` (jogador seleciona um nível)
  - → `MAIN MENU` (jogador clica em "Voltar")

### 4. LOADING LEVEL
- **Descrição:** Carregando recursos do nível (cena, objetos, etc.)
- **Transições possíveis:**
  - → `PLAYING` (automática após conclusão do carregamento)

### 5. PLAYING (Estado Principal)
- **Descrição:** Jogador está ativamente decorando o ambiente
- **Transições possíveis:**
  - → `PAUSED` (jogador pausa o jogo)
  - → `LEVEL REVIEW` (jogador clica em "Revisar decoração")
  - → `MAIN MENU` (jogador desiste/sai do nível)

### 6. PAUSED
- **Descrição:** Jogo pausado (tempo congelado)
- **Transições possíveis:**
  - → `PLAYING` (jogador clica em "Continuar")
  - → `SETTINGS` (jogador abre configurações)
  - → `MAIN MENU` (jogador abandona o nível)

### 7. LEVEL REVIEW
- **Descrição:** Modo de revisão - jogador visualiza a decoração finalizada
- **Transições possíveis:**
  - → `PLAYING` (jogador volta para continuar decorando)
  - → `LEVEL COMPLETE` (jogador confirma que terminou)

### 8. LEVEL COMPLETE
- **Descrição:** Tela de conclusão do nível com estatísticas
- **Transições possíveis:**
  - → `LEVEL SELECTION` (jogador escolhe próximo nível)
  - → `MAIN MENU` (jogador volta ao menu principal)

### 9. SETTINGS
- **Descrição:** Tela de configurações (áudio, gráficos, controles)
- **Transições possíveis:**
  - → `[ESTADO ANTERIOR]` (jogador fecha configurações)
  - Nota: Este estado lembra de onde veio e retorna para lá

### 10. EXITING (Estado Final)
- **Descrição:** Salvamento final e encerramento do jogo
- **Transições possíveis:**
  - Nenhuma (estado final, encerra a aplicação)

## Matriz de Transições Válidas

| De \ Para | INIT | MENU | SEL | LOAD | PLAY | PAUSE | REV | COMP | SET | EXIT |
|-----------|------|------|-----|------|------|-------|-----|------|-----|------|
| **INITIALIZATION** | ✗ | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ |
| **MAIN MENU** | ✗ | ✗ | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✓ | ✓ |
| **LEVEL SELECTION** | ✗ | ✓ | ✗ | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ |
| **LOADING LEVEL** | ✗ | ✗ | ✗ | ✗ | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ |
| **PLAYING** | ✗ | ✓ | ✗ | ✗ | ✗ | ✓ | ✓ | ✗ | ✗ | ✗ |
| **PAUSED** | ✗ | ✓ | ✗ | ✗ | ✓ | ✗ | ✗ | ✗ | ✓ | ✗ |
| **LEVEL REVIEW** | ✗ | ✗ | ✗ | ✗ | ✓ | ✗ | ✗ | ✓ | ✗ | ✗ |
| **LEVEL COMPLETE** | ✗ | ✓ | ✓ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ |
| **SETTINGS** | ✗ | * | ✗ | ✗ | ✗ | * | ✗ | ✗ | ✗ | ✗ |
| **EXITING** | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ | ✗ |

**Legenda:**
- ✓ = Transição válida
- ✗ = Transição inválida
- \* = Retorna ao estado anterior (Settings guarda o estado de origem)

## Propriedades do Autômato

### 1. Determinístico
- Cada estado tem transições claramente definidas
- Não há ambiguidade: para cada entrada, há exatamente uma transição possível

### 2. Finito
- Número fixo de estados: 10 estados no total
- Conjunto finito de transições entre estados

### 3. Validação de Transições
- O método `IsTransitionValid()` do `GameManager` valida cada tentativa de transição
- Transições inválidas são bloqueadas e registradas no log

### 4. Estado Anterior
- O autômato mantém registro do estado anterior
- Útil para estados como `SETTINGS` que precisam retornar ao estado de origem

### 5. Eventos de Transição
- Cada transição dispara eventos (`OnStateChanged`)
- Outros sistemas podem se inscrever para reagir a mudanças de estado

## Implementação Técnica

### Arquitetura do Código

```
GameState.cs          → Enum com todos os estados
GameManager.cs        → Implementação do autômato
  ├─ TransitionToState()    → Solicita mudança de estado
  ├─ IsTransitionValid()    → Valida se transição é permitida
  ├─ EnterState()           → Lógica ao entrar em estado
  └─ ExitState()            → Lógica ao sair de estado
```

### Exemplo de Uso

```csharp
// Pausar o jogo
GameManager.Instance.PauseGame();

// Iniciar um nível
GameManager.Instance.StartLevel(0);

// Voltar ao menu
GameManager.Instance.ReturnToMainMenu();

// Escutar mudanças de estado
GameManager.Instance.OnStateChanged += (oldState, newState) => {
    Debug.Log($"Estado mudou de {oldState} para {newState}");
};
```

## Fluxo Típico de Jogo

### Fluxo Completo (Jogador completa um nível)

1. `INITIALIZATION` → Jogo inicia
2. `MAIN MENU` → Jogador vê menu principal
3. `LEVEL SELECTION` → Jogador escolhe nível
4. `LOADING LEVEL` → Nível carrega
5. `PLAYING` → Jogador decora o ambiente
6. `LEVEL REVIEW` → Jogador revisa sua decoração
7. `LEVEL COMPLETE` → Nível completado, mostra estatísticas
8. `LEVEL SELECTION` → Jogador escolhe próximo nível
9. ... (repete 3-8)

### Fluxo com Pausa

1. `PLAYING` → Jogador está decorando
2. `PAUSED` → Jogador pausa (aperta ESC)
3. `SETTINGS` → Jogador ajusta volume
4. `PAUSED` → Volta para pausa
5. `PLAYING` → Continua jogando

### Fluxo de Saída

1. Qualquer estado...
2. `MAIN MENU` → Jogador volta ao menu
3. `EXITING` → Jogador clica em "Sair"
4. Aplicação encerra

## Benefícios do Autômato

### 1. Previsibilidade
- O comportamento do jogo é sempre previsível
- Não há estados "indefinidos" ou "inválidos"

### 2. Manutenibilidade
- Fácil adicionar novos estados
- Fácil modificar transições
- Código centralizado no `GameManager`

### 3. Debugabilidade
- Estado atual sempre visível no Inspector
- Logs de todas as transições
- Fácil rastrear fluxo do jogo

### 4. Extensibilidade
- Novos estados podem ser adicionados sem quebrar o código existente
- Sistema de eventos permite que outros sistemas reajam a mudanças

### 5. Robustez
- Validação de transições previne bugs
- Estado do jogo sempre é consistente

## Integração com Outros Sistemas

### GameManager ↔ LevelManager
```
LEVEL SELECTION → LevelManager carrega lista de níveis
LOADING LEVEL   → LevelManager.LoadLevel(index)
LEVEL COMPLETE  → LevelManager.UnloadCurrentLevel()
```

### GameManager ↔ SaveManager
```
LEVEL COMPLETE  → SaveManager.CompleteLevel(index)
INITIALIZATION  → SaveManager.LoadGame()
EXITING        → SaveManager.SaveGame()
```

### GameManager ↔ AudioManager
```
MAIN MENU      → AudioManager.PlayMusic("menu_theme")
PLAYING        → AudioManager.PlayMusic("gameplay_theme")
PAUSED         → AudioManager.PauseMusic()
```

### GameManager ↔ UIManager
```
Cada estado    → UIManager mostra/esconde telas apropriadas
MAIN MENU      → Mostra MainMenuScreen
PLAYING        → Mostra GameplayHUD
PAUSED         → Mostra PauseScreen
```

## Testes Recomendados

### Testes de Transição
1. ✓ Verificar todas as transições válidas funcionam
2. ✓ Verificar transições inválidas são bloqueadas
3. ✓ Verificar que Settings retorna ao estado correto

### Testes de Estado
1. ✓ Verificar EnterState() executa lógica correta
2. ✓ Verificar ExitState() faz cleanup apropriado
3. ✓ Verificar Time.timeScale correto em cada estado

### Testes de Integração
1. ✓ Fluxo completo: menu → nível → conclusão
2. ✓ Fluxo com pausa e configurações
3. ✓ Fluxo de saída do jogo

## Possíveis Extensões Futuras

### Novos Estados Potenciais
- `CUTSCENE` - Para animações narrativas
- `TUTORIAL` - Estado especial para tutorial
- `MULTIPLAYER_LOBBY` - Se adicionar multiplayer
- `SHOP` - Loja para comprar decorações
- `GALLERY` - Galeria de decorações salvas

### Melhorias no Autômato
- Sub-estados (estados aninhados)
- Histórico de estados (pilha de estados)
- Transições condicionais (com predicados)
- Transições temporais (auto-transição após X segundos)

## Referências

- **Padrão de Projeto:** State Pattern (GoF)
- **Arquitetura:** Finite State Machine (FSM)
- **Implementação:** Singleton Pattern + Events

---

**Documento criado em:** 07/11/2025  
**Autor:** Sistema de IA para Projeto Integrador II  
**Versão:** 1.0

