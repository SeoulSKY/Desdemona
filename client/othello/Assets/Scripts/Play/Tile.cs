using System;
using System.Collections;
using UnityEngine;

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
        
        public delegate IEnumerator DiskPlaced();
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
        }

        private IEnumerator OnMouseUpAsButton()
        {
            GetComponent<MeshRenderer>().materials = new Material[]{};
            OnMouseEnter();

            yield return OnDiskPlaced?.Invoke();
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