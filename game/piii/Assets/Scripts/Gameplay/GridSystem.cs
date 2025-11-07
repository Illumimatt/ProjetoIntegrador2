using UnityEngine;

namespace Dekora.Gameplay
{
    /// <summary>
    /// GridSystem - Sistema de grade para fazer "snap" de objetos decoráveis.
    /// Proporciona alinhamento perfeito dos objetos no espaço.
    /// 
    /// Responsabilidades:
    /// - Definir tamanho da grade
    /// - Calcular posição mais próxima no grid
    /// - Desenhar visualização da grade (debug)
    /// </summary>
    public class GridSystem : MonoBehaviour
    {
        #region Grid Settings
        
        [Header("Configurações da Grade")]
        [SerializeField]
        [Tooltip("Tamanho de cada célula do grid")]
        private float _gridSize = 1f;
        
        [SerializeField]
        [Tooltip("Usar snap ao grid (pode ser desativado para posicionamento livre)")]
        private bool _useGridSnapping = true;
        
        /// <summary>
        /// Tamanho de cada célula do grid
        /// </summary>
        public float GridSize
        {
            get => _gridSize;
            set => _gridSize = Mathf.Max(0.1f, value);
        }
        
        /// <summary>
        /// Verdadeiro se o snapping está ativo
        /// </summary>
        public bool UseGridSnapping
        {
            get => _useGridSnapping;
            set => _useGridSnapping = value;
        }
        
        #endregion
        
        #region Visualization
        
        [Header("Visualização (Debug)")]
        [SerializeField]
        [Tooltip("Mostrar grade na Scene View")]
        private bool _showGrid = true;
        
        [SerializeField]
        [Tooltip("Tamanho da área da grade")]
        private Vector2 _gridArea = new Vector2(10, 10);
        
        [SerializeField]
        [Tooltip("Cor da grade")]
        private Color _gridColor = new Color(0, 1, 0, 0.3f);
        
        #endregion
        
        #region Snapping Methods
        
        /// <summary>
        /// Retorna a posição mais próxima no grid
        /// </summary>
        /// <param name="position">Posição original</param>
        /// <returns>Posição ajustada ao grid</returns>
        public Vector3 SnapToGrid(Vector3 position)
        {
            if (!_useGridSnapping)
            {
                return position;
            }
            
            // Calcular a posição "snapped"
            float snappedX = Mathf.Round(position.x / _gridSize) * _gridSize;
            float snappedY = Mathf.Round(position.y / _gridSize) * _gridSize;
            float snappedZ = Mathf.Round(position.z / _gridSize) * _gridSize;
            
            return new Vector3(snappedX, snappedY, snappedZ);
        }
        
        /// <summary>
        /// Retorna a posição mais próxima no grid (apenas X e Z, mantém Y)
        /// </summary>
        /// <param name="position">Posição original</param>
        /// <returns>Posição ajustada ao grid no plano horizontal</returns>
        public Vector3 SnapToGridXZ(Vector3 position)
        {
            if (!_useGridSnapping)
            {
                return position;
            }
            
            float snappedX = Mathf.Round(position.x / _gridSize) * _gridSize;
            float snappedZ = Mathf.Round(position.z / _gridSize) * _gridSize;
            
            return new Vector3(snappedX, position.y, snappedZ);
        }
        
        /// <summary>
        /// Retorna a posição mais próxima no grid com profundidade Z baseada na camada
        /// (Para jogos 2.5D com sistema de camadas)
        /// </summary>
        /// <param name="position">Posição original</param>
        /// <param name="layerName">Nome da camada (determina Z)</param>
        /// <returns>Posição ajustada ao grid com Z da camada</returns>
        public Vector3 SnapToGridWithLayer(Vector3 position, string layerName)
        {
            // Snap X e Y ao grid
            Vector3 snapped = SnapToGridXZ(position);
            
            // Obter profundidade Z da camada
            LayerSystem layerSystem = GetComponent<LayerSystem>();
            if (layerSystem != null && layerSystem.LayerExists(layerName))
            {
                snapped.z = layerSystem.GetLayerDepth(layerName);
            }
            else
            {
                // Se LayerSystem não existir, manter Z original
                snapped.z = position.z;
            }
            
            return snapped;
        }
        
        /// <summary>
        /// Verifica se uma posição está no grid
        /// </summary>
        /// <param name="position">Posição a verificar</param>
        /// <returns>True se está perfeitamente alinhada ao grid</returns>
        public bool IsOnGrid(Vector3 position)
        {
            float remainderX = position.x % _gridSize;
            float remainderZ = position.z % _gridSize;
            
            return Mathf.Approximately(remainderX, 0f) && Mathf.Approximately(remainderZ, 0f);
        }
        
        /// <summary>
        /// Verifica se uma posição está no grid E na camada correta
        /// </summary>
        /// <param name="position">Posição a verificar</param>
        /// <param name="layerName">Nome da camada esperada</param>
        /// <returns>True se está no grid e na profundidade Z correta</returns>
        public bool IsOnGridAndLayer(Vector3 position, string layerName)
        {
            if (!IsOnGrid(position))
                return false;
            
            LayerSystem layerSystem = GetComponent<LayerSystem>();
            if (layerSystem != null && layerSystem.LayerExists(layerName))
            {
                float expectedZ = layerSystem.GetLayerDepth(layerName);
                return Mathf.Approximately(position.z, expectedZ);
            }
            
            return true; // Se não tem LayerSystem, considerar OK
        }
        
        #endregion
        
        #region Gizmos (Visualização)
        
        private void OnDrawGizmos()
        {
            if (!_showGrid) return;
            
            Gizmos.color = _gridColor;
            
            // Desenhar linhas horizontais (eixo X)
            for (float z = -_gridArea.y / 2; z <= _gridArea.y / 2; z += _gridSize)
            {
                Vector3 start = transform.position + new Vector3(-_gridArea.x / 2, 0, z);
                Vector3 end = transform.position + new Vector3(_gridArea.x / 2, 0, z);
                Gizmos.DrawLine(start, end);
            }
            
            // Desenhar linhas verticais (eixo Z)
            for (float x = -_gridArea.x / 2; x <= _gridArea.x / 2; x += _gridSize)
            {
                Vector3 start = transform.position + new Vector3(x, 0, -_gridArea.y / 2);
                Vector3 end = transform.position + new Vector3(x, 0, _gridArea.y / 2);
                Gizmos.DrawLine(start, end);
            }
        }
        
        #endregion
    }
}

