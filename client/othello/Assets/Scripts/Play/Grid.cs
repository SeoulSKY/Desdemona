using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace Play
{
    public class Grid : MonoBehaviour
    {
        [Tooltip("The reference tile to spawn other remaining tiles")]
        [SerializeField] private Tile reference;
       
        [Tooltip("The x distance between each tile")]
        [SerializeField] private float xTileDistance;
        [Tooltip("The z distance between each tile")]
        [SerializeField] private float zTileDistance;
        
        private const uint Size = 8;
        private Tile[,] _tiles;

        public delegate IEnumerator DiskPlaced(Tile tile);
        public event DiskPlaced OnDiskPlaced;
        
        private void Start()
        {
            PlaceTiles();
        }

        /// <summary>
        /// Enumerate each tile in the grid
        /// </summary>
        /// <param name="callback">The callback for each tile</param>
        public void Enumerate(Action<uint, uint, Tile> callback)
        {
            for (uint i = 0; i < Size; i++)
            {
                for (uint j = 0; j < Size; j++)
                {
                    callback(i, j, _tiles[i, j]);
                }
            }
        }
        
        private void PlaceTiles()
        {
            _tiles = new Tile[Size, Size];
            
            Enumerate((i, j, _) =>
            {
                Tile current;
                if (i == 0 && j == 0)
                {
                    current = reference;
                }
                else
                {
                    current = Instantiate(reference, transform);
                }

                var refPos = reference.transform.position;

                current.transform.position = new Vector3(refPos.x + j * xTileDistance, refPos.y, refPos.z - i * zTileDistance);
                current.name = $"{i},{j}";
                current.gameObject.SetActive(true);

                IEnumerator Lambda(Tile tile)
                {
                    yield return OnDiskPlaced?.Invoke(tile);
                }

                current.OnDiskPlaced += Lambda;
                _tiles[i, j] = current;
            });
        }
        
        /// <summary>
        /// Convert this object to a string representation
        /// </summary>
        /// <returns>The converted string</returns>
        public new string ToString()
        {
            var builder = new StringBuilder();
            
            for (var i = 0; i < Size; i++)
            {
                for (var j = 0; j < Size; j++)
                {
                    builder.Append(_tiles[i, j].ToString());
                }
                builder.Append('\n');
            }

            return builder.ToString().Trim();
        }
    }
}