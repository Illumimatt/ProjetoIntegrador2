# Sistema de Camadas 2.5D - Documentação Técnica

## Arquitetura do Sistema

### Visão Geral

O sistema de camadas 2.5D do Dekora implementa profundidade visual em um jogo com câmera ortogonal. Ele separa objetos em diferentes "planos" de profundidade, criando a ilusão de 3D enquanto mantém a simplicidade de um sistema 2D.

```
┌─────────────────────────────────────────────────────────┐
│                    ARQUITETURA 2.5D                     │
├─────────────────────────────────────────────────────────┤
│                                                         │
│   ┌──────────────┐         ┌───────────────┐          │
│   │ CAMERA       │◄────────┤ Input System  │          │
│   │ CONTROLLER   │         └───────────────┘          │
│   └──────┬───────┘                                     │
│          │                                             │
│          │ renders                                     │
│          ▼                                             │
│   ┌──────────────┐                                     │
│   │ LAYER SYSTEM │◄─────┐                              │
│   └──────┬───────┘      │ uses                        │
│          │              │                              │
│          │ manages      │                              │
│          ▼              │                              │
│   ┌──────────────┐      │                              │
│   │  DECORATIVE  │──────┘                              │
│   │   OBJECTS    │                                     │
│   └──────┬───────┘                                     │
│          │                                             │
│          │ snaps to                                    │
│          ▼                                             │
│   ┌──────────────┐                                     │
│   │ GRID SYSTEM  │                                     │
│   └──────────────┘                                     │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## Componentes

### 1. LayerSystem

**Arquivo:** `game/piii/Assets/Scripts/Gameplay/LayerSystem.cs`

**Responsabilidades:**
- Definir camadas disponíveis (Background, Furniture_Back, etc.)
- Calcular profundidade Z para cada camada
- Gerenciar sorting order para renderização
- Aplicar camadas aos objetos
- Suportar ordenação automática por posição Y

**Estrutura de Dados:**

```csharp
public class LayerDefinition
{
    string name;           // Nome único da camada
    int sortingOrder;      // Ordem de renderização
    float depth;           // Profundidade Z
    string description;    // Descrição
}
```

**Camadas Padrão:**

| Nome | Z | Sort | Descrição |
|------|---|------|-----------|
| Background | 0.0 | 0 | Parede de fundo |
| Furniture_Back | -1.0 | 100 | Móveis grandes |
| Furniture_Mid | -2.0 | 200 | Móveis médios |
| Furniture_Front | -3.0 | 300 | Móveis pequenos |
| Decorations | -4.0 | 400 | Decorações |
| Floor | -5.0 | 500 | Objetos no chão |

**API Principal:**

```csharp
// Obter profundidade
float GetLayerDepth(string layerName)

// Aplicar camada a objeto
void SetObjectLayer(GameObject obj, string layerName, bool maintainXY = true)

// Atualizar sorting por Y
void UpdateObjectSorting(GameObject obj, string layerName)

// Verificar existência
bool LayerExists(string layerName)

// Obter definição
LayerDefinition GetLayer(string layerName)
```

### 2. CameraController

**Arquivo:** `game/piii/Assets/Scripts/Gameplay/CameraController.cs`

**Responsabilidades:**
- Configurar câmera como ortográfica
- Controlar zoom (scroll do mouse)
- Controlar pan (movimento)
- Focar em objetos com animação suave
- Respeitar limites de movimento

**Configurações:**

```csharp
// Câmera
float orthographicSize = 5f;      // Tamanho da visão
float cameraDistance = 10f;       // Distância em Z

// Zoom
bool allowZoom = true;
float zoomSpeed = 2f;
Vector2 zoomRange = (3f, 10f);    // Min, Max

// Pan
bool allowPan = true;
float panSpeed = 0.5f;
bool usePanBounds = true;
Bounds panBounds;                  // Limites de movimento

// Focus
float defaultFocusDuration = 0.5f;
AnimationCurve focusCurve;         // Curva de animação
```

**API Principal:**

```csharp
// Foco
void FocusOn(Vector3 targetPosition, float duration = -1, float targetZoom = -1)
void FocusOnObject(GameObject target, float duration = -1, float targetZoom = -1)
void ResetToInitialPosition(float duration = -1)

// Configuração
void SetOrthographicSize(float size, bool instant = false)
void SetPosition(Vector3 position, bool instant = false)
void SetPanBounds(Bounds bounds)
void SetControlsEnabled(bool zoom, bool pan)

// Conversão
Vector3 ScreenToWorldPoint(Vector3 screenPosition)
Vector3 WorldToScreenPoint(Vector3 worldPosition)
```

### 3. DecorativeObject

**Arquivo:** `game/piii/Assets/Scripts/Gameplay/DecorativeObject.cs`

**Novas Funcionalidades:**
- Propriedade `_layerName` para definir camada
- Propriedade `_autoUpdateSorting` para Y-sorting
- Referência a `_layerSystem`
- Aplicação automática de camada em Awake
- Atualização de sorting em LateUpdate

**Novos Métodos:**

```csharp
// Gerenciamento de camada
void ApplyLayer(string layerName)           // Aplica camada inicial
void ChangeLayer(string newLayerName)       // Muda para outra camada
LayerDefinition GetCurrentLayerInfo()       // Info da camada atual

// Propriedades
string LayerName { get; }                   // Camada atual
```

**Integração:**

```csharp
// Em Awake
_layerSystem = _currentLevel.GetComponent<LayerSystem>();
ApplyLayer(_layerName);

// Em LateUpdate (se autoUpdateSorting)
_layerSystem.UpdateObjectSorting(gameObject, _layerName);

// Em OnPointerUp (ao soltar objeto)
Vector3 snapped = _gridSystem.SnapToGridWithLayer(position, _layerName);
_layerSystem.SetObjectLayer(gameObject, _layerName);
```

### 4. GridSystem

**Arquivo:** `game/piii/Assets/Scripts/Gameplay/GridSystem.cs`

**Novos Métodos:**

```csharp
// Snap com camada
Vector3 SnapToGridWithLayer(Vector3 position, string layerName)

// Validação com camada
bool IsOnGridAndLayer(Vector3 position, string layerName)
```

**Implementação:**

```csharp
public Vector3 SnapToGridWithLayer(Vector3 position, string layerName)
{
    // 1. Snap X e Y ao grid
    Vector3 snapped = SnapToGridXZ(position);
    
    // 2. Obter Z da camada
    LayerSystem layerSystem = GetComponent<LayerSystem>();
    if (layerSystem != null && layerSystem.LayerExists(layerName))
    {
        snapped.z = layerSystem.GetLayerDepth(layerName);
    }
    
    return snapped;
}
```

## Fluxo de Dados

### Inicialização de um Nível

```
1. Level.Awake()
   └─> Inicializa componentes

2. LayerSystem.Awake()
   └─> Constrói dicionário de camadas

3. CameraController.Awake()
   └─> Configura câmera como ortográfica
   └─> Posiciona em (0, 0, -10)

4. DecorativeObject.Awake() [para cada objeto]
   └─> Obtém referência ao LayerSystem
   └─> Aplica camada inicial
       └─> LayerSystem.SetObjectLayer()
           ├─> Ajusta posição Z
           └─> Configura sorting order
```

### Interação do Jogador

```
1. Jogador clica em objeto
   └─> DecorativeObject.OnPointerDown()
       └─> _isDragging = true

2. Jogador arrasta objeto
   └─> DecorativeObject.OnDrag()
       └─> Atualiza posição X,Y (mantém Z)

3. Jogador solta objeto
   └─> DecorativeObject.OnPointerUp()
       ├─> GridSystem.SnapToGridWithLayer()
       │   ├─> Snap X,Y ao grid
       │   └─> Define Z pela camada
       └─> LayerSystem.SetObjectLayer()
           └─> Confirma Z e sorting

4. Frame seguinte (se autoUpdateSorting)
   └─> DecorativeObject.LateUpdate()
       └─> LayerSystem.UpdateObjectSorting()
           └─> Ajusta sorting baseado em Y
```

### Controle de Câmera

```
1. Jogador usa scroll
   └─> CameraController.HandleZoom()
       ├─> Ajusta _targetOrthographicSize
       └─> LateUpdate aplica suavizado

2. Jogador arrasta com botão do meio
   └─> CameraController.HandlePan()
       ├─> Calcula delta do mouse
       ├─> Converte para world space
       ├─> Aplica limites (se usePanBounds)
       └─> Atualiza transform.position

3. Script solicita foco
   └─> CameraController.FocusOn()
       └─> Inicia corrotina FocusCoroutine()
           ├─> Interpola posição com curva
           └─> Interpola zoom com curva
```

## Matemática do Sistema

### Cálculo de Sorting Order

```csharp
sortingOrder = baseSortingOrder + (yOffset)

onde:
  baseSortingOrder = camada.sortingOrder (ex: 200 para Furniture_Mid)
  yOffset = -(int)(positionY * ySortingMultiplier)
  ySortingMultiplier = 100 (configurável)

Exemplo:
  Objeto em Furniture_Mid (sort=200) na posição Y=2.5
  sortingOrder = 200 + (-(2.5 * 100))
  sortingOrder = 200 - 250 = -50
  
  Objeto em Furniture_Mid (sort=200) na posição Y=-1.0
  sortingOrder = 200 + (-(-1.0 * 100))
  sortingOrder = 200 + 100 = 300

Conclusão: Objetos mais baixos (Y menor) têm sorting maior (frente)
```

### Conversão de Coordenadas

```csharp
// Screen → World (ortográfica)
Vector3 ScreenToWorld(Vector3 screen)
{
    // Z define a distância do plano
    screen.z = camera.WorldToScreenPoint(transform.position).z;
    return camera.ScreenToWorldPoint(screen);
}

// World → Screen
Vector3 WorldToScreen(Vector3 world)
{
    return camera.WorldToScreenPoint(world);
}
```

### Snap ao Grid

```csharp
// Snap simples (X, Y, Z)
snapped.x = Round(position.x / gridSize) * gridSize
snapped.y = Round(position.y / gridSize) * gridSize
snapped.z = Round(position.z / gridSize) * gridSize

// Snap 2.5D (X, Y ao grid, Z pela camada)
snapped.x = Round(position.x / gridSize) * gridSize
snapped.y = position.y  // Mantém Y
snapped.z = layerDepth  // Z da camada
```

## Padrões de Design Aplicados

### 1. Component Pattern
- Cada sistema é um componente independente
- Composição sobre herança
- LayerSystem, CameraController são componentes

### 2. Data-Driven Design
- Camadas definidas em dados (array serializado)
- Fácil adicionar/modificar camadas sem código
- Inspector-friendly

### 3. Observer Pattern (implícito)
- CameraController reage a input
- DecorativeObject reage a eventos de ponteiro
- LayerSystem é consultado quando necessário

### 4. State Pattern
- Camera tem estados (_isFocusing, _isPanning)
- DecorativeObject tem estados (_isDragging, _isPlaced)

## Otimizações

### 1. Caching
- Dicionário de camadas construído em Awake (O(1) lookup)
- Referências armazenadas (evita GetComponent)

### 2. Lazy Evaluation
- Sorting só atualiza se `autoUpdateSorting = true`
- Gizmos só desenham no Editor

### 3. Smooth Damping
- Zoom usa SmoothDamp para performance
- Evita lerp linear frame-a-frame

### 4. Coroutines
- Animações de foco usam corrotinas
- Não bloqueia Update loop

## Limitações e Considerações

### Limitações

1. **Número de camadas:** Ilimitado, mas ~6-10 é ideal
2. **Objetos por camada:** Sem limite técnico, ~50-100 por camada é confortável
3. **Y-sorting:** Custo O(n) em LateUpdate, desative para objetos estáticos
4. **Z-fighting:** Pode ocorrer se dois objetos têm Z idêntico

### Considerações

1. **Naming:** Nomes de camadas são case-sensitive
2. **Z-Order:** Menor Z = mais perto da câmera
3. **Sorting:** Maior sort = renderiza depois = na frente
4. **Performance:** Auto-sorting tem custo, use criteriosamente

## Testes Recomendados

### Testes Funcionais

1. ✅ Objeto é posicionado na profundidade Z correta
2. ✅ Objetos renderizam em ordem correta
3. ✅ Y-sorting funciona (objetos mais baixos na frente)
4. ✅ Snap ao grid mantém Z da camada
5. ✅ Mudança de camada atualiza Z
6. ✅ Reset retorna à camada inicial

### Testes de Câmera

1. ✅ Zoom in/out funciona
2. ✅ Pan funciona
3. ✅ Limites são respeitados
4. ✅ Foco anima suavemente
5. ✅ Reset retorna à posição inicial

### Testes de Integração

1. ✅ LayerSystem + GridSystem funcionam juntos
2. ✅ CameraController não interfere em gameplay
3. ✅ DecorativeObject usa camadas corretamente
4. ✅ Performance aceitável com 100+ objetos

## Extensões Futuras

### Possíveis Melhorias

1. **Parallax:** Camadas de fundo com parallax
2. **Dynamic Layers:** Criar/remover camadas em runtime
3. **Layer Groups:** Agrupar camadas relacionadas
4. **Occlusion:** Objetos traseiros não renderizam se ocultos
5. **Lighting 2.5D:** Sombras por camada
6. **Collision Layers:** Colisão apenas entre mesmas camadas

### APIs Adicionais

```csharp
// LayerSystem
void CreateLayer(string name, float depth, int sorting)
void RemoveLayer(string name)
void SwapLayers(string layer1, string layer2)

// CameraController
void ShakeCamera(float intensity, float duration)
void TiltCamera(float angle, float duration)
void FollowObject(GameObject target, bool smooth)

// DecorativeObject
void AnimateToLayer(string newLayer, float duration)
bool CanChangeToLayer(string newLayer)  // Validação
```

## Diagramas

### Diagrama de Classes

```
┌─────────────────┐
│  MonoBehaviour  │
└────────┬────────┘
         │
    ┌────┴──────────────┬─────────────────┬──────────────────┐
    │                   │                 │                  │
┌───▼──────────┐  ┌─────▼────────┐  ┌────▼──────────┐  ┌───▼──────────┐
│ LayerSystem  │  │CameraControl │  │DecorativeObj  │  │ GridSystem   │
│              │  │              │  │               │  │              │
│+GetDepth()   │  │+FocusOn()    │  │+ChangeLayer() │  │+SnapToGrid   │
│+SetLayer()   │  │+SetZoom()    │  │+OnDrag()      │  │ WithLayer()  │
│+UpdateSort() │  │+SetPos()     │  │+OnPointerUp() │  │+IsOnGrid()   │
└──────────────┘  └──────────────┘  └───────────────┘  └──────────────┘
```

### Diagrama de Sequência (Colocar Objeto)

```
Jogador    DecorativeObject    GridSystem    LayerSystem
   │              │                │              │
   │─OnPointerUp─>│                │              │
   │              │                │              │
   │              │─SnapToGrid─────>│              │
   │              │    WithLayer    │              │
   │              │                │              │
   │              │<───snapped──────│              │
   │              │   position      │              │
   │              │                │              │
   │              │─SetObjectLayer────────────────>│
   │              │                │              │
   │              │<───Z & Sort─────────────────────│
   │              │   configured    │              │
   │              │                │              │
   │<─feedback────│                │              │
   │  (som/visual)│                │              │
```

## Referências Técnicas

- Unity Docs: Camera (Orthographic Projection)
- Unity Docs: Sprite Renderer (Sorting)
- Unity Docs: Transform (Position & Rotation)
- Game Dev Gems: 2.5D Games Techniques
- Unpacking (jogo referência)

---

**Versão:** 1.0  
**Data:** 07/11/2025  
**Autor:** Sistema de IA - Projeto Integrador II  
**Status:** ✅ Implementado e Documentado

