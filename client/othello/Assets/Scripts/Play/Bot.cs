using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using CandyCoded.env;
using UnityEngine;
using UnityEngine.Networking;

namespace Play
{
    public class Bot : MonoBehaviour
    {
        private string _host;

        private void Awake()
        {
            _host = env.variables["AI_SERVER_HOST"];
        }

        private string Url(string endPoint, params Tuple<string, string>[] parameters)
        {
            var url = $"{_host}/{endPoint}";
            return parameters.Length == 0 ? url : $"{url}?{string.Join('&', parameters.Select(t => $"{t.Item1}={t.Item2}"))}";
        }

        private IEnumerator SendGet(string endPoint, Action<string> callback, params Tuple<string, string>[] parameters)
        {
            using (var request = UnityWebRequest.Get(Url(endPoint, parameters)))
            {
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new HttpRequestException(request.error);
                }

                callback(request.downloadHandler.text);
            }
        }
        
        /// <summary>
        /// Returns the initial board of the game
        /// </summary>
        /// <param name="callback">The method to be called with the initial board</param>
        /// <returns></returns>
        public IEnumerator InitialBoard(Action<char[][]> callback)
        {
            yield return SendGet("initial-board", s => callback(ParseGrid(s)));
        }

        /// <summary>
        /// Returns the result of the given action applied to the board
        /// </summary>
        /// <param name="grid">The grid to apply</param>
        /// <param name="player">The current player</param>
        /// <param name="tile">The tile to place a disk</param>
        /// <param name="callback">The method to be called with the result</param>
        /// <returns></returns>
        public IEnumerator Result(Grid grid, Player player, Tile tile, Action<char[][]> callback)
        {
            yield return SendGet("result", s => callback(ParseGrid(s)),
                new Tuple<string, string>("board", grid.ToString()),
                new Tuple<string, string>("player", player.ToChar().ToString()),
                new Tuple<string, string>("position", tile.name)
                    );
        }
        
        private static char[][] ParseGrid(string s)
        {
            return s.Split("\n")
                .Select(line => line.ToCharArray())
                .ToArray();
        }
    }
}