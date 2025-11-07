using UnityEngine;
using UnityEngine.EventSystems;

namespace Dekora.Gameplay
{
    /// <summary>
    /// DecorativeObject - Componente que transforma um GameObject em um objeto decorável.
    /// Este é o componente principal da interação do jogador no Dekora.
    /// 
    /// Responsabilidades:
    /// - Detectar interação do jogador (clique/toque)
    /// - Permitir arrastar o objeto
    /// - Fazer snap ao grid quando solto
    /// - Manter estado (colocado, não colocado)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DecorativeObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        #region Object Properties
        
        [Header("Propriedades do Objeto")]
        [SerializeField]
        [Tooltip("Nome do objeto decorável")]
        private string _objectName = "Objeto Decorável";
        
        [SerializeField]
        [Tooltip("Categoria do objeto (móvel, decoração, etc)")]
        private string _category = "Decoração";
        
        [SerializeField]
        [Tooltip("Permite rotação do objeto")]
        private bool _canRotate = true;
        
        /// <summary>
        /// Nome do objeto
        /// </summary>
        public string ObjectName => _objectName;
        
        /// <summary>
        /// Categoria do objeto
        /// </summary>
        public string Category => _category;
        
        #endregion
        
        #region Layer System (2.5D)
        
        [Header("Sistema de Camadas 2.5D")]
        [SerializeField]
        [Tooltip("Camada do objeto (afeta rendering e profundidade Z)")]
        private string _layerName = "Furniture_Mid";
        
        [SerializeField]
        [Tooltip("Atualizar sorting automaticamente baseado na posição Y")]
        private bool _autoUpdateSorting = true;
        
        private LayerSystem _layerSystem;
        
        /// <summary>
        /// Nome da camada onde o objeto está
        /// </summary>
        public string LayerName => _layerName;
        
        #endregion
        
        #region State
        
        [Header("Estado")]
        [SerializeField]
        private bool _isPlaced = false;
        
        [SerializeField]
        private bool _isDragging = false;
        
        /// <summary>
        /// Verdadeiro se o objeto já foi colocado/posicionado
        /// </summary>
        public bool IsPlaced => _isPlaced;
        
        /// <summary>
        /// Verdadeiro se o objeto está sendo arrastado
        /// </summary>
        public bool IsDragging => _isDragging;
        
        #endregion
        
        #region Position Data
        
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _dragOffset;
        
        #endregion
        
        #region References
        
        private Camera _mainCamera;
        private Level _currentLevel;
        private GridSystem _gridSystem;
        private string _initialLayerName;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _mainCamera = Camera.main;
            _currentLevel = GetComponentInParent<Level>();
            
            if (_currentLevel != null)
            {
                _gridSystem = _currentLevel.GridSystem;
                _layerSystem = _currentLevel.GetComponent<LayerSystem>();
            }
            
            // Salvar posição inicial
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _initialLayerName = _layerName;
            
            // Aplicar camada inicial
            ApplyLayer(_layerName);
        }
        
        private void Start()
        {
            Debug.Log($"[DecorativeObject] '{_objectName}' inicializado na camada '{_layerName}'.");
        }
        
        private void LateUpdate()
        {
            // Atualizar sorting order baseado na posição Y (para objetos na mesma camada)
            if (_autoUpdateSorting && _layerSystem != null && !_isDragging)
            {
                _layerSystem.UpdateObjectSorting(gameObject, _layerName);
            }
        }
        
        #endregion
        
        #region Interaction Methods
        
        /// <summary>
        /// Chamado quando o ponteiro é pressionado sobre o objeto
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            // Verificar se está em modo Playing
            if (Core.GameManager.Instance.CurrentState != Core.GameState.Playing)
            {
                return;
            }
            
            _isDragging = true;
            
            // Calcular offset para drag suave
            Vector3 mousePos = GetMouseWorldPosition();
            _dragOffset = transform.position - mousePos;
            
            Debug.Log($"[DecorativeObject] Começou a arrastar '{_objectName}'");
            
            // Tocar som de pegar objeto
            // Managers.AudioManager.Instance.PlaySFX("pickup");
        }
        
        /// <summary>
        /// Chamado quando o ponteiro é liberado
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            _isDragging = false;
            
            // Fazer snap ao grid se houver GridSystem
            if (_gridSystem != null)
            {
                Vector3 snappedPosition = _gridSystem.SnapToGridWithLayer(transform.position, _layerName);
                transform.position = snappedPosition;
            }
            
            // Atualizar camada (garante Z correto após snap)
            if (_layerSystem != null)
            {
                _layerSystem.SetObjectLayer(gameObject, _layerName);
            }
            
            // Marcar como colocado
            _isPlaced = true;
            
            // Atualizar progresso do nível
            if (_currentLevel != null)
            {
                _currentLevel.UpdateCompletionPercentage();
            }
            
            Debug.Log($"[DecorativeObject] Soltou '{_objectName}' em {transform.position}");
            
            // Tocar som de colocar objeto
            // Managers.AudioManager.Instance.PlaySFX("place");
        }
        
        /// <summary>
        /// Chamado enquanto o objeto está sendo arrastado
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            // Mover o objeto com o mouse/toque
            Vector3 mousePos = GetMouseWorldPosition();
            transform.position = mousePos + _dragOffset;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Converte a posição do mouse em coordenadas do mundo
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = _mainCamera.WorldToScreenPoint(transform.position).z;
            return _mainCamera.ScreenToWorldPoint(mousePos);
        }
        
        /// <summary>
        /// Reseta o objeto para sua posição inicial
        /// </summary>
        public void ResetToInitialPosition()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            _isPlaced = false;
            _isDragging = false;
            
            // Restaurar camada inicial
            ChangeLayer(_initialLayerName);
            
            Debug.Log($"[DecorativeObject] '{_objectName}' resetado para posição inicial.");
        }
        
        /// <summary>
        /// Rotaciona o objeto (pode ser chamado por botões da UI)
        /// </summary>
        public void RotateObject(float angle)
        {
            if (_canRotate && _isPlaced)
            {
                transform.Rotate(Vector3.up, angle);
                
                // Tocar som de rotação
                // Managers.AudioManager.Instance.PlaySFX("rotate");
            }
        }
        
        #endregion
        
        #region Layer Management
        
        /// <summary>
        /// Aplica uma camada ao objeto (configuração inicial)
        /// </summary>
        private void ApplyLayer(string layerName)
        {
            if (_layerSystem != null && _layerSystem.LayerExists(layerName))
            {
                _layerSystem.SetObjectLayer(gameObject, layerName);
            }
            else if (_layerSystem == null)
            {
                Debug.LogWarning($"[DecorativeObject] LayerSystem não encontrado para '{_objectName}'");
            }
        }
        
        /// <summary>
        /// Muda o objeto para uma camada diferente
        /// Útil para objetos que podem ser reposicionados em diferentes profundidades
        /// </summary>
        /// <param name="newLayerName">Nome da nova camada</param>
        public void ChangeLayer(string newLayerName)
        {
            if (_layerSystem == null)
            {
                Debug.LogWarning($"[DecorativeObject] Não é possível mudar camada - LayerSystem não encontrado");
                return;
            }
            
            if (!_layerSystem.LayerExists(newLayerName))
            {
                Debug.LogWarning($"[DecorativeObject] Camada '{newLayerName}' não existe!");
                return;
            }
            
            _layerName = newLayerName;
            _layerSystem.SetObjectLayer(gameObject, _layerName);
            
            Debug.Log($"[DecorativeObject] '{_objectName}' movido para camada '{_layerName}'");
        }
        
        /// <summary>
        /// Retorna informações sobre a camada atual do objeto
        /// </summary>
        public LayerSystem.LayerDefinition GetCurrentLayerInfo()
        {
            if (_layerSystem != null)
            {
                return _layerSystem.GetLayer(_layerName);
            }
            return null;
        }
        
        #endregion
        
        #region Gizmos (Debug Visual)
        
        private void OnDrawGizmos()
        {
            // Desenhar wireframe quando selecionado
            if (_isDragging)
            {
                Gizmos.color = Color.yellow;
            }
            else if (_isPlaced)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.white;
            }
            
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
        
        #endregion
    }
}

