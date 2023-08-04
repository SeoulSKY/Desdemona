using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Text;
using CandyCoded.env;
using UnityEngine;
using UnityEngine.Networking;

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
            char[][] response;
            using (var request = UnityWebRequest.Get($"{env.variables["AI_SERVER_HOST"]}/initial-board"))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new HttpRequestException(request.error);
                }

                response = request.downloadHandler.text
                    .Split("\n")
                    .Select(line => line.ToCharArray())
                    .ToArray();
            }
            
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
        }

        private IEnumerator OnDiskPlaced(Tile tile)
        {
            char[][] response;
            using (var request = UnityWebRequest.Get($"{env.variables["AI_SERVER_HOST"]}/result?board={ToString()}&player=H&action={tile.name}"))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new HttpRequestException(request.error);
                }
                
                Debug.Log($"Resulted board: \n{request.downloadHandler.text}");
                response = request.downloadHandler.text
                    .Split("\n")
                    .Select(line => line.ToCharArray())
                    .ToArray();
            }


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
        }

        /// <summary>
        /// Convert this object to a string representation
        /// </summary>
        /// <returns>The converted string</returns>
        private new string ToString()
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