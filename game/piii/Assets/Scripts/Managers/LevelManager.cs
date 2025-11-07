using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dekora.Managers
{
    /// <summary>
    /// LevelManager - Gerencia o carregamento, descarregamento e controle de níveis/fases.
    /// 
    /// Responsabilidades:
    /// - Carregar níveis (cenas) de forma assíncrona
    /// - Descarregar níveis quando não estão em uso
    /// - Manter informações sobre nível atual
    /// - Rastrear progresso do jogador nas fases
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static LevelManager _instance;
        
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LevelManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LevelManager");
                        _instance = go.AddComponent<LevelManager>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Variables
        
        [Header("Nível Atual")]
        [SerializeField]
        private int _currentLevelIndex = -1;
        
        [SerializeField]
        private string _currentLevelName = "";
        
        /// <summary>
        /// Índice do nível atualmente carregado (-1 se nenhum)
        /// </summary>
        public int CurrentLevelIndex => _currentLevelIndex;
        
        /// <summary>
        /// Nome do nível atual
        /// </summary>
        public string CurrentLevelName => _currentLevelName;
        
        /// <summary>
        /// Verdadeiro se há um nível carregado
        /// </summary>
        public bool IsLevelLoaded => _currentLevelIndex >= 0;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Evento disparado quando um nível começa a carregar
        /// </summary>
        public event Action<int> OnLevelLoadStarted;
        
        /// <summary>
        /// Evento disparado quando um nível termina de carregar
        /// </summary>
        public event Action<int> OnLevelLoadCompleted;
        
        /// <summary>
        /// Evento disparado durante o carregamento (progresso)
        /// </summary>
        public event Action<float> OnLevelLoadProgress;
        
        /// <summary>
        /// Evento disparado quando um nível é descarregado
        /// </summary>
        public event Action<int> OnLevelUnloaded;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[LevelManager] Inicializado.");
        }
        
        #endregion
        
        #region Level Loading Methods
        
        /// <summary>
        /// Carrega um nível por índice de forma assíncrona
        /// </summary>
        /// <param name="levelIndex">Índice do nível a carregar</param>
        public void LoadLevel(int levelIndex)
        {
            StartCoroutine(LoadLevelAsync(levelIndex));
        }
        
        /// <summary>
        /// Carrega um nível pelo nome da cena
        /// </summary>
        /// <param name="levelName">Nome da cena a carregar</param>
        public void LoadLevel(string levelName)
        {
            StartCoroutine(LoadLevelAsync(levelName));
        }
        
        /// <summary>
        /// Corrotina que carrega um nível de forma assíncrona
        /// </summary>
        private IEnumerator LoadLevelAsync(int levelIndex)
        {
            Debug.Log($"[LevelManager] Iniciando carregamento do nível {levelIndex}");
            
            // Notificar início do carregamento
            OnLevelLoadStarted?.Invoke(levelIndex);
            
            // Descarregar nível anterior se houver
            if (IsLevelLoaded)
            {
                yield return UnloadCurrentLevel();
            }
            
            // Carregar nova cena de forma assíncrona
            // Nota: Aqui você deve usar o nome real da cena ou build index
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelIndex);
            asyncLoad.allowSceneActivation = false;
            
            // Reportar progresso
            while (asyncLoad.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnLevelLoadProgress?.Invoke(progress);
                yield return null;
            }
            
            // Ativar a cena quando pronto
            asyncLoad.allowSceneActivation = true;
            
            // Aguardar conclusão
            yield return asyncLoad;
            
            // Atualizar estado
            _currentLevelIndex = levelIndex;
            _currentLevelName = SceneManager.GetActiveScene().name;
            
            Debug.Log($"[LevelManager] Nível {levelIndex} carregado: {_currentLevelName}");
            
            // Notificar conclusão
            OnLevelLoadCompleted?.Invoke(levelIndex);
        }
        
        /// <summary>
        /// Corrotina que carrega um nível pelo nome
        /// </summary>
        private IEnumerator LoadLevelAsync(string levelName)
        {
            Debug.Log($"[LevelManager] Iniciando carregamento do nível '{levelName}'");
            
            // Notificar início do carregamento
            OnLevelLoadStarted?.Invoke(-1);
            
            // Descarregar nível anterior se houver
            if (IsLevelLoaded)
            {
                yield return UnloadCurrentLevel();
            }
            
            // Carregar nova cena
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
            asyncLoad.allowSceneActivation = false;
            
            // Reportar progresso
            while (asyncLoad.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnLevelLoadProgress?.Invoke(progress);
                yield return null;
            }
            
            // Ativar a cena
            asyncLoad.allowSceneActivation = true;
            yield return asyncLoad;
            
            // Atualizar estado
            _currentLevelName = levelName;
            _currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
            
            Debug.Log($"[LevelManager] Nível '{levelName}' carregado");
            
            // Notificar conclusão
            OnLevelLoadCompleted?.Invoke(_currentLevelIndex);
        }
        
        /// <summary>
        /// Descarrega o nível atual
        /// </summary>
        public IEnumerator UnloadCurrentLevel()
        {
            if (!IsLevelLoaded)
            {
                Debug.LogWarning("[LevelManager] Nenhum nível para descarregar");
                yield break;
            }
            
            Debug.Log($"[LevelManager] Descarregando nível {_currentLevelIndex}");
            
            int levelToUnload = _currentLevelIndex;
            
            // Descarregar a cena
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(_currentLevelIndex);
            yield return asyncUnload;
            
            // Limpar estado
            _currentLevelIndex = -1;
            _currentLevelName = "";
            
            // Notificar
            OnLevelUnloaded?.Invoke(levelToUnload);
            
            Debug.Log($"[LevelManager] Nível {levelToUnload} descarregado");
        }
        
        /// <summary>
        /// Recarrega o nível atual (útil para reiniciar)
        /// </summary>
        public void ReloadCurrentLevel()
        {
            if (!IsLevelLoaded)
            {
                Debug.LogWarning("[LevelManager] Nenhum nível para recarregar");
                return;
            }
            
            LoadLevel(_currentLevelIndex);
        }
        
        #endregion
        
        #region Level Information
        
        /// <summary>
        /// Retorna o número total de níveis disponíveis
        /// </summary>
        public int GetTotalLevels()
        {
            // Retorna o número de cenas no Build Settings
            return SceneManager.sceneCountInBuildSettings;
        }
        
        /// <summary>
        /// Verifica se um nível está disponível
        /// </summary>
        public bool IsLevelAvailable(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < GetTotalLevels();
        }
        
        #endregion
    }
}

