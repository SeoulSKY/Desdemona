using System;
using System.Collections;
using System.Linq;
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
        
        private IEnumerator Start()
        {
            yield return InitializeBoard();
            _grid.OnDiskPlaced += OnDiskPlaced;
        }

        private IEnumerator InitializeBoard()
        {
            yield return bot.InitialBoard(response =>
            {
                foreach (var (i, j, tile) in _grid.Enumerate())
                {
                    if (!DiskColorMethods.CanParse(response[i][j]))
                    {
                        continue;
                    }
                    tile.PlaceDisk(DiskColorMethods.Parse(response[i][j]));
                }
            });
            
            UpdateActiveTiles();
        }
        
        private IEnumerator OnDiskPlaced(Tile tile)
        {
            if (!tile.CanPlaceDisk)
            {
                yield break;
            }

            foreach (var t in _grid.Tiles())
            {
                t.CanPlaceDisk = false;
            }

            yield return bot.Result(_grid, Player.Human, tile, UpdateGrid, OnGameOver);
            yield return new WaitForSeconds(1);
            yield return bot.Decide(_grid, (decision, result) =>
            {
                if (decision == null)
                {
                    Debug.Log("AI has no actions to take this turn");
                }
                else
                {
                    decision.PlaceDisk(Player.Bot.Disk());
                }
                UpdateGrid(result);
                UpdateActiveTiles();
            }, OnGameOver);
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

        private void UpdateActiveTiles()
        {
            var coroutine = bot.Actions(_grid, actions =>
            {
                foreach (var tile in _grid.Tiles())
                {
                    tile.CanPlaceDisk = actions.Contains(tile);
                }
            });

            StartCoroutine(coroutine);
        }

        private static void OnGameOver(Player? winner)
        {
            Debug.Log("Game over");
            Debug.Log(winner.HasValue ? $"Winner: {winner}" : "Winner: Draw");
        }
    }
}