using System.Collections;
using UnityEngine;

namespace Dekora.Gameplay
{
    /// <summary>
    /// CameraController - Controla a câmera ortogonal 2.5D do Dekora.
    /// 
    /// Características:
    /// - Projeção ortográfica (sem perspectiva)
    /// - Controles de zoom (scroll do mouse)
    /// - Controles de pan (arrastar com botão do meio)
    /// - Foco suave em objetos
    /// - Limites de movimento configuráveis
    /// 
    /// Responsabilidades:
    /// - Posicionar câmera para visualização 2.5D
    /// - Permitir exploração do ambiente
    /// - Focar em objetos específicos
    /// - Manter câmera dentro dos limites do nível
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        #region Camera Configuration
        
        [Header("Configuração da Câmera")]
        [SerializeField]
        [Tooltip("Tamanho ortográfico inicial")]
        private float _orthographicSize = 5f;
        
        [SerializeField]
        [Tooltip("Distância da câmera em relação aos objetos (Z)")]
        private float _cameraDistance = 10f;
        
        [SerializeField]
        [Tooltip("Posição inicial da câmera")]
        private Vector3 _initialPosition = new Vector3(0, 0, -10);
        
        private Camera _camera;
        
        #endregion
        
        #region Zoom Controls
        
        [Header("Controles de Zoom")]
        [SerializeField]
        [Tooltip("Permitir zoom com scroll do mouse")]
        private bool _allowZoom = true;
        
        [SerializeField]
        [Tooltip("Velocidade do zoom")]
        private float _zoomSpeed = 2f;
        
        [SerializeField]
        [Tooltip("Limites do zoom (min, max)")]
        private Vector2 _zoomRange = new Vector2(3f, 10f);
        
        [SerializeField]
        [Tooltip("Suavização do zoom")]
        private float _zoomSmoothTime = 0.1f;
        
        private float _targetOrthographicSize;
        private float _zoomVelocity;
        
        #endregion
        
        #region Pan Controls
        
        [Header("Controles de Pan (Movimento)")]
        [SerializeField]
        [Tooltip("Permitir movimento da câmera")]
        private bool _allowPan = true;
        
        [SerializeField]
        [Tooltip("Velocidade do pan")]
        private float _panSpeed = 0.5f;
        
        [SerializeField]
        [Tooltip("Usar limites de movimento")]
        private bool _usePanBounds = true;
        
        [SerializeField]
        [Tooltip("Limites de movimento da câmera")]
        private Bounds _panBounds = new Bounds(Vector3.zero, new Vector3(20, 20, 0));
        
        [SerializeField]
        [Tooltip("Botão do mouse para pan (0=esquerdo, 1=direito, 2=meio)")]
        private int _panMouseButton = 2;
        
        private Vector3 _lastMousePosition;
        private bool _isPanning;
        
        #endregion
        
        #region Focus System
        
        [Header("Sistema de Foco")]
        [SerializeField]
        [Tooltip("Duração padrão da animação de foco")]
        private float _defaultFocusDuration = 0.5f;
        
        [SerializeField]
        [Tooltip("Curva de animação do foco")]
        private AnimationCurve _focusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private bool _isFocusing;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            
            // Configurar como ortogonal
            _camera.orthographic = true;
            _camera.orthographicSize = _orthographicSize;
            _targetOrthographicSize = _orthographicSize;
            
            // Posicionar câmera
            transform.position = _initialPosition;
            transform.rotation = Quaternion.identity; // Olhando para frente (eixo Z)
            
            Debug.Log("[CameraController] Câmera ortogonal 2.5D inicializada.");
        }
        
        private void Update()
        {
            // Verificar se está em estado que permite controle
            if (!CanControlCamera())
            {
                return;
            }
            
            HandleZoom();
            HandlePan();
        }
        
        private void LateUpdate()
        {
            // Aplicar zoom suavizado
            if (!_isFocusing)
            {
                _camera.orthographicSize = Mathf.SmoothDamp(
                    _camera.orthographicSize,
                    _targetOrthographicSize,
                    ref _zoomVelocity,
                    _zoomSmoothTime
                );
            }
        }
        
        #endregion
        
        #region Camera Control
        
        /// <summary>
        /// Verifica se a câmera pode ser controlada no estado atual
        /// </summary>
        private bool CanControlCamera()
        {
            // Permitir controle apenas durante gameplay ou review
            Core.GameState currentState = Core.GameManager.Instance.CurrentState;
            
            return currentState == Core.GameState.Playing || 
                   currentState == Core.GameState.LevelReview;
        }
        
        /// <summary>
        /// Controla o zoom da câmera com scroll do mouse
        /// </summary>
        private void HandleZoom()
        {
            if (!_allowZoom || _isFocusing) return;
            
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            
            if (scroll != 0)
            {
                // Aumentar ou diminuir o tamanho ortográfico
                _targetOrthographicSize -= scroll * _zoomSpeed;
                
                // Aplicar limites
                _targetOrthographicSize = Mathf.Clamp(
                    _targetOrthographicSize,
                    _zoomRange.x,
                    _zoomRange.y
                );
                
                Debug.Log($"[CameraController] Zoom: {_targetOrthographicSize:F2}");
            }
        }
        
        /// <summary>
        /// Controla o movimento (pan) da câmera
        /// </summary>
        private void HandlePan()
        {
            if (!_allowPan || _isFocusing) return;
            
            // Iniciar pan
            if (Input.GetMouseButtonDown(_panMouseButton))
            {
                _lastMousePosition = Input.mousePosition;
                _isPanning = true;
            }
            
            // Executar pan
            if (Input.GetMouseButton(_panMouseButton) && _isPanning)
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                
                // Converter delta de screen space para world space
                // Escalar pelo tamanho ortográfico para movimento consistente
                Vector3 move = new Vector3(
                    -delta.x * _panSpeed * _camera.orthographicSize * 0.001f,
                    -delta.y * _panSpeed * _camera.orthographicSize * 0.001f,
                    0
                );
                
                Vector3 newPosition = transform.position + move;
                
                // Aplicar limites se habilitados
                if (_usePanBounds)
                {
                    newPosition = ClampToBounds(newPosition);
                }
                
                transform.position = newPosition;
                _lastMousePosition = Input.mousePosition;
            }
            
            // Finalizar pan
            if (Input.GetMouseButtonUp(_panMouseButton))
            {
                _isPanning = false;
            }
        }
        
        /// <summary>
        /// Limita a posição da câmera aos bounds configurados
        /// </summary>
        private Vector3 ClampToBounds(Vector3 position)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            
            float minX = _panBounds.min.x + halfWidth;
            float maxX = _panBounds.max.x - halfWidth;
            float minY = _panBounds.min.y + halfHeight;
            float maxY = _panBounds.max.y - halfHeight;
            
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
            
            return position;
        }
        
        #endregion
        
        #region Focus System
        
        /// <summary>
        /// Foca a câmera em uma posição específica com animação suave
        /// </summary>
        /// <param name="targetPosition">Posição alvo (apenas X e Y são usados)</param>
        /// <param name="duration">Duração da animação</param>
        /// <param name="targetZoom">Tamanho ortográfico alvo (opcional)</param>
        public void FocusOn(Vector3 targetPosition, float duration = -1, float targetZoom = -1)
        {
            if (duration < 0)
                duration = _defaultFocusDuration;
            
            Vector3 target = new Vector3(
                targetPosition.x,
                targetPosition.y,
                _initialPosition.z // Manter distância Z
            );
            
            // Aplicar limites se habilitados
            if (_usePanBounds)
            {
                target = ClampToBounds(target);
            }
            
            float zoom = targetZoom > 0 ? targetZoom : _targetOrthographicSize;
            
            StartCoroutine(FocusCoroutine(target, zoom, duration));
        }
        
        /// <summary>
        /// Foca a câmera em um GameObject
        /// </summary>
        public void FocusOnObject(GameObject target, float duration = -1, float targetZoom = -1)
        {
            if (target == null)
            {
                Debug.LogWarning("[CameraController] Objeto alvo é nulo!");
                return;
            }
            
            FocusOn(target.transform.position, duration, targetZoom);
        }
        
        /// <summary>
        /// Corrotina que anima o movimento de foco
        /// </summary>
        private IEnumerator FocusCoroutine(Vector3 targetPosition, float targetZoom, float duration)
        {
            _isFocusing = true;
            
            Vector3 startPosition = transform.position;
            float startZoom = _camera.orthographicSize;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Aplicar curva de animação
                float curveValue = _focusCurve.Evaluate(t);
                
                // Interpolar posição
                transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
                
                // Interpolar zoom
                _camera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, curveValue);
                
                yield return null;
            }
            
            // Garantir posição final exata
            transform.position = targetPosition;
            _camera.orthographicSize = targetZoom;
            _targetOrthographicSize = targetZoom;
            
            _isFocusing = false;
            
            Debug.Log($"[CameraController] Foco concluído em {targetPosition}");
        }
        
        /// <summary>
        /// Retorna a câmera para a posição inicial
        /// </summary>
        public void ResetToInitialPosition(float duration = -1)
        {
            FocusOn(_initialPosition, duration, _orthographicSize);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Define o tamanho ortográfico (zoom) da câmera
        /// </summary>
        public void SetOrthographicSize(float size, bool instant = false)
        {
            size = Mathf.Clamp(size, _zoomRange.x, _zoomRange.y);
            
            if (instant)
            {
                _camera.orthographicSize = size;
            }
            
            _targetOrthographicSize = size;
        }
        
        /// <summary>
        /// Define a posição da câmera
        /// </summary>
        public void SetPosition(Vector3 position, bool instant = false)
        {
            position.z = _initialPosition.z; // Manter distância Z
            
            if (_usePanBounds)
            {
                position = ClampToBounds(position);
            }
            
            if (instant)
            {
                transform.position = position;
            }
            else
            {
                FocusOn(position);
            }
        }
        
        /// <summary>
        /// Define os limites de movimento da câmera
        /// </summary>
        public void SetPanBounds(Bounds bounds)
        {
            _panBounds = bounds;
            
            // Ajustar posição se estiver fora dos novos limites
            if (_usePanBounds)
            {
                transform.position = ClampToBounds(transform.position);
            }
        }
        
        /// <summary>
        /// Habilita ou desabilita os controles da câmera
        /// </summary>
        public void SetControlsEnabled(bool zoom, bool pan)
        {
            _allowZoom = zoom;
            _allowPan = pan;
        }
        
        /// <summary>
        /// Converte uma posição de tela para posição do mundo
        /// </summary>
        public Vector3 ScreenToWorldPoint(Vector3 screenPosition)
        {
            return _camera.ScreenToWorldPoint(screenPosition);
        }
        
        /// <summary>
        /// Converte uma posição do mundo para posição de tela
        /// </summary>
        public Vector3 WorldToScreenPoint(Vector3 worldPosition)
        {
            return _camera.WorldToScreenPoint(worldPosition);
        }
        
        #endregion
        
        #region Gizmos (Visualização)
        
        private void OnDrawGizmos()
        {
            if (_usePanBounds)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(_panBounds.center, _panBounds.size);
            }
            
            // Desenhar posição inicial
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_initialPosition, 0.3f);
        }
        
        #endregion
        
        #region Debug
        
        [ContextMenu("Debug: Info da Câmera")]
        private void DebugCameraInfo()
        {
            Debug.Log("=== CAMERA INFO ===");
            Debug.Log($"Position: {transform.position}");
            Debug.Log($"Orthographic Size: {_camera.orthographicSize}");
            Debug.Log($"Target Size: {_targetOrthographicSize}");
            Debug.Log($"Aspect: {_camera.aspect}");
            Debug.Log($"Is Focusing: {_isFocusing}");
            Debug.Log($"Is Panning: {_isPanning}");
            Debug.Log("===================");
        }
        
        #endregion
    }
}

