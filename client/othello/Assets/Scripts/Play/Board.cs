using System.Collections;
using UnityEngine;

namespace Play
{
    public class Board : MonoBehaviour
    {
        [Tooltip("The game object to create tiles as its children")]
        [SerializeField] private Grid grid;
        
        [Tooltip("The game object for communicating with the AI server")]
        [SerializeField] private Bot bot;
        
        private IEnumerator Start()
        {
            grid.OnDiskPlaced += OnDiskPlaced;
            yield return InitializeBoard();
        }

        private IEnumerator InitializeBoard()
        {
            yield return bot.InitialBoard(response =>
            {
                grid.Enumerate((i, j, tile) =>
                {
                    if (!DiskColorMethods.CanParse(response[i][j]))
                    {
                        return;
                    }
                    tile.PlaceDisk(DiskColorMethods.Parse(response[i][j]));
                });
            });
        }
        
        private IEnumerator OnDiskPlaced(Tile tile)
        {
            yield return bot.Result(grid, Player.Human, tile, response =>
            {
                grid.Enumerate((i, j, current) =>
                {
                    if (!DiskColorMethods.CanParse(response[i][j]))
                    {
                        if (current.HasDisk())
                        {
                            current.ClearDisk();
                        }
                        return;
                    }
                    
                    var diskColor = DiskColorMethods.Parse(response[i][j]);

                    if (!current.HasDisk())
                    {
                        current.PlaceDisk(diskColor);
                    } 
                    else if (current.DiskColor() != diskColor)
                    {
                        current.FlipDisk();
                    }
                });
            });
        }
    }
}