using System.Collections.Generic;
using UnityEngine;

namespace Dekora.Gameplay
{
    /// <summary>
    /// LayerSystem - Sistema de gerenciamento de camadas para jogo 2.5D.
    /// Controla a profundidade (Z) e ordem de renderização dos objetos.
    /// 
    /// Em um jogo 2.5D com câmera ortogonal, as camadas determinam:
    /// - Profundidade visual (posição Z)
    /// - Ordem de renderização (sorting order)
    /// - Organização lógica dos objetos
    /// 
    /// Responsabilidades:
    /// - Definir camadas disponíveis no nível
    /// - Calcular profundidade para cada camada
    /// - Aplicar camada a objetos
    /// - Gerenciar colisões entre camadas
    /// </summary>
    public class LayerSystem : MonoBehaviour
    {
        #region Layer Configuration
        
        [Header("Configuração de Camadas")]
        [SerializeField]
        [Tooltip("Definição de todas as camadas do nível")]
        private LayerDefinition[] _layers = new LayerDefinition[]
        {
            new LayerDefinition { 
                name = "Background", 
                sortingOrder = 0, 
                depth = 0f,
                description = "Parede de fundo e elementos fixos"
            },
            new LayerDefinition { 
                name = "Furniture_Back", 
                sortingOrder = 100, 
                depth = -1f,
                description = "Móveis grandes no fundo (cama, armário)"
            },
            new LayerDefinition { 
                name = "Furniture_Mid", 
                sortingOrder = 200, 
                depth = -2f,
                description = "Móveis médios (mesa, cadeira)"
            },
            new LayerDefinition { 
                name = "Furniture_Front", 
                sortingOrder = 300, 
                depth = -3f,
                description = "Móveis pequenos na frente"
            },
            new LayerDefinition { 
                name = "Decorations", 
                sortingOrder = 400, 
                depth = -4f,
                description = "Decorações (quadros, plantas, luminárias)"
            },
            new LayerDefinition { 
                name = "Floor", 
                sortingOrder = 500, 
                depth = -5f,
                description = "Objetos no chão (tapetes, almofadas)"
            }
        };
        
        [Header("Configurações Avançadas")]
        [SerializeField]
        [Tooltip("Usar ordenação automática por posição Y dentro da mesma camada")]
        private bool _useYSorting = true;
        
        [SerializeField]
        [Tooltip("Multiplicador para ordenação por Y (quanto maior, mais sensível)")]
        private float _ySortingMultiplier = 100f;
        
        private Dictionary<string, LayerDefinition> _layerDict;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            BuildLayerDictionary();
            Debug.Log($"[LayerSystem] Inicializado com {_layers.Length} camadas.");
        }
        
        private void BuildLayerDictionary()
        {
            _layerDict = new Dictionary<string, LayerDefinition>();
            
            foreach (var layer in _layers)
            {
                if (!string.IsNullOrEmpty(layer.name))
                {
                    _layerDict[layer.name] = layer;
                }
            }
        }
        
        #endregion
        
        #region Layer Management
        
        /// <summary>
        /// Retorna a profundidade (Z) para uma camada específica
        /// </summary>
        /// <param name="layerName">Nome da camada</param>
        /// <returns>Valor Z da camada</returns>
        public float GetLayerDepth(string layerName)
        {
            if (_layerDict != null && _layerDict.TryGetValue(layerName, out LayerDefinition layer))
            {
                return layer.depth;
            }
            
            Debug.LogWarning($"[LayerSystem] Camada '{layerName}' não encontrada! Usando profundidade padrão.");
            return 0f;
        }
        
        /// <summary>
        /// Retorna o sorting order base para uma camada
        /// </summary>
        /// <param name="layerName">Nome da camada</param>
        /// <returns>Sorting order da camada</returns>
        public int GetSortingOrder(string layerName)
        {
            if (_layerDict != null && _layerDict.TryGetValue(layerName, out LayerDefinition layer))
            {
                return layer.sortingOrder;
            }
            
            Debug.LogWarning($"[LayerSystem] Camada '{layerName}' não encontrada! Usando sorting order padrão.");
            return 0;
        }
        
        /// <summary>
        /// Retorna a definição completa de uma camada
        /// </summary>
        public LayerDefinition GetLayer(string layerName)
        {
            if (_layerDict != null && _layerDict.TryGetValue(layerName, out LayerDefinition layer))
            {
                return layer;
            }
            
            return null;
        }
        
        /// <summary>
        /// Retorna todas as camadas disponíveis
        /// </summary>
        public LayerDefinition[] GetAllLayers()
        {
            return _layers;
        }
        
        /// <summary>
        /// Verifica se uma camada existe
        /// </summary>
        public bool LayerExists(string layerName)
        {
            return _layerDict != null && _layerDict.ContainsKey(layerName);
        }
        
        #endregion
        
        #region Object Layer Application
        
        /// <summary>
        /// Aplica uma camada a um objeto, ajustando posição Z e sorting
        /// </summary>
        /// <param name="obj">GameObject a ser configurado</param>
        /// <param name="layerName">Nome da camada</param>
        /// <param name="maintainXY">Se true, mantém posições X e Y atuais</param>
        public void SetObjectLayer(GameObject obj, string layerName, bool maintainXY = true)
        {
            if (obj == null)
            {
                Debug.LogError("[LayerSystem] GameObject é nulo!");
                return;
            }
            
            if (!LayerExists(layerName))
            {
                Debug.LogError($"[LayerSystem] Camada '{layerName}' não existe!");
                return;
            }
            
            LayerDefinition layer = GetLayer(layerName);
            
            // Ajustar posição Z (profundidade)
            Vector3 pos = obj.transform.position;
            if (maintainXY)
            {
                pos.z = layer.depth;
            }
            else
            {
                pos = new Vector3(pos.x, pos.y, layer.depth);
            }
            obj.transform.position = pos;
            
            // Ajustar Sorting Order para Sprite Renderers
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = CalculateSortingOrder(layer, obj.transform.position.y);
                
                Debug.Log($"[LayerSystem] '{obj.name}' → Camada '{layerName}' (Z: {layer.depth}, Sort: {spriteRenderer.sortingOrder})");
            }
            else
            {
                Debug.Log($"[LayerSystem] '{obj.name}' → Camada '{layerName}' (Z: {layer.depth})");
            }
            
            // Para objetos 3D, a posição Z é suficiente para ordenação com câmera ortográfica
        }
        
        /// <summary>
        /// Calcula o sorting order considerando a posição Y (opcional)
        /// </summary>
        private int CalculateSortingOrder(LayerDefinition layer, float yPosition)
        {
            if (!_useYSorting)
            {
                return layer.sortingOrder;
            }
            
            // Objetos mais baixos na tela aparecem na frente
            // Y negativo aumenta o sorting order
            int yOffset = -(int)(yPosition * _ySortingMultiplier);
            
            return layer.sortingOrder + yOffset;
        }
        
        /// <summary>
        /// Atualiza o sorting order de um objeto baseado em sua posição Y atual
        /// Útil para objetos que se movem
        /// </summary>
        public void UpdateObjectSorting(GameObject obj, string layerName)
        {
            if (!_useYSorting) return;
            
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                LayerDefinition layer = GetLayer(layerName);
                if (layer != null)
                {
                    spriteRenderer.sortingOrder = CalculateSortingOrder(layer, obj.transform.position.y);
                }
            }
        }
        
        #endregion
        
        #region Batch Operations
        
        /// <summary>
        /// Aplica camadas a múltiplos objetos de uma vez
        /// </summary>
        public void SetObjectsLayer(GameObject[] objects, string layerName)
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    SetObjectLayer(obj, layerName);
                }
            }
            
            Debug.Log($"[LayerSystem] {objects.Length} objetos configurados para camada '{layerName}'");
        }
        
        /// <summary>
        /// Encontra todos os objetos em uma camada específica
        /// </summary>
        public List<GameObject> GetObjectsInLayer(string layerName)
        {
            List<GameObject> objectsInLayer = new List<GameObject>();
            float targetDepth = GetLayerDepth(layerName);
            
            // Buscar todos os DecorativeObjects no nível
            DecorativeObject[] allObjects = GetComponentsInChildren<DecorativeObject>();
            
            foreach (var obj in allObjects)
            {
                if (Mathf.Approximately(obj.transform.position.z, targetDepth))
                {
                    objectsInLayer.Add(obj.gameObject);
                }
            }
            
            return objectsInLayer;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Retorna o nome da camada mais próxima de uma profundidade Z
        /// </summary>
        public string GetClosestLayerName(float depth)
        {
            string closestLayer = "Furniture_Mid"; // Default
            float minDistance = float.MaxValue;
            
            foreach (var layer in _layers)
            {
                float distance = Mathf.Abs(layer.depth - depth);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestLayer = layer.name;
                }
            }
            
            return closestLayer;
        }
        
        /// <summary>
        /// Debug: Lista todas as camadas no console
        /// </summary>
        [ContextMenu("Debug: Listar Camadas")]
        public void DebugListLayers()
        {
            Debug.Log("=== CAMADAS DO NÍVEL ===");
            foreach (var layer in _layers)
            {
                Debug.Log($"• {layer.name}: Z={layer.depth}, Sort={layer.sortingOrder} - {layer.description}");
            }
            Debug.Log("========================");
        }
        
        #endregion
        
        #region Gizmos (Visualização)
        
        private void OnDrawGizmos()
        {
            if (_layers == null || _layers.Length == 0) return;
            
            // Desenhar planos representando cada camada
            foreach (var layer in _layers)
            {
                Gizmos.color = GetLayerColor(layer.name);
                
                Vector3 center = new Vector3(0, 0, layer.depth);
                Vector3 size = new Vector3(20, 20, 0.01f);
                
                // Desenhar contorno do plano
                DrawWirePlane(center, size);
            }
        }
        
        private void DrawWirePlane(Vector3 center, Vector3 size)
        {
            Vector3 halfSize = size * 0.5f;
            
            Vector3 p1 = center + new Vector3(-halfSize.x, -halfSize.y, 0);
            Vector3 p2 = center + new Vector3(halfSize.x, -halfSize.y, 0);
            Vector3 p3 = center + new Vector3(halfSize.x, halfSize.y, 0);
            Vector3 p4 = center + new Vector3(-halfSize.x, halfSize.y, 0);
            
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
        }
        
        private Color GetLayerColor(string layerName)
        {
            // Cores diferentes para cada tipo de camada
            switch (layerName)
            {
                case "Background": return new Color(0.5f, 0.5f, 0.5f, 0.3f);
                case "Furniture_Back": return new Color(0.8f, 0.4f, 0.4f, 0.3f);
                case "Furniture_Mid": return new Color(0.4f, 0.8f, 0.4f, 0.3f);
                case "Furniture_Front": return new Color(0.4f, 0.4f, 0.8f, 0.3f);
                case "Decorations": return new Color(0.8f, 0.8f, 0.4f, 0.3f);
                case "Floor": return new Color(0.8f, 0.4f, 0.8f, 0.3f);
                default: return new Color(1f, 1f, 1f, 0.3f);
            }
        }
        
        #endregion
        
        #region Layer Definition Class
        
        /// <summary>
        /// Define uma camada 2.5D com suas propriedades
        /// </summary>
        [System.Serializable]
        public class LayerDefinition
        {
            [Tooltip("Nome único da camada")]
            public string name;
            
            [Tooltip("Ordem de renderização (maior = mais na frente)")]
            public int sortingOrder;
            
            [Tooltip("Profundidade Z (negativo = mais perto da câmera)")]
            public float depth;
            
            [Tooltip("Descrição da camada")]
            public string description;
            
            public override string ToString()
            {
                return $"{name} (Z:{depth}, Sort:{sortingOrder})";
            }
        }
        
        #endregion
    }
}

