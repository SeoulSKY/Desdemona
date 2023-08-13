using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

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
        
        private async UniTask OnDiskPlaced(Grid.Position position)
        {
            var tile = _grid.Tile(position);
            if (!tile.CanPlaceDisk)
            {
                return;
            }

            foreach (var t in _grid.Tiles())
            {
                t.CanPlaceDisk = false;
            }

            var (board, isGameOver) = await bot.Result(_grid, Player.Human, position, OnGameOver);
            await UpdateGrid(board, position);

            if (isGameOver)
            {
                return;
            }
            
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
                _grid.Tile(decision).PlaceDisk(Player.Bot.Disk());
            }
            await UpdateGrid(result, decision);
            await UpdateActiveTiles();
        }
        
        private async UniTask UpdateGrid(char[][] newGrid, Grid.Position start)
        {
            var flipping = new List<Tuple<uint, Tile>>();
            foreach (var (i, j, current) in _grid.Enumerate(start))
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
                    flipping.Add( new Tuple<uint, Tile>(
                        start.Distance(new Grid.Position(i, j)),
                        current
                        )
                    );
                }
            }

            uint prevDistance = 1;
            foreach (var (distance, tile) in flipping.OrderBy(t => t.Item1))
            {
                if (distance > prevDistance)
                {
                    await UniTask.WaitForSeconds(0.2f);
                }
                
                Assert.IsNotNull(tile.Disk);
                Debug.Log($"Flipping {tile.name}");
                tile.Disk.Flip();

                prevDistance = distance;
            }
            
            await _grid.WaitWhileFlipping();
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
                tile.CanPlaceDisk = actions.Contains(Grid.Position.Parse(tile));
            }
        }

        private static void OnGameOver(Player? winner)
        {
            Debug.Log("Game over");
            Debug.Log(winner.HasValue ? $"Winner: {winner}" : "Winner: Draw");
        }
    }
}