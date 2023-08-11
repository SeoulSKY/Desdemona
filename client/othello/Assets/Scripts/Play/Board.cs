using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Play
{
    public class Board : MonoBehaviour
    {
        [Tooltip("The game object for communicating with the AI server")]
        [SerializeField] private Bot bot;
        
        private Grid _grid;

        private void Awake()
        {
            _grid = GetComponentInChildren<Grid>();
        }
        
        private async void Start()
        {
            await InitializeBoard();
            _grid.OnDiskPlaced += OnDiskPlaced;
        }

        private async UniTask InitializeBoard()
        {
            var response = await bot.InitialBoard();
            foreach (var (i, j, tile) in _grid.Enumerate())
            {
                if (!DiskColorMethods.CanParse(response[i][j]))
                {
                    continue;
                }
                tile.PlaceDisk(DiskColorMethods.Parse(response[i][j]));
            }
            
            await UpdateActiveTiles();
        }
        
        private async UniTask OnDiskPlaced(Tile tile)
        {
            if (!tile.CanPlaceDisk)
            {
                return;
            }

            foreach (var t in _grid.Tiles())
            {
                t.CanPlaceDisk = false;
            }

            var (board, isGameOver) = await bot.Result(_grid, Player.Human, tile, OnGameOver);
            UpdateGrid(board);

            if (isGameOver)
            {
                return;
            }
            
            await _grid.WaitWhileFlipping();
            await Decide();
        }

        private async UniTask Decide()
        {
            var (decision, result, isGameOver) = await bot.Decide(_grid, OnGameOver);
            if (isGameOver)
            {
                return;
            }
            
            if (decision == null)
            {
                Debug.Log("AI has no actions to take this turn");
            }
            else
            {
                decision.PlaceDisk(Player.Bot.Disk());
            }
            UpdateGrid(result);
            await UpdateActiveTiles();
        }
        
        private void UpdateGrid(char[][] newGrid)
        {
            foreach (var (i, j, current) in _grid.Enumerate())
            {
                if (!DiskColorMethods.CanParse(newGrid[i][j]))
                {
                    if (current.Disk != null)
                    {
                        current.ClearDisk();
                    }
                    continue;
                }
                    
                var diskColor = DiskColorMethods.Parse(newGrid[i][j]);

                if (current.Disk == null)
                {
                    current.PlaceDisk(diskColor);
                } 
                else if (current.Disk.Color != diskColor)
                {
                    current.Disk.Flip();
                }
            }
        }

        private async UniTask UpdateActiveTiles()
        {
            var actions = await bot.Actions(_grid);
            if (actions.Count == 0)
            {
                Debug.Log("Human has no actions to take this turn");
                await Decide();
            }
            
            foreach (var tile in _grid.Tiles())
            {
                tile.CanPlaceDisk = actions.Contains(tile);
            }
        }

        private static void OnGameOver(Player? winner)
        {
            Debug.Log("Game over");
            Debug.Log(winner.HasValue ? $"Winner: {winner}" : "Winner: Draw");
        }
    }
}