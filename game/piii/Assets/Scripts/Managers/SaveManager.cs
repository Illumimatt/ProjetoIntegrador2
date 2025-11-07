using System;
using System.IO;
using UnityEngine;

namespace Dekora.Managers
{
    /// <summary>
    /// SaveManager - Gerencia salvamento e carregamento do progresso do jogador.
    /// 
    /// Responsabilidades:
    /// - Salvar progresso do jogo (níveis completados, decorações, etc.)
    /// - Carregar progresso salvo
    /// - Salvar configurações do jogador
    /// - Gerenciar múltiplos slots de save (se necessário)
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static SaveManager _instance;
        
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SaveManager");
                        _instance = go.AddComponent<SaveManager>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Variables
        
        [Header("Configurações de Save")]
        [SerializeField]
        [Tooltip("Nome do arquivo de save")]
        private string _saveFileName = "dekora_save.json";
        
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, _saveFileName);
        
        private GameSaveData _currentSaveData;
        
        /// <summary>
        /// Dados de save atualmente carregados
        /// </summary>
        public GameSaveData CurrentSaveData => _currentSaveData;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Evento disparado quando o jogo é salvo
        /// </summary>
        public event Action OnGameSaved;
        
        /// <summary>
        /// Evento disparado quando o save é carregado
        /// </summary>
        public event Action<GameSaveData> OnGameLoaded;
        
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
            
            Debug.Log("[SaveManager] Inicializado.");
            
            // Carregar save existente ou criar novo
            LoadGame();
        }
        
        #endregion
        
        #region Save/Load Methods
        
        /// <summary>
        /// Salva o progresso atual do jogo
        /// </summary>
        public void SaveGame()
        {
            try
            {
                // Atualizar timestamp
                _currentSaveData.lastSaveTime = DateTime.Now.ToString();
                
                // Serializar para JSON
                string json = JsonUtility.ToJson(_currentSaveData, true);
                
                // Escrever no arquivo
                File.WriteAllText(SaveFilePath, json);
                
                Debug.Log($"[SaveManager] Jogo salvo em: {SaveFilePath}");
                
                // Notificar
                OnGameSaved?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erro ao salvar jogo: {e.Message}");
            }
        }
        
        /// <summary>
        /// Carrega o progresso salvo do jogo
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    // Ler arquivo
                    string json = File.ReadAllText(SaveFilePath);
                    
                    // Desserializar
                    _currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                    
                    Debug.Log($"[SaveManager] Jogo carregado de: {SaveFilePath}");
                    Debug.Log($"[SaveManager] Último save: {_currentSaveData.lastSaveTime}");
                    
                    // Notificar
                    OnGameLoaded?.Invoke(_currentSaveData);
                }
                else
                {
                    // Criar novo save
                    Debug.Log("[SaveManager] Nenhum save encontrado. Criando novo.");
                    _currentSaveData = new GameSaveData();
                    SaveGame();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erro ao carregar jogo: {e.Message}");
                
                // Em caso de erro, criar save novo
                _currentSaveData = new GameSaveData();
            }
        }
        
        /// <summary>
        /// Reseta o save (apaga progresso)
        /// </summary>
        public void ResetSave()
        {
            Debug.Log("[SaveManager] Resetando save...");
            
            _currentSaveData = new GameSaveData();
            SaveGame();
        }
        
        /// <summary>
        /// Deleta o arquivo de save
        /// </summary>
        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("[SaveManager] Arquivo de save deletado.");
                }
                
                _currentSaveData = new GameSaveData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Erro ao deletar save: {e.Message}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Marca um nível como completado
        /// </summary>
        public void CompleteLevel(int levelIndex)
        {
            if (!_currentSaveData.completedLevels.Contains(levelIndex))
            {
                _currentSaveData.completedLevels.Add(levelIndex);
                _currentSaveData.highestLevelReached = Mathf.Max(_currentSaveData.highestLevelReached, levelIndex + 1);
                SaveGame();
                
                Debug.Log($"[SaveManager] Nível {levelIndex} marcado como completo.");
            }
        }
        
        /// <summary>
        /// Verifica se um nível foi completado
        /// </summary>
        public bool IsLevelCompleted(int levelIndex)
        {
            return _currentSaveData.completedLevels.Contains(levelIndex);
        }
        
        /// <summary>
        /// Retorna o maior nível alcançado pelo jogador
        /// </summary>
        public int GetHighestLevelReached()
        {
            return _currentSaveData.highestLevelReached;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Estrutura de dados para salvar o progresso do jogo.
    /// Serializada em JSON e salva em disco.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// Timestamp do último save
        /// </summary>
        public string lastSaveTime;
        
        /// <summary>
        /// Lista de índices de níveis completados
        /// </summary>
        public System.Collections.Generic.List<int> completedLevels = new System.Collections.Generic.List<int>();
        
        /// <summary>
        /// Maior nível alcançado (para unlock progressivo)
        /// </summary>
        public int highestLevelReached = 0;
        
        /// <summary>
        /// Volume da música (0 a 1)
        /// </summary>
        public float musicVolume = 0.7f;
        
        /// <summary>
        /// Volume dos efeitos sonoros (0 a 1)
        /// </summary>
        public float sfxVolume = 0.8f;
        
        /// <summary>
        /// Tempo total de jogo em segundos
        /// </summary>
        public float totalPlayTime = 0f;
    }
}

