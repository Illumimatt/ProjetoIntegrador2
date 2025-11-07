using System.Collections.Generic;
using UnityEngine;

namespace Dekora.Gameplay
{
    /// <summary>
    /// Level - Representa uma fase/nível do jogo Dekora.
    /// Contém todos os objetos decoráveis e o sistema de grid.
    /// 
    /// Responsabilidades:
    /// - Gerenciar objetos decoráveis no nível
    /// - Fornecer acesso ao GridSystem
    /// - Rastrear progresso/conclusão do nível
    /// - Armazenar metadados do nível (nome, dificuldade, etc.)
    /// </summary>
    public class Level : MonoBehaviour
    {
        #region Level Data
        
        [Header("Informações do Nível")]
        [SerializeField]
        [Tooltip("Nome do nível")]
        private string _levelName = "Novo Nível";
        
        [SerializeField]
        [Tooltip("Descrição do nível")]
        [TextArea(3, 5)]
        private string _levelDescription = "";
        
        [SerializeField]
        [Tooltip("Índice do nível (ordem no jogo)")]
        private int _levelIndex = 0;
        
        /// <summary>
        /// Nome do nível
        /// </summary>
        public string LevelName => _levelName;
        
        /// <summary>
        /// Descrição do nível
        /// </summary>
        public string LevelDescription => _levelDescription;
        
        /// <summary>
        /// Índice/ordem do nível
        /// </summary>
        public int LevelIndex => _levelIndex;
        
        #endregion
        
        #region Decorative Objects
        
        [Header("Objetos Decoráveis")]
        [SerializeField]
        [Tooltip("Lista de todos os objetos que podem ser decorados neste nível")]
        private List<DecorativeObject> _decorativeObjects = new List<DecorativeObject>();
        
        /// <summary>
        /// Lista de objetos decoráveis no nível
        /// </summary>
        public List<DecorativeObject> DecorativeObjects => _decorativeObjects;
        
        #endregion
        
        #region Grid System
        
        [Header("Sistema de Grid")]
        [SerializeField]
        [Tooltip("Sistema de grid para snapping de objetos")]
        private GridSystem _gridSystem;
        
        /// <summary>
        /// Sistema de grid do nível
        /// </summary>
        public GridSystem GridSystem => _gridSystem;
        
        #endregion
        
        #region Level State
        
        private bool _isComplete = false;
        private float _completionPercentage = 0f;
        
        /// <summary>
        /// Verdadeiro se o nível foi completado
        /// </summary>
        public bool IsComplete => _isComplete;
        
        /// <summary>
        /// Porcentagem de conclusão (0 a 100)
        /// </summary>
        public float CompletionPercentage => _completionPercentage;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Inicializar grid system se não estiver setado
            if (_gridSystem == null)
            {
                _gridSystem = GetComponent<GridSystem>();
                
                if (_gridSystem == null)
                {
                    _gridSystem = gameObject.AddComponent<GridSystem>();
                }
            }
            
            // Encontrar todos os DecorativeObjects filhos
            DiscoverDecorativeObjects();
        }
        
        private void Start()
        {
            Debug.Log($"[Level] '{_levelName}' inicializado com {_decorativeObjects.Count} objetos decoráveis.");
        }
        
        #endregion
        
        #region Level Methods
        
        /// <summary>
        /// Descobre automaticamente todos os DecorativeObjects filhos
        /// </summary>
        private void DiscoverDecorativeObjects()
        {
            _decorativeObjects.Clear();
            
            DecorativeObject[] foundObjects = GetComponentsInChildren<DecorativeObject>();
            _decorativeObjects.AddRange(foundObjects);
            
            Debug.Log($"[Level] {foundObjects.Length} objetos decoráveis descobertos.");
        }
        
        /// <summary>
        /// Calcula a porcentagem de conclusão do nível
        /// </summary>
        public void UpdateCompletionPercentage()
        {
            if (_decorativeObjects.Count == 0)
            {
                _completionPercentage = 0f;
                return;
            }
            
            int placedObjects = 0;
            foreach (var obj in _decorativeObjects)
            {
                if (obj.IsPlaced)
                {
                    placedObjects++;
                }
            }
            
            _completionPercentage = (placedObjects / (float)_decorativeObjects.Count) * 100f;
            
            // Verificar se o nível está completo
            if (_completionPercentage >= 100f && !_isComplete)
            {
                CompleteLevel();
            }
        }
        
        /// <summary>
        /// Marca o nível como completo
        /// </summary>
        private void CompleteLevel()
        {
            _isComplete = true;
            Debug.Log($"[Level] Nível '{_levelName}' completado!");
            
            // Notificar o GameManager
            Core.GameManager.Instance.CompleteCurrentLevel();
        }
        
        /// <summary>
        /// Reseta o nível para o estado inicial
        /// </summary>
        public void ResetLevel()
        {
            foreach (var obj in _decorativeObjects)
            {
                obj.ResetToInitialPosition();
            }
            
            _isComplete = false;
            _completionPercentage = 0f;
            
            Debug.Log($"[Level] Nível '{_levelName}' resetado.");
        }
        
        #endregion
    }
}

