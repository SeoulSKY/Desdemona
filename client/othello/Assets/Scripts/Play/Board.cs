using System.Collections;
using System.Linq;
using System.Net.Http;
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
                        continue;
                    }

                    var newTile = Instantiate(reference, grid.transform);
                    var refPos = reference.transform.position;

                    newTile.transform.position = new Vector3(refPos.x + i * xTileDistance, refPos.y, refPos.z - j * zTileDistance);
                    newTile.name = $"Tile {i} {j}";
                    newTile.gameObject.SetActive(true);
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
    }
}