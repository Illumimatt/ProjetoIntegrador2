using System;

namespace Dekora.Core
{
    /// <summary>
    /// Enumeração que define todos os estados possíveis do jogo.
    /// Esta é a base do autômato que controla o fluxo do Dekora.
    /// </summary>
    [Serializable]
    public enum GameState
    {
        /// <summary>
        /// Estado inicial - carregando recursos do jogo
        /// </summary>
        Initialization,
        
        /// <summary>
        /// Menu principal - jogador ainda não iniciou uma partida
        /// </summary>
        MainMenu,
        
        /// <summary>
        /// Seleção de nível - jogador escolhe qual fase jogar
        /// </summary>
        LevelSelection,
        
        /// <summary>
        /// Carregando nível - transição antes de iniciar gameplay
        /// </summary>
        LoadingLevel,
        
        /// <summary>
        /// Jogando - estado principal onde ocorre a decoração
        /// </summary>
        Playing,
        
        /// <summary>
        /// Pausado - jogo temporariamente suspenso
        /// </summary>
        Paused,
        
        /// <summary>
        /// Revisão do nível - jogador vê o resultado da decoração
        /// </summary>
        LevelReview,
        
        /// <summary>
        /// Nível completo - tela de conclusão da fase
        /// </summary>
        LevelComplete,
        
        /// <summary>
        /// Configurações - ajustes de áudio, gráficos, etc
        /// </summary>
        Settings,
        
        /// <summary>
        /// Saindo do jogo - limpeza e salvamento final
        /// </summary>
        Exiting
    }
}

