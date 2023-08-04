using System;
using System.Collections;
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
        
        public delegate IEnumerator DiskPlaced(Tile tile);
        public event DiskPlaced OnDiskPlaced;


        private void Awake()
        {
            _disk = GetComponentInChildren<Disk>(true);
            _disk.gameObject.SetActive(false);
        }

        /// <summary>
        /// Check if the disk is present on the tile
        /// </summary>
        /// <returns>true if it is present, false otherwise</returns>
        public bool HasDisk()
        {
            return _disk.gameObject.activeSelf;
        }

        /// <summary>
        /// Get the color of the disk on this tile
        /// </summary>
        /// <returns>The color of the disk</returns>
        /// <exception cref="InvalidOperationException">If !HasDisk()</exception>
        public Disk.Color DiskColor()
        {
            if (!HasDisk())
            {
                throw new InvalidOperationException("There is no disk on this tile to get its color");
            }

            return _disk.CurrentColor;
        }

        /// <summary>
        /// Place a disk on this tile
        /// </summary>
        /// <param name="color">The color of the disk to place</param>
        /// <exception cref="InvalidOperationException">If HasDisk()</exception>
        public void PlaceDisk(Disk.Color color)
        {
            if (HasDisk())
            {
                throw new InvalidOperationException("This tile is occupied by another disk");
            }
            
            _disk.SetColor(color);
            _disk.gameObject.SetActive(true);
            
            Debug.Log($"Placed {color} at {name}");
        }

        /// <summary>
        /// Clear the disk on this tile
        /// </summary>
        /// <exception cref="InvalidOperationException">If !HasDisk()</exception>
        public void ClearDisk()
        {
            if (!HasDisk())
            {
                throw new InvalidOperationException("There is no disk on this tile to clear");
            }

            _disk.gameObject.SetActive(false);
        }

        /// <summary>
        /// FLip the disk on this tile and display the other color
        /// </summary>
        /// <exception cref="InvalidOperationException">If !HasDisk()</exception>
        public void FlipDisk()
        {
            if (!HasDisk())
            {
                throw new InvalidOperationException("There is no disk on this tile to flip");
            }
            
            _disk.SetColor(_disk.CurrentColor == Disk.Color.Dark ? Disk.Color.Light : Disk.Color.Dark);
            Debug.Log($"Flipped {name}");
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
            return HasDisk() ? _disk.CurrentColor.ToChar().ToString() : EmptyChar.ToString();
        }
    }
}