using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dekora.Managers
{
    /// <summary>
    /// AudioManager - Gerencia toda a reprodução de áudio do jogo (música e SFX).
    /// Padrão Singleton para acesso global.
    /// 
    /// Responsabilidades:
    /// - Tocar músicas de fundo
    /// - Tocar efeitos sonoros
    /// - Controlar volume global e individual
    /// - Gerenciar transições suaves entre músicas
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        private static AudioManager _instance;
        
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();
                    
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Audio Sources
        
        [Header("Audio Sources")]
        [SerializeField]
        [Tooltip("AudioSource para música de fundo")]
        private AudioSource _musicSource;
        
        [SerializeField]
        [Tooltip("AudioSource para efeitos sonoros")]
        private AudioSource _sfxSource;
        
        #endregion
        
        #region Volume Settings
        
        [Header("Volume Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Volume da música")]
        private float _musicVolume = 0.7f;
        
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Volume dos efeitos sonoros")]
        private float _sfxVolume = 0.8f;
        
        /// <summary>
        /// Volume da música (0 a 1)
        /// </summary>
        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (_musicSource != null)
                    _musicSource.volume = _musicVolume;
            }
        }
        
        /// <summary>
        /// Volume dos efeitos sonoros (0 a 1)
        /// </summary>
        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                if (_sfxSource != null)
                    _sfxSource.volume = _sfxVolume;
            }
        }
        
        #endregion
        
        #region Audio Clips Library
        
        [Header("Audio Library")]
        [SerializeField]
        [Tooltip("Dicionário de músicas disponíveis")]
        private List<NamedAudioClip> _musicLibrary = new List<NamedAudioClip>();
        
        [SerializeField]
        [Tooltip("Dicionário de efeitos sonoros disponíveis")]
        private List<NamedAudioClip> _sfxLibrary = new List<NamedAudioClip>();
        
        private Dictionary<string, AudioClip> _musicDict;
        private Dictionary<string, AudioClip> _sfxDict;
        
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
            
            // Criar AudioSources se não existirem
            SetupAudioSources();
            
            // Construir dicionários
            BuildAudioDictionaries();
            
            Debug.Log("[AudioManager] Inicializado.");
        }
        
        private void SetupAudioSources()
        {
            // Music Source
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.volume = _musicVolume;
            }
            
            // SFX Source
            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
                _sfxSource.loop = false;
                _sfxSource.volume = _sfxVolume;
            }
        }
        
        private void BuildAudioDictionaries()
        {
            // Construir dicionário de músicas
            _musicDict = new Dictionary<string, AudioClip>();
            foreach (var namedClip in _musicLibrary)
            {
                if (!string.IsNullOrEmpty(namedClip.name) && namedClip.clip != null)
                {
                    _musicDict[namedClip.name] = namedClip.clip;
                }
            }
            
            // Construir dicionário de SFX
            _sfxDict = new Dictionary<string, AudioClip>();
            foreach (var namedClip in _sfxLibrary)
            {
                if (!string.IsNullOrEmpty(namedClip.name) && namedClip.clip != null)
                {
                    _sfxDict[namedClip.name] = namedClip.clip;
                }
            }
            
            Debug.Log($"[AudioManager] {_musicDict.Count} músicas e {_sfxDict.Count} SFX carregados.");
        }
        
        #endregion
        
        #region Music Methods
        
        /// <summary>
        /// Toca uma música pelo nome
        /// </summary>
        public void PlayMusic(string musicName, bool fadeIn = false, float fadeDuration = 1f)
        {
            if (_musicDict.TryGetValue(musicName, out AudioClip clip))
            {
                if (fadeIn)
                {
                    StartCoroutine(FadeInMusic(clip, fadeDuration));
                }
                else
                {
                    _musicSource.clip = clip;
                    _musicSource.Play();
                }
                
                Debug.Log($"[AudioManager] Tocando música: {musicName}");
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Música '{musicName}' não encontrada!");
            }
        }
        
        /// <summary>
        /// Toca um AudioClip diretamente como música
        /// </summary>
        public void PlayMusic(AudioClip clip, bool fadeIn = false, float fadeDuration = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Clip de música é nulo!");
                return;
            }
            
            if (fadeIn)
            {
                StartCoroutine(FadeInMusic(clip, fadeDuration));
            }
            else
            {
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }
        
        /// <summary>
        /// Para a música atual
        /// </summary>
        public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutMusic(fadeDuration));
            }
            else
            {
                _musicSource.Stop();
            }
        }
        
        /// <summary>
        /// Pausa a música
        /// </summary>
        public void PauseMusic()
        {
            _musicSource.Pause();
        }
        
        /// <summary>
        /// Resume a música pausada
        /// </summary>
        public void ResumeMusic()
        {
            _musicSource.UnPause();
        }
        
        /// <summary>
        /// Fade in da música
        /// </summary>
        private IEnumerator FadeInMusic(AudioClip clip, float duration)
        {
            _musicSource.clip = clip;
            _musicSource.volume = 0f;
            _musicSource.Play();
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(0f, _musicVolume, elapsed / duration);
                yield return null;
            }
            
            _musicSource.volume = _musicVolume;
        }
        
        /// <summary>
        /// Fade out da música
        /// </summary>
        private IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            
            _musicSource.Stop();
            _musicSource.volume = _musicVolume;
        }
        
        #endregion
        
        #region SFX Methods
        
        /// <summary>
        /// Toca um efeito sonoro pelo nome
        /// </summary>
        public void PlaySFX(string sfxName, float volumeScale = 1f)
        {
            if (_sfxDict.TryGetValue(sfxName, out AudioClip clip))
            {
                _sfxSource.PlayOneShot(clip, volumeScale * _sfxVolume);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX '{sfxName}' não encontrado!");
            }
        }
        
        /// <summary>
        /// Toca um AudioClip diretamente como SFX
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Clip de SFX é nulo!");
                return;
            }
            
            _sfxSource.PlayOneShot(clip, volumeScale * _sfxVolume);
        }
        
        #endregion
        
        #region Helper Classes
        
        /// <summary>
        /// Classe auxiliar para associar nomes a AudioClips no Inspector
        /// </summary>
        [Serializable]
        public class NamedAudioClip
        {
            public string name;
            public AudioClip clip;
        }
        
        #endregion
    }
}

