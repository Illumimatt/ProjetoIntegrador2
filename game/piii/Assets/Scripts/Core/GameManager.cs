using System;
using UnityEngine;

namespace Dekora.Core
{
    /// <summary>
    /// GameManager - Gerenciador central do jogo que implementa o autômato de estados.
    /// Padrão Singleton garantindo única instância e acesso global.
    /// 
    /// Responsabilidades:
    /// - Controlar transições entre estados do jogo
    /// - Orquestrar outros gerenciadores (Level, Save, Audio)
    /// - Manter estado global do jogo
    /// - Validar transições permitidas do autômato
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static GameManager _instance;
        
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region State Machine Variables
        
        [Header("Estado do Autômato")]
        [SerializeField] 
        [Tooltip("Estado atual do jogo")]
        private GameState _currentState = GameState.Initialization;
        
        [SerializeField]
        [Tooltip("Estado anterior (útil para voltar de menus)")]
        private GameState _previousState = GameState.Initialization;
        
        /// <summary>
        /// Estado atual do autômato
        /// </summary>
        public GameState CurrentState => _currentState;
        
        /// <summary>
        /// Estado anterior do autômato
        /// </summary>
        public GameState PreviousState => _previousState;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Evento disparado quando o estado do jogo muda.
        /// Outros sistemas podem se inscrever para reagir a mudanças de estado.
        /// </summary>
        public event Action<GameState, GameState> OnStateChanged;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Garantir que só existe uma instância
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[GameManager] Inicializado com sucesso.");
        }
        
        private void Start()
        {
            // Iniciar o autômato
            TransitionToState(GameState.Initialization);
        }
        
        #endregion
        
        #region State Machine Methods
        
        /// <summary>
        /// Solicita transição para um novo estado.
        /// Valida se a transição é permitida pelo autômato antes de executar.
        /// </summary>
        /// <param name="newState">Estado desejado</param>
        /// <returns>True se a transição foi bem-sucedida</returns>
        public bool TransitionToState(GameState newState)
        {
            // Não fazer transição para o mesmo estado
            if (_currentState == newState)
            {
                Debug.LogWarning($"[GameManager] Já está no estado {newState}");
                return false;
            }
            
            // Validar se a transição é permitida
            if (!IsTransitionValid(_currentState, newState))
            {
                Debug.LogError($"[GameManager] Transição inválida: {_currentState} -> {newState}");
                return false;
            }
            
            // Executar transição
            GameState oldState = _currentState;
            
            // Exit do estado atual
            ExitState(_currentState);
            
            // Atualizar estados
            _previousState = _currentState;
            _currentState = newState;
            
            // Enter do novo estado
            EnterState(newState);
            
            // Notificar listeners
            OnStateChanged?.Invoke(oldState, newState);
            
            Debug.Log($"[GameManager] Transição: {oldState} -> {newState}");
            
            return true;
        }
        
        /// <summary>
        /// Valida se uma transição entre estados é permitida pelo autômato.
        /// Define as regras de transição válidas.
        /// </summary>
        private bool IsTransitionValid(GameState from, GameState to)
        {
            // Matriz de transições válidas do autômato
            switch (from)
            {
                case GameState.Initialization:
                    return to == GameState.MainMenu;
                    
                case GameState.MainMenu:
                    return to == GameState.LevelSelection 
                        || to == GameState.Settings 
                        || to == GameState.Exiting;
                    
                case GameState.LevelSelection:
                    return to == GameState.LoadingLevel 
                        || to == GameState.MainMenu;
                    
                case GameState.LoadingLevel:
                    return to == GameState.Playing;
                    
                case GameState.Playing:
                    return to == GameState.Paused 
                        || to == GameState.LevelReview 
                        || to == GameState.MainMenu;
                    
                case GameState.Paused:
                    return to == GameState.Playing 
                        || to == GameState.Settings 
                        || to == GameState.MainMenu;
                    
                case GameState.LevelReview:
                    return to == GameState.Playing 
                        || to == GameState.LevelComplete;
                    
                case GameState.LevelComplete:
                    return to == GameState.LevelSelection 
                        || to == GameState.MainMenu;
                    
                case GameState.Settings:
                    return to == _previousState; // Volta para o estado anterior
                    
                case GameState.Exiting:
                    return false; // Estado final, sem transições
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Lógica executada ao ENTRAR em um estado
        /// </summary>
        private void EnterState(GameState state)
        {
            switch (state)
            {
                case GameState.Initialization:
                    OnEnterInitialization();
                    break;
                    
                case GameState.MainMenu:
                    OnEnterMainMenu();
                    break;
                    
                case GameState.LevelSelection:
                    OnEnterLevelSelection();
                    break;
                    
                case GameState.LoadingLevel:
                    OnEnterLoadingLevel();
                    break;
                    
                case GameState.Playing:
                    OnEnterPlaying();
                    break;
                    
                case GameState.Paused:
                    OnEnterPaused();
                    break;
                    
                case GameState.LevelReview:
                    OnEnterLevelReview();
                    break;
                    
                case GameState.LevelComplete:
                    OnEnterLevelComplete();
                    break;
                    
                case GameState.Settings:
                    OnEnterSettings();
                    break;
                    
                case GameState.Exiting:
                    OnEnterExiting();
                    break;
            }
        }
        
        /// <summary>
        /// Lógica executada ao SAIR de um estado
        /// </summary>
        private void ExitState(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    OnExitPlaying();
                    break;
                    
                case GameState.Paused:
                    OnExitPaused();
                    break;
                    
                // Adicionar cleanup específico de outros estados conforme necessário
            }
        }
        
        #endregion
        
        #region State Enter/Exit Handlers
        
        private void OnEnterInitialization()
        {
            Debug.Log("[GameManager] Inicializando recursos do jogo...");
            // Aqui carregamos recursos iniciais, configurações, etc.
            // Após carregar, transita automaticamente para MainMenu
            TransitionToState(GameState.MainMenu);
        }
        
        private void OnEnterMainMenu()
        {
            Debug.Log("[GameManager] Entrando no Menu Principal");
            // UIManager deve mostrar MainMenuScreen
            // AudioManager deve tocar música do menu
        }
        
        private void OnEnterLevelSelection()
        {
            Debug.Log("[GameManager] Entrando na Seleção de Níveis");
            // UIManager mostra tela de seleção de fases
            // LevelManager carrega metadados dos níveis
        }
        
        private void OnEnterLoadingLevel()
        {
            Debug.Log("[GameManager] Carregando nível...");
            // LevelManager carrega a cena do nível
            // Mostra tela de loading
            // Após carregar, transita para Playing
        }
        
        private void OnEnterPlaying()
        {
            Debug.Log("[GameManager] Iniciando gameplay");
            // Ativa controles do jogador
            // Ativa sistema de decoração
            Time.timeScale = 1f; // Garante que o tempo está normal
        }
        
        private void OnExitPlaying()
        {
            Debug.Log("[GameManager] Saindo do gameplay");
            // Desativa controles se necessário
        }
        
        private void OnEnterPaused()
        {
            Debug.Log("[GameManager] Jogo pausado");
            Time.timeScale = 0f; // Para o tempo do jogo
            // UIManager mostra menu de pausa
        }
        
        private void OnExitPaused()
        {
            Debug.Log("[GameManager] Despausando jogo");
            Time.timeScale = 1f; // Restaura o tempo
        }
        
        private void OnEnterLevelReview()
        {
            Debug.Log("[GameManager] Revisando decoração");
            // Modo câmera livre para ver a decoração
            // Desativa controles de decoração
        }
        
        private void OnEnterLevelComplete()
        {
            Debug.Log("[GameManager] Nível completado!");
            // SaveManager salva progresso
            // Mostra estatísticas, conquistas, etc.
            // UIManager mostra tela de conclusão
        }
        
        private void OnEnterSettings()
        {
            Debug.Log("[GameManager] Abrindo configurações");
            // UIManager mostra SettingsScreen
        }
        
        private void OnEnterExiting()
        {
            Debug.Log("[GameManager] Encerrando jogo...");
            // SaveManager salva tudo
            // Cleanup de recursos
            Application.Quit();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Pausa o jogo (atalho para transição comum)
        /// </summary>
        public void PauseGame()
        {
            if (_currentState == GameState.Playing)
            {
                TransitionToState(GameState.Paused);
            }
        }
        
        /// <summary>
        /// Despausa o jogo (atalho para transição comum)
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState == GameState.Paused)
            {
                TransitionToState(GameState.Playing);
            }
        }
        
        /// <summary>
        /// Volta ao menu principal (disponível de vários estados)
        /// </summary>
        public void ReturnToMainMenu()
        {
            TransitionToState(GameState.MainMenu);
        }
        
        /// <summary>
        /// Inicia um nível específico
        /// </summary>
        public void StartLevel(int levelIndex)
        {
            if (_currentState == GameState.LevelSelection)
            {
                // LevelManager.Instance.LoadLevel(levelIndex);
                TransitionToState(GameState.LoadingLevel);
            }
        }
        
        /// <summary>
        /// Completa o nível atual
        /// </summary>
        public void CompleteCurrentLevel()
        {
            if (_currentState == GameState.Playing || _currentState == GameState.LevelReview)
            {
                TransitionToState(GameState.LevelComplete);
            }
        }
        
        /// <summary>
        /// Abre as configurações
        /// </summary>
        public void OpenSettings()
        {
            if (_currentState == GameState.MainMenu || _currentState == GameState.Paused)
            {
                TransitionToState(GameState.Settings);
            }
        }
        
        /// <summary>
        /// Fecha as configurações (volta ao estado anterior)
        /// </summary>
        public void CloseSettings()
        {
            if (_currentState == GameState.Settings)
            {
                TransitionToState(_previousState);
            }
        }
        
        /// <summary>
        /// Sai do jogo
        /// </summary>
        public void QuitGame()
        {
            TransitionToState(GameState.Exiting);
        }
        
        #endregion
    }
}

