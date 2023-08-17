using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Play
{
    public class BoardGrid : MonoBehaviour
    {
        public class Position
        {
            public uint Row { get; private set; }
            public uint Col { get; private set; }

            public Position(uint row, uint col)
            {
                if (row >= Size)
                {
                    throw new ArgumentException($"row must be less than {Size}. Given: {row}");
                } 
                if (col >= Size)
                {
                    throw new ArgumentException($"col must be less than {Size}. Given: {col}");
                }
                
                Row = row;
                Col = col;
            }

            private static Position NewUnchecked(uint row, uint col)
            {
                return new Position(0, 0)
                {
                    Row = row,
                    Col = col,
                };
            }

            private bool IsValid()
            {
                return Row < Size && Col < Size;
            }
            
            /// <summary>
            /// Returns the neighbour positions of this position
            /// </summary>
            /// <returns></returns>
            public IEnumerable<Position> Neighbours()
            {
                return new []
                { 
                    NewUnchecked(Row - 1, Col - 1),
                    NewUnchecked(Row - 1, Col),
                    NewUnchecked(Row - 1, Col + 1),
                    NewUnchecked(Row, Col - 1),
                    NewUnchecked(Row, Col + 1),
                    NewUnchecked(Row + 1, Col - 1),
                    NewUnchecked(Row + 1, Col),
                    NewUnchecked(Row + 1, Col + 1),
                }.Where(p => p.IsValid());
            }

            /// <summary>
            /// Calculate the Euclidean distance between this Position and the given one
            /// </summary>
            /// <param name="other">The other position</param>
            /// <returns>The Euclidean distance</returns>
            public uint Distance(Position other)
            {
                var rowDistance = Math.Abs((int) Row - (int) other.Row);
                var colDistance = Math.Abs((int) Col - (int) other.Col);
                return (uint) Math.Max(rowDistance, colDistance);
            }

            public override string ToString()
            {
                return $"{Row},{Col}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is Position other && Row == other.Row && Col == other.Col;
            }

            /// <summary>
            /// Parse the given tile into the corresponding position
            /// </summary>
            /// <param name="tile">The tile to parse</param>
            /// <returns>The corresponding position</returns>
            public static Position Parse(Tile tile)
            {
                var indexes = tile.name.Split(",").Select(uint.Parse).ToArray();
                return new Position(indexes[0], indexes[1]);
            }
        }
        
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

        public delegate UniTask DiskPlaced(Position position);
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
        /// Enumerate each tile in a breath-search first manner
        /// </summary>
        /// <param name="from">The starting position of the enumeration</param>
        public IEnumerable<Tuple<uint, uint, Tile>> Enumerate(Position from)
        {
            var queue = new Queue<Position>();
            var visited = new HashSet<Position>();
            queue.Enqueue(from);
            visited.Add(from);

            while (queue.Any())
            {
                var position = queue.Dequeue();
                yield return new Tuple<uint, uint, Tile>(position.Row, position.Col, Tile(position));
                
                foreach (var neighbour in position.Neighbours())
                {
                    if (visited.Contains(neighbour))
                    {
                        continue;
                    }
                    visited.Add(neighbour);
                    queue.Enqueue(neighbour);
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
        /// Returns the tile from the given position
        /// </summary>
        /// <param name="position">The position of the tile</param>
        /// <returns>The tile</returns>
        public Tile Tile(Position position)
        {
            return _tiles[position.Row, position.Col];
        }

        /// <summary>
        /// Find the position of the tile with the given name
        /// </summary>
        /// <param name="tileName">The name of the tile to find</param>
        /// <returns>The position</returns>
        public Position Find(string tileName)
        {
            if (string.IsNullOrWhiteSpace(tileName))
            {
                throw new ArgumentException($"Given tile name is not valid: '{tileName}'");
            }
            
            foreach (var (i, j, tile) in Enumerate())
            {
                if (tile.name == tileName)
                {
                    return new Position(i, j);
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

                var refPos = _referenceTile.transform.localPosition;
                current.transform.localPosition = new Vector3(refPos.x + j * xTileDistance, refPos.y, refPos.z - i * zTileDistance);
                
                current.name = new Position(i, j).ToString();
                current.gameObject.SetActive(true);

                async UniTask Lambda(Tile tile)
                {
                    await OnDiskPlaced?.Invoke(Position.Parse(tile)).ToCoroutine();
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
                Assert.IsNotNull(tile.Disk);
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