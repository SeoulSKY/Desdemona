using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Play
{
    public enum DiskColor
    {
        Dark,
        Light,
    }
    
    public static class DiskColorMethods
    {
        private const char DarkDiskChar = 'D';
        private const char LightDiskChar = 'L';

        /// <summary>
        /// Check if the given character can be parsed into disk color
        /// </summary>
        /// <param name="ch">The character to check</param>
        /// <returns>true if it can, false otherwise</returns>
        public static bool CanParse(char ch)
        {
            return ch is DarkDiskChar or LightDiskChar;
        }
        
        /// <summary>
        /// Parse the given character into a Disk.Color
        /// </summary>
        /// <param name="ch">The character to parse</param>
        /// <returns>The parsed Color</returns>
        /// <exception cref="ArgumentException">If the given character cannot be parsed</exception>
        public static DiskColor Parse(char ch)
        {
            return ch switch
            {
                DarkDiskChar => DiskColor.Dark,
                LightDiskChar => DiskColor.Light,
                var _ => throw new ArgumentException($"Given character cannot be parsed into Color: {ch}"),
            };
        }

        /// <summary>
        /// Convert the color to a char
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <returns>The converted char</returns>
        public static char ToChar(this DiskColor color)
        {
            return color switch
            {
                DiskColor.Dark => DarkDiskChar,
                var _ => LightDiskChar,
            };
        }

        /// <summary>
        /// Returns the opposite color of this color
        /// </summary>
        /// <param name="color">The original color</param>
        /// <returns>The opposite color</returns>
        public static DiskColor Opposite(this DiskColor color)
        {
            return color switch
            {
                DiskColor.Dark => DiskColor.Light,
                var _ => DiskColor.Dark,
            };
        }
    }
    
    public class Disk : MonoBehaviour
    {
        /// <summary>
        /// The relative height to spawn the disk from current position
        /// </summary>
        [SerializeField] private float spawnHeight;

        /// <summary>
        /// The audio clip to play when the disk spawns
        /// </summary>
        [SerializeField] private AudioClip spawnSound;

        /// <summary>
        /// The audio clip to play when the disk flips
        /// </summary>
        [SerializeField] private AudioClip flipSound;
        
        /// <summary>
        /// The audio clip to play when the disk drops
        /// </summary>
        [SerializeField] private AudioClip dropSound;

        private Rigidbody _rigidbody;
        private AudioSource _audioSource;

        private bool _isReady;
        
        public bool IsSpawning { get; private set; }
        public bool IsFlipping { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _audioSource = GetComponent<AudioSource>();
            
            _isReady = true;
        }

        private DiskColor _color = DiskColor.Dark;
        
        public DiskColor Color
        {
            get
            {
                return _color;
            }

            set
            {
                if (!_isReady)
                {
                    Awake();
                }

                if (_color == value)
                {
                    return;
                }
                
                transform.Rotate(new Vector3(180, 0, 0));
                _color = value;
            }
        }

        /// <summary>
        /// Spawn a disk with the given color
        /// </summary>
        /// <param name="color">The color to spawn</param>
        public void Spawn(DiskColor color)
        {
            IsSpawning = true;
            Color = color;
            
            gameObject.SetActive(true);
            _audioSource.PlayOneShot(spawnSound);
            transform.localPosition += new Vector3(0, spawnHeight, 0);
        }
        
        /// <summary>
        /// Flip this disk so that it shows the opposite color
        /// </summary>
        public void Flip()
        {
            IsFlipping = true;
            _color = Color.Opposite();
            _audioSource.PlayOneShot(flipSound);
            
            _rigidbody.AddForce(Vector3.up * 5, ForceMode.Impulse);
            _rigidbody.AddTorque(transform.right * 0.05f, ForceMode.Impulse);
        }
        
        /// <summary>
        /// Wait while this disk is flipping
        /// </summary>
        public async UniTask WaitWhileSpawning()
        {
            if (!IsSpawning)
            {
                throw new InvalidOperationException("This disk is not spawning currently");
            }
            await UniTask.WaitWhile(() => IsSpawning);
        }

        /// <summary>
        /// Wait while this disk is flipping
        /// </summary>
        public async UniTask WaitWhileFlipping()
        {
            if (!IsFlipping)
            {
                throw new InvalidOperationException("This disk is not flipping currently");
            }
            await UniTask.WaitWhile(() => IsFlipping);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.CompareTag("Board"))
            {
                return;
            }
            if (!IsFlipping && !IsSpawning)
            {
                return;
            }

            IsSpawning = false;
            IsFlipping = false;
            _audioSource.PlayOneShot(dropSound);
        }
    }
}