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
        [Tooltip("The reference disk to spawn other remaining disks")]
        [SerializeField] private Disk reference;
       
        [Tooltip("The x distance between each disk")]
        [SerializeField] private float xDiskDistance;
        [Tooltip("The z distance between each disk")]
        [SerializeField] private float zDiskDistance;

        private const uint BoardSize = 8;
        private Disk[,] _disks;

        private IEnumerator Start()
        {
            CreateDisks();
            yield return InitializeBoard();
        }

        private void CreateDisks()
        {
            _disks = new Disk[BoardSize, BoardSize];
            reference.gameObject.SetActive(false);
            for (var i = 0; i < BoardSize; i++)
            {
                for (var j = 0; j < BoardSize; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }
                    
                    var newDisk = Instantiate(reference, transform);
                    var refPos = reference.transform.position;
                    
                    newDisk.transform.position = new Vector3(refPos.x + j * xDiskDistance, refPos.y, refPos.z - i * zDiskDistance);
                    newDisk.name = $"{i} {j}";
                    newDisk.gameObject.SetActive(false);
                    _disks[i, j] = newDisk;
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
            
            for (var i = 0; i < BoardSize; i++)
            {
                for (var j = 0; j < BoardSize; j++)
                {
                    switch (response[i][j])
                    {
                        case Disk.Light:
                            _disks[i, j].FlipColor();
                            _disks[i, j].gameObject.SetActive(true);
                            break;
                        case Disk.Dark:
                            _disks[i, j].gameObject.SetActive(true);
                            break;
                    }
                }
            }
        }
    }
}