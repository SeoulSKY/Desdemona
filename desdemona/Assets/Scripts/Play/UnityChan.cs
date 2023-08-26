using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Play
{
    public class UnityChan : MonoBehaviour
    {
        /// <summary>
        /// How long does it take for Unity-Chan to get boring in seconds
        /// </summary>
        [SerializeField] private float boringInterval = 10f;

        /// <summary>
        /// The audios to play in random when Unity-Chan is greeting
        /// </summary>
        [SerializeField] private AudioClip[] greetingAudios;

        /// <summary>
        /// The audios to play in random when Unity-Chan is thinking
        /// </summary>
        [SerializeField] private AudioClip[] thinkingAudios;

        /// <summary>
        /// The audios to play in the order of boring actions when Unity-Chan is boring
        /// </summary>
        [SerializeField] private AudioClip[] boringAudios;

        /// <summary>
        /// The audios to play in random when Unity-Chan is spawning a disk
        /// </summary>
        [SerializeField] private AudioClip[] spawningAudio;
        
        /// <summary>
        /// The audios to play in random when Unity-Chan won the game
        /// </summary>
        [SerializeField] private AudioClip[] wonAudios;
        
        /// <summary>
        /// The audios to play in random when Unity-Chan lost the game
        /// </summary>
        [SerializeField] private AudioClip[] lostAudios;
        
        /// <summary>
        /// The audios to play in random when draw the game
        /// </summary>
        [SerializeField] private AudioClip[] drawAudios;

        private AudioSource _audioSource;
        private Animator _animator;
        private int _thinkingHash;
        private int _wonHash;
        private int _drawHash;
        private int _lostHash;
        private int _boringHash;
        private int _boringIndexHash;
        private int _idleStateHash;
        private int[] _boringStateHashes;

        private Board _board;
        private bool _isGameOver;
        
        [CanBeNull]
        private Coroutine _idleCoroutine;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _animator = GetComponentInChildren<Animator>();
            _thinkingHash = Animator.StringToHash("thinking");
            _wonHash = Animator.StringToHash("won");
            _drawHash = Animator.StringToHash("draw");
            _lostHash = Animator.StringToHash("lost");
            _boringHash = Animator.StringToHash("boring");
            _boringIndexHash = Animator.StringToHash("boringIndex");
            _idleStateHash = Animator.StringToHash("Idle");
            _boringStateHashes = new[]
            {
                Animator.StringToHash("Boring"),
                Animator.StringToHash("Boring 1"),
            };

            _board = FindObjectOfType<Board>();
            
            _board.OnThinking += OnThinking;
            _board.OnDecided += OnDecided;
            _board.OnGameOver += OnGameOver;
        }

        private void Start()
        {
            var choice = Random.Range(0, greetingAudios.Length);
            _audioSource.PlayOneShot(greetingAudios[choice]);
        }

        private void Update()
        {
            if (IsIdle() && _idleCoroutine == null)
            {
                _idleCoroutine = StartCoroutine(OnIdle().ToCoroutine());
            }
        }

        private bool IsIdle()
        {
            return _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == _idleStateHash;
        }

        private async UniTask OnIdle()
        {
            await UniTask.WaitForSeconds(boringInterval);
            if (!IsIdle())
            {
                _idleCoroutine = null;
                return;
            }
            
            var choice = Random.Range(0, boringAudios.Length);

            _animator.SetInteger(_boringIndexHash, choice);
            _animator.SetTrigger(_boringHash);
                
            await UniTask.WaitUntil(() => _boringStateHashes.Contains(_animator.GetCurrentAnimatorStateInfo(0).shortNameHash));

            _audioSource.Stop();
            _audioSource.PlayOneShot(boringAudios[choice]);
            _idleCoroutine = null;
        }

        private async UniTask OnThinking()
        {
            _animator.SetBool(_thinkingHash, true);
            await UniTask.WaitUntil(() => _animator.IsInTransition(0));

            _audioSource.Stop();
            var choice = Random.Range(0, thinkingAudios.Length);
            _audioSource.PlayOneShot(thinkingAudios[choice]);
        }

        private async UniTask OnDecided(Tile tile)
        {
            _animator.SetBool(_thinkingHash, false);
            
            // Wait for playing spawn animation
            await UniTask.WaitUntil(() => _animator.IsInTransition(0));
            await UniTask.WaitForSeconds(0.35f);
            
            _audioSource.Stop();
            var choice = Random.Range(0, spawningAudio.Length);
            _audioSource.PlayOneShot(spawningAudio[choice]);
        }

        private UniTask OnGameOver(Player? winner)
        {
            _audioSource.Stop();
            _isGameOver = true;
            switch (winner)
            {
                case Player.Bot:
                {
                    var choice = Random.Range(0, wonAudios.Length);
;                    _audioSource.PlayOneShot(wonAudios[choice]);
                    _animator.SetTrigger(_wonHash);
                    break;
                }
                case Player.Human:
                {
                    var choice = Random.Range(0, lostAudios.Length);
                    _audioSource.PlayOneShot(lostAudios[choice]);
                    _animator.SetTrigger(_lostHash);
                    break;
                }
                default:
                {
                    var choice = Random.Range(0, drawAudios.Length);
                    _audioSource.PlayOneShot(drawAudios[choice]);
                    _animator.SetTrigger(_drawHash);
                    break;
                }
            }
            return UniTask.CompletedTask;
        }
    }
}