using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Play
{
    public class Grid : MonoBehaviour
    {
        /// <summary>
        /// The reference tile to spawn other remaining tiles
        /// </summary>
        private Tile _referenceTile;
       
        [Tooltip("The x distance between each tile")]
        [SerializeField] private float xTileDistance;
        [Tooltip("The z distance between each tile")]
        [SerializeField] private float zTileDistance;
        
        private const uint Size = 8;
        private Tile[,] _tiles;

        public delegate UniTask DiskPlaced(Tile tile);
        public event DiskPlaced OnDiskPlaced;

        private void Awake()
        {
            _referenceTile = GetComponentInChildren<Tile>();
        }

        private void Start()
        {
            PlaceTiles();
        }

        /// <summary>
        /// Enumerate each tile in this grid
        /// </summary>
        public IEnumerable<Tuple<uint, uint, Tile>> Enumerate()
        {
            for (uint i = 0; i < Size; i++)
            {
                for (uint j = 0; j < Size; j++)
                {
                    yield return new Tuple<uint, uint, Tile>(i, j, _tiles[i, j]);
                }
            }
        }

        /// <summary>
        /// Iterate each grid in this tile
        /// </summary>
        /// <returns>The iterator</returns>
        public IEnumerable<Tile> Tiles()
        {
            var enumerator = _tiles.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current as Tile;
            }
        }

        /// <summary>
        /// Returns the tile with the given tile name
        /// </summary>
        /// <param name="tileName">The name of the tile to find</param>
        /// <returns>The target tile or null if not found</returns>
        /// <exception cref="ArgumentException">When the tile with the given name is not found</exception>>
        public Tile GetTile(string tileName)
        {
            if (string.IsNullOrWhiteSpace(tileName))
            {
                throw new ArgumentException($"Given tile name is not valid: '{tileName}'");
            }
            
            foreach (var tile in _tiles)
            {
                if (tile.name == tileName)
                {
                    return tile;
                }
            }

            throw new ArgumentException($"Tile not found in this grid: '{tileName}'");
        }

        private void PlaceTiles()
        {
            _tiles = new Tile[Size, Size];

            foreach (var (i, j, _) in Enumerate())
            {
                Tile current;
                if (i == 0 && j == 0)
                {
                    current = _referenceTile;
                }
                else
                {
                    current = Instantiate(_referenceTile, transform);
                }

                var refPos = _referenceTile.transform.position;

                current.transform.position = new Vector3(refPos.x + j * xTileDistance, refPos.y, refPos.z - i * zTileDistance);
                current.name = $"{i},{j}";
                current.gameObject.SetActive(true);

                async UniTask Lambda(Tile tile)
                {
                    await OnDiskPlaced?.Invoke(tile).ToCoroutine();
                }

                current.OnDiskPlaced += Lambda;
                _tiles[i, j] = current;
            }
        }

        /// <summary>
        /// Wait while any of the disk in this grid is flipping
        /// </summary>
        /// <returns></returns>
        public async UniTask WaitWhileFlipping()
        {
            foreach (var tile in Tiles().Where(t => t.Disk != null && t.Disk.IsFlipping))
            {
                await tile.Disk.WaitWhileFlipping();
            }
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