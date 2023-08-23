using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Play
{
    public class Tile : MonoBehaviour
    {
        [Tooltip("The material that the tile will have when the mouse point enters")]
        [SerializeField] private Material onMouseEnterMaterial;
        
        [Tooltip("The material that the tile will have when the mouse point is pressed")]
        [SerializeField] private Material onMouseDownMaterial;

        private ParticleSystem _spawnHint;
        
        private bool _canPlaceDisk;
        public bool CanPlaceDisk
        {
            get
            {
                return _canPlaceDisk;
            }
            set
            {
                _canPlaceDisk = value;
                _spawnHint.gameObject.SetActive(_canPlaceDisk);
                
                if (_canPlaceDisk)
                {
                    _spawnHint.Play();
                }
                else
                {
                    _spawnHint.Stop();
                }
            }
        }

        /// <summary>
        /// A character that represents the empty tile
        /// </summary>
        private const char EmptyChar = 'E';
        
        private Disk _disk;
        
        [CanBeNull]
        public Disk Disk { get; private set; }

        public delegate UniTask DiskPlaced(Tile tile);
        public event DiskPlaced OnDiskPlaced;

        private void Awake()
        {
            _spawnHint = GetComponentInChildren<ParticleSystem>(true);
            _disk = GetComponentInChildren<Disk>(true);
            _disk.gameObject.SetActive(false);
            _disk.gameObject.GetComponent<Rigidbody>().useGravity = false;
            _disk.gameObject.GetComponent<Collider>().enabled = false;
        }
        
        /// <summary>
        /// Place a disk on this tile
        /// </summary>
        /// <param name="color">The color of the disk to place</param>
        /// <exception cref="InvalidOperationException">If Disk != null</exception>
        public void PlaceDisk(DiskColor color)
        {
            if (Disk != null)
            {
                throw new InvalidOperationException($"Tile {name} is occupied by another disk");
            }
            
            _disk.gameObject.GetComponent<Rigidbody>().useGravity = true;
            _disk.gameObject.GetComponent<Collider>().enabled = true;
            _disk.Spawn(color);

            Disk = _disk;

            Debug.Log($"Placed {color} at {name}");
        }

        /// <summary>
        /// Clear the disk on this tile
        /// </summary>
        /// <exception cref="InvalidOperationException">If Disk == null</exception>
        public void ClearDisk()
        {
            if (Disk == null)
            {
                throw new InvalidOperationException($"There is no disk on tile {name} to clear");
            }

            Disk = null;
            _disk.gameObject.GetComponent<Rigidbody>().useGravity = false;
            _disk.gameObject.SetActive(false);
            Debug.Log($"Cleared the disk at {name}");
        }

        private async void OnMouseUpAsButton()
        {
            await OnDiskPlaced?.Invoke(this).ToCoroutine();
        }

        private void OnMouseEnter()
        {
            GetComponent<Renderer>().material = onMouseEnterMaterial;

            if (Disk != null || !CanPlaceDisk)
            {
                return;
            }
            
            _disk.Color = Player.Human.Disk();
            _disk.gameObject.SetActive(true);
        }

        private void OnMouseExit()
        {
            GetComponent<Renderer>().materials = new Material[]{};

            if (Disk != null || !CanPlaceDisk)
            {
                return;
            }
            
            _disk.gameObject.SetActive(false);
        }

        private void OnMouseDown()
        {
            GetComponent<Renderer>().material = onMouseDownMaterial;
        }

        private void OnMouseUp()
        {
            OnMouseEnter();
        }

        /// <summary>
        /// Convert this object to a string representation
        /// </summary>
        /// <returns>The converted string</returns>
        public new string ToString()
        {
            return Disk != null ? Disk.Color.ToChar().ToString() : EmptyChar.ToString();
        }
    }
}