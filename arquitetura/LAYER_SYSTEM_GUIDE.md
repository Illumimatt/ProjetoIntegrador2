# 🎨 Sistema de Camadas 2.5D - Guia Completo

## Visão Geral

O **Sistema de Camadas 2.5D** do Dekora permite organizar objetos em diferentes profundidades visuais, criando uma sensação de profundidade mesmo com câmera ortogonal. Este sistema é essencial para jogos isométricos e 2.5D como o Dekora.

## 📐 Conceito

```
VISTA LATERAL (como a câmera vê):

Background ──────┐         (Z = 0)   ← Parede de fundo
                 │
Furniture_Back ──┤         (Z = -1)  ← Cama, armário
                 │
Furniture_Mid ───┤         (Z = -2)  ← Mesa, cadeira
                 │
Furniture_Front ─┤         (Z = -3)  ← Objetos pequenos
                 │
Decorations ─────┤         (Z = -4)  ← Quadros, plantas
                 │
Floor ───────────┘         (Z = -5)  ← Tapetes, almofadas

        ▲
        │
    Câmera Ortogonal
   (olhando de frente)
```

## 🏗️ Componentes do Sistema

### 1. LayerSystem.cs

Gerencia todas as camadas do nível e aplica profundidade aos objetos.

**Localização:** `Assets/Scripts/Gameplay/LayerSystem.cs`

**Funcionalidades:**
- Define camadas disponíveis
- Calcula profundidade (Z) para cada camada
- Aplica sorting order para renderização
- Suporta ordenação automática por Y

### 2. CameraController.cs

Controla a câmera ortogonal 2.5D.

**Localização:** `Assets/Scripts/Gameplay/CameraController.cs`

**Funcionalidades:**
- Projeção ortográfica
- Zoom com scroll
- Pan (movimento) com mouse
- Foco suave em objetos
- Limites de movimento

### 3. DecorativeObject.cs (Atualizado)

Objetos decoráveis agora suportam camadas.

**Novas funcionalidades:**
- Configuração de camada
- Mudança dinâmica de camada
- Ordenação automática por Y
- Snap com profundidade Z correta

### 4. GridSystem.cs (Atualizado)

Sistema de grid agora considera camadas.

**Novos métodos:**
- `SnapToGridWithLayer()` - Snap com Z da camada
- `IsOnGridAndLayer()` - Valida posição e camada

## 📚 Configuração no Unity

### Passo 1: Adicionar LayerSystem ao Nível

1. Selecione o GameObject **Level** na hierarquia
2. Clique em **Add Component**
3. Procure por **Layer System** e adicione
4. As camadas padrão já vêm configuradas:

```
• Background (Z=0, Sort=0)
• Furniture_Back (Z=-1, Sort=100)
• Furniture_Mid (Z=-2, Sort=200)
• Furniture_Front (Z=-3, Sort=300)
• Decorations (Z=-4, Sort=400)
• Floor (Z=-5, Sort=500)
```

### Passo 2: Configurar a Câmera

1. Selecione a **Main Camera**
2. Adicione o componente **Camera Controller**
3. Configure:
   ```
   Projection: Orthographic ✓
   Orthographic Size: 5
   Camera Distance: 10
   
   Zoom:
   - Allow Zoom: ✓
   - Zoom Speed: 2
   - Zoom Range: (3, 10)
   
   Pan:
   - Allow Pan: ✓
   - Pan Speed: 0.5
   - Use Pan Bounds: ✓
   - Pan Bounds: Center (0,0,0), Size (20,20,0)
   ```

### Passo 3: Configurar Objetos Decoráveis

Para cada objeto decorável:

1. Adicione o componente **Decorative Object**
2. Na seção **Sistema de Camadas 2.5D**:
   ```
   Layer Name: Furniture_Mid (ou a camada apropriada)
   Auto Update Sorting: ✓
   ```

3. O objeto será automaticamente posicionado na profundidade Z correta!

## 💻 API e Exemplos de Código

### LayerSystem

```csharp
// Obter referência
LayerSystem layerSystem = level.GetComponent<LayerSystem>();

// Aplicar camada a um objeto
layerSystem.SetObjectLayer(myObject, "Furniture_Mid");

// Obter profundidade de uma camada
float depth = layerSystem.GetLayerDepth("Decorations"); // -4.0

// Obter sorting order de uma camada
int sortOrder = layerSystem.GetSortingOrder("Floor"); // 500

// Verificar se camada existe
bool exists = layerSystem.LayerExists("Custom_Layer");

// Atualizar sorting por posição Y
layerSystem.UpdateObjectSorting(myObject, "Furniture_Mid");

// Listar todas as camadas (Debug)
layerSystem.DebugListLayers();
```

### CameraController

```csharp
// Obter referência
CameraController cam = Camera.main.GetComponent<CameraController>();

// Focar em posição
cam.FocusOn(new Vector3(5, 0, 0), duration: 1f);

// Focar em objeto
cam.FocusOnObject(furnitureObject, duration: 0.5f);

// Focar com zoom específico
cam.FocusOn(targetPos, duration: 1f, targetZoom: 4f);

// Voltar para posição inicial
cam.ResetToInitialPosition(duration: 1f);

// Definir zoom
cam.SetOrthographicSize(6f, instant: false);

// Definir posição
cam.SetPosition(new Vector3(10, 5, -10), instant: true);

// Controlar habilitação
cam.SetControlsEnabled(zoom: true, pan: false);

// Conversão de coordenadas
Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
Vector3 screenPos = cam.WorldToScreenPoint(objectPosition);
```

### DecorativeObject (Camadas)

```csharp
// Obter referência
DecorativeObject obj = GetComponent<DecorativeObject>();

// Mudar para outra camada
obj.ChangeLayer("Furniture_Front");

// Obter camada atual
string currentLayer = obj.LayerName;

// Obter informações da camada
LayerSystem.LayerDefinition layerInfo = obj.GetCurrentLayerInfo();
Debug.Log($"Camada: {layerInfo.name}, Z: {layerInfo.depth}");
```

### GridSystem (com Camadas)

```csharp
// Obter referência
GridSystem grid = level.GridSystem;

// Snap ao grid com camada
Vector3 snapped = grid.SnapToGridWithLayer(position, "Furniture_Mid");
transform.position = snapped;

// Verificar se está no grid e na camada
bool isValid = grid.IsOnGridAndLayer(position, "Decorations");
```

## 🎮 Controles do Jogador

### Câmera

| Ação | Controle | Descrição |
|------|----------|-----------|
| **Zoom In** | Scroll Up | Aproxima a câmera |
| **Zoom Out** | Scroll Down | Afasta a câmera |
| **Pan** | Botão do Meio + Arrastar | Move a câmera |
| **Foco** | - | Programático (via script) |

### Objetos

| Ação | Controle | Descrição |
|------|----------|-----------|
| **Pegar** | Click | Clica no objeto |
| **Arrastar** | Click + Arrastar | Move o objeto |
| **Soltar** | Release | Coloca no grid+camada |
| **Rotacionar** | Botão UI | Roda o objeto (via script) |

## 🎨 Camadas Padrão

### Background
- **Profundidade:** Z = 0
- **Sorting:** 0
- **Uso:** Parede de fundo, elementos fixos
- **Interativo:** Não

### Furniture_Back
- **Profundidade:** Z = -1
- **Sorting:** 100
- **Uso:** Móveis grandes (cama, armário, estante)
- **Interativo:** Sim

### Furniture_Mid
- **Profundidade:** Z = -2
- **Sorting:** 200
- **Uso:** Móveis médios (mesa, cadeira, sofá)
- **Interativo:** Sim

### Furniture_Front
- **Profundidade:** Z = -3
- **Sorting:** 300
- **Uso:** Móveis pequenos (banqueta, criado-mudo)
- **Interativo:** Sim

### Decorations
- **Profundidade:** Z = -4
- **Sorting:** 400
- **Uso:** Decorações (quadros, plantas, luminárias)
- **Interativo:** Sim

### Floor
- **Profundidade:** Z = -5
- **Sorting:** 500
- **Uso:** Objetos no chão (tapetes, almofadas)
- **Interativo:** Sim

## 🔧 Configuração Avançada

### Adicionar Nova Camada

1. Abra o script `LayerSystem.cs`
2. Localize o array `_layers` no Inspector
3. Aumente o **Size**
4. Preencha os campos:
   ```
   Name: Custom_Layer
   Sorting Order: 350 (entre outras camadas)
   Depth: -2.5 (profundidade Z)
   Description: Descrição da camada
   ```

### Ordenação por Y (Auto-Sorting)

Para objetos na mesma camada serem ordenados pela posição Y:

1. No **LayerSystem**, ative:
   ```
   Use Y Sorting: ✓
   Y Sorting Multiplier: 100
   ```

2. No **DecorativeObject**:
   ```
   Auto Update Sorting: ✓
   ```

**Como funciona:**
- Objetos mais baixos (Y menor) aparecem na frente
- Multiplicador aumenta a sensibilidade

### Limites da Câmera

Para restringir movimento da câmera:

1. No **CameraController**:
   ```
   Use Pan Bounds: ✓
   Pan Bounds:
     Center: (0, 0, 0)
     Size: (20, 20, 0)  ← Ajuste conforme tamanho do nível
   ```

2. A câmera não poderá sair desses limites

### Gizmos de Visualização

**LayerSystem** desenha planos coloridos na Scene View:
- Cada camada tem uma cor diferente
- Mostra a profundidade Z visualmente
- Apenas visível no Editor, não no jogo

**GridSystem** desenha a grade:
- Linhas verdes mostram o grid
- Configure `Show Grid` para ativar/desativar

**CameraController** desenha:
- Amarelo: Limites de movimento
- Verde: Posição inicial

## 🎯 Workflows Comuns

### Criar um Quarto Decorável

```csharp
// 1. Setup do nível
GameObject level = new GameObject("Level_Quarto1");
level.AddComponent<Level>();
level.AddComponent<LayerSystem>();
level.AddComponent<GridSystem>();

// 2. Setup da câmera
Camera.main.gameObject.AddComponent<CameraController>();

// 3. Adicionar fundo (não interativo)
GameObject parede = CreateSprite("Parede", "Background");
// Parede não precisa de DecorativeObject

// 4. Adicionar móveis (interativos)
GameObject cama = CreateFurniture("Cama", "Furniture_Back");
GameObject mesa = CreateFurniture("Mesa", "Furniture_Mid");
GameObject luminaria = CreateFurniture("Luminária", "Decorations");

GameObject CreateFurniture(string name, string layer)
{
    GameObject obj = new GameObject(name);
    obj.transform.parent = level.transform;
    
    // Adicionar visual (sprite ou modelo 3D)
    SpriteRenderer sprite = obj.AddComponent<SpriteRenderer>();
    
    // Adicionar collider
    obj.AddComponent<BoxCollider2D>();
    
    // Adicionar componente decorável
    DecorativeObject deco = obj.AddComponent<DecorativeObject>();
    // Configurar layer via Inspector ou:
    deco.ChangeLayer(layer);
    
    return obj;
}
```

### Transição Suave de Foco

```csharp
// Quando jogador seleciona um objeto
public void OnObjectSelected(GameObject obj)
{
    CameraController cam = Camera.main.GetComponent<CameraController>();
    
    // Focar no objeto com zoom
    cam.FocusOnObject(obj, duration: 0.8f, targetZoom: 4f);
}

// Quando volta para visão geral
public void OnViewAllClick()
{
    CameraController cam = Camera.main.GetComponent<CameraController>();
    cam.ResetToInitialPosition(duration: 1f);
}
```

### Validação de Colocação

```csharp
// Verificar se objeto pode ser colocado
public bool CanPlaceObject(Vector3 position, string layerName)
{
    GridSystem grid = level.GridSystem;
    
    // Verifica grid e camada
    if (!grid.IsOnGridAndLayer(position, layerName))
        return false;
    
    // Verifica colisão com outros objetos
    Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
    if (colliders.Length > 0)
        return false;
    
    return true;
}
```

## 🐛 Troubleshooting

### Problema: Objetos renderizam em ordem errada

**Solução:**
1. Verifique se `LayerSystem` está no GameObject `Level`
2. Confirme que `DecorativeObject` tem a camada correta configurada
3. Verifique se `Auto Update Sorting` está ativo
4. Aumente `Y Sorting Multiplier` para maior separação

### Problema: Câmera não move

**Solução:**
1. Verifique se está no estado `Playing` ou `LevelReview`
2. Confirme que `Allow Pan` está ativado
3. Use o botão do meio do mouse (não esquerdo/direito)
4. Verifique se não está em `_isFocusing`

### Problema: Snap não funciona com camadas

**Solução:**
1. Use `SnapToGridWithLayer()` ao invés de `SnapToGrid()`
2. Verifique que `LayerSystem` existe no mesmo GameObject que `GridSystem`
3. Confirme que o nome da camada está correto (case-sensitive)

### Problema: Objetos "flutuam" no Z errado

**Solução:**
1. Chame `layerSystem.SetObjectLayer()` após mover o objeto
2. Certifique-se que `OnPointerUp()` está aplicando a camada
3. Verifique profundidade Z da camada: `layerSystem.GetLayerDepth()`

## 📊 Performance

### Otimizações

1. **Y-Sorting:** Só ative `Auto Update Sorting` em objetos que se movem
2. **Gizmos:** Desative em build (já é automático)
3. **Layer Updates:** Atualize camada apenas quando necessário
4. **Camera Focus:** Use corrotinas para animações suaves

### Métricas

- **Custo de LayerSystem:** Negligível (cálculos simples)
- **Custo de CameraController:** Baixo (apenas durante input)
- **Custo de Y-Sorting:** Médio (em LateUpdate, por objeto)
  - Limite: ~200 objetos com auto-sorting
  - Solução: Desative para objetos estáticos

## 🎓 Conceitos Técnicos

### Câmera Ortográfica vs Perspectiva

| Característica | Ortográfica | Perspectiva |
|----------------|-------------|-------------|
| Tamanho | Constante | Varia com distância |
| Paralelas | Permanecem paralelas | Convergem ao longe |
| Profundidade | Não aparente | Aparente |
| Uso | 2D, 2.5D, Isométrico | 3D realista |

### Sorting Order

Ordem de renderização de sprites:
- **Menor valor** = renderiza primeiro = **atrás**
- **Maior valor** = renderiza depois = **na frente**

### Z-Depth em Ortográfica

Mesmo em ortográfica, Z importa:
- Câmera renderiza objetos do mais longe ao mais perto
- Z negativo = mais perto da câmera
- Z positivo = mais longe da câmera

## 📖 Referências

- **Unity Manual:** Cameras (Orthographic)
- **Unity Manual:** Sprite Renderer (Sorting)
- **Game Dev Pattern:** 2.5D Games
- **Referência:** Unpacking (jogo similar)

## ✅ Checklist de Implementação

- [ ] LayerSystem adicionado ao Level
- [ ] CameraController adicionado à Main Camera
- [ ] Câmera configurada como Orthographic
- [ ] DecorativeObjects configurados com camadas
- [ ] GridSystem atualizado para usar camadas
- [ ] Y-Sorting configurado
- [ ] Limites de câmera definidos
- [ ] Controles testados (zoom e pan)
- [ ] Objetos renderizam na ordem correta
- [ ] Performance aceitável

---

**Versão:** 1.0  
**Data:** 07/11/2025  
**Status:** ✅ Sistema Completo e Funcional

Para dúvidas, consulte a [documentação completa](README.md) ou os scripts fonte!

