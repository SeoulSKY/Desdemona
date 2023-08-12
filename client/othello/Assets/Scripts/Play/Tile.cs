using System;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Play
{
    public class Tile : MonoBehaviour
    {
        [Tooltip("The material that the tile will have when the mouse point enters")]
        [SerializeField] private Material onMouseEnterMaterial;
        
        [Tooltip("The material that the tile will have when the mouse point is pressed")]
        [SerializeField] private Material onMouseDownMaterial;

        [Tooltip("The material that this tile will have when it's activated")]
        [SerializeField] private Material onActivatedMaterial;

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
                GetComponent<MeshRenderer>().materials = _canPlaceDisk ? new[] { onActivatedMaterial } : new Material[] { };
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
            _disk = GetComponentInChildren<Disk>(true);
            _disk.gameObject.SetActive(false);
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
            
            _disk.gameObject.SetActive(true);
            Disk = _disk;
            _disk.Color = color;

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
            _disk.gameObject.SetActive(false);
            Debug.Log($"Cleared the disk at {name}");
        }

        private async void OnMouseUpAsButton()
        {
            GetComponent<MeshRenderer>().materials = new Material[]{};
            OnMouseEnter();
            
            await OnDiskPlaced?.Invoke(this).ToCoroutine();
        }

        private void OnMouseEnter()
        {
            GetComponent<Renderer>().material = CanPlaceDisk ? onActivatedMaterial : onMouseEnterMaterial;

            if (Disk != null || !CanPlaceDisk)
            {
                return;
            }
            
            _disk.gameObject.SetActive(true);
            _disk.Color = Player.Human.Disk();
        }

        private void OnMouseExit()
        {
            GetComponent<Renderer>().materials = CanPlaceDisk ? new []{onActivatedMaterial} : new Material[]{};

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