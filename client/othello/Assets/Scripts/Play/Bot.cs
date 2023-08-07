using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CandyCoded.env;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Play
{
    public class Bot : MonoBehaviour
    {
        private string _host;
        private uint _intelligence = 1;

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
        
        /// <summary>
        /// Returns the decision of the AI from the given grid
        /// </summary>
        /// <param name="grid">The grid to decide the next action of the AI</param>
        /// <param name="callback">The method to be called with the result</param>
        /// <returns></returns>
        public IEnumerator Decide(Grid grid, Action<Tile, char[][], char?> callback)
        {
            yield return SendGet("decide", s =>
            {
                var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(s);
                if (response == null)
                {
                    throw new JsonException("Invalid format of Json received from the server");
                }

                callback(
                    grid.GetComponentsInChildren<Tile>().First(t => t.name == response["decision"]),
                    ParseGrid(response["result"]),
                    response.TryGetValue("winner", out var winner) ? winner[0] : null
                    );
            }, new Tuple<string, string>("board", grid.ToString()), 
                new Tuple<string, string>("intelligence", _intelligence.ToString()));
        }
        
        private static char[][] ParseGrid(string s)
        {
            return s.Split("\n")
                .Select(line => line.ToCharArray())
                .ToArray();
        }
    }
}