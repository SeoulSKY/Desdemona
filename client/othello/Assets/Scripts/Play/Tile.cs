using System;
using System.Collections;
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


        /// <summary>
        /// A character that represents the empty tile
        /// </summary>
        private const char EmptyChar = 'E';

        private Disk _disk;

        [CanBeNull]
        public Disk Disk => _disk.gameObject.activeSelf ? _disk : null;
        
        public delegate IEnumerator DiskPlaced(Tile tile);
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
                throw new InvalidOperationException("This tile is occupied by another disk");
            }
            
            _disk.gameObject.SetActive(true);
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
                throw new InvalidOperationException("There is no disk on this tile to clear");
            }

            _disk.gameObject.SetActive(false);
            
            Debug.Log($"Cleared the disk at {name}");
        }

        private IEnumerator OnMouseUpAsButton()
        {
            GetComponent<MeshRenderer>().materials = new Material[]{};
            OnMouseEnter();
            
            yield return OnDiskPlaced?.Invoke(this);
        }

        private void OnMouseEnter()
        {
            GetComponent<MeshRenderer>().material = onMouseEnterMaterial;
        }

        private void OnMouseExit()
        {
            GetComponent<MeshRenderer>().materials = new Material[]{};
        }

        private void OnMouseDown()
        {
            GetComponent<MeshRenderer>().material = onMouseDownMaterial;
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