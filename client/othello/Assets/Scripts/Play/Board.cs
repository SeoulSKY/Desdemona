using System.Collections;
using System.Text;
using UnityEngine;

namespace Play
{
    public class Board : MonoBehaviour
    {
        [Tooltip("The reference tile to spawn other remaining tiles")]
        [SerializeField] private Tile reference;
       
        [Tooltip("The x distance between each tile")]
        [SerializeField] private float xTileDistance;
        [Tooltip("The z distance between each tile")]
        [SerializeField] private float zTileDistance;

        [Tooltip("The game object to create tiles as its children")]
        [SerializeField] private GameObject grid;
        
        [Tooltip("The game object for communicating with the AI server")]
        [SerializeField] private Bot bot;
        
        private const uint GridSize = 8;
        private Tile[,] _grid;
        
        private IEnumerator Start()
        {
            PlaceTiles();
            yield return InitializeBoard();
        }

        private void PlaceTiles()
        {
            _grid = new Tile[GridSize, GridSize];
            
            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        _grid[i, j] = reference;
                        continue;
                    }

                    var newTile = Instantiate(reference, grid.transform);
                    var refPos = reference.transform.position;

                    newTile.transform.position = new Vector3(refPos.x + j * xTileDistance, refPos.y, refPos.z - i * zTileDistance);
                    newTile.name = $"{i},{j}";
                    newTile.gameObject.SetActive(true);
                    newTile.OnDiskPlaced += OnDiskPlaced;
                    _grid[i, j] = newTile;
                }
            }
        }

        private IEnumerator InitializeBoard()
        {
            yield return bot.InitialBoard(response =>
            {
                for (var i = 0; i < GridSize; i++)
                {
                    for (var j = 0; j < GridSize; j++)
                    {
                        if (DiskColorMethods.CanParse(response[i][j]))
                        {
                            _grid[i, j].PlaceDisk(DiskColorMethods.Parse(response[i][j]));
                        }
                    }
                }
            });
        }

        private IEnumerator OnDiskPlaced(Tile tile)
        {
            yield return bot.Result(this, Player.Human, tile, response =>
            {
                for (var i = 0; i < GridSize; i++)
                {
                    for (var j = 0; j < GridSize; j++)
                    {
                        if (!DiskColorMethods.CanParse(response[i][j]))
                        {
                            if (_grid[i, j].HasDisk())
                            {
                                _grid[i, j].ClearDisk();
                            }
                            continue;
                        }
                    
                        var diskColor = DiskColorMethods.Parse(response[i][j]);

                        if (!_grid[i, j].HasDisk())
                        {
                            _grid[i, j].PlaceDisk(diskColor);
                        } else if (_grid[i, j].DiskColor() != diskColor)
                        {
                            _grid[i, j].FlipDisk();
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Convert this object to a string representation
        /// </summary>
        /// <returns>The converted string</returns>
        public new string ToString()
        {
            var builder = new StringBuilder();

            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    builder.Append(_grid[i, j].ToString());
                }
                builder.Append('\n');
            }

            return builder.ToString().Trim();
        }
    }
}