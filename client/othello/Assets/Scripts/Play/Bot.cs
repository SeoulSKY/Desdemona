using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CandyCoded.env;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

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

        private async UniTask<string> SendGet(string endPoint, params Tuple<string, string>[] parameters)
        {
            using (var request = UnityWebRequest.Get(Url(endPoint, parameters)))
            {
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new HttpRequestException(request.error);
                }

                Debug.Log($"Response from the server:\n{request.downloadHandler.text}");
                return request.downloadHandler.text;
            }
        }
        
        /// <summary>
        /// Returns the initial board of the game
        /// </summary>
        /// <returns>The initial board</returns>
        public async UniTask<char[][]> InitialBoard()
        {
            return ParseGrid(await SendGet("initial-board"));
        }
        
        /// <summary>
        /// Returns the result of the given action applied to the board
        /// </summary>
        /// <param name="grid">The grid to apply</param>
        /// <param name="player">The current player</param>
        /// <param name="tile">The tile to place a disk</param>
        /// <param name="callback">The callback to be called with the game is over</param>
        /// <returns>The resulted board</returns>
        public async UniTask<Tuple<char[][], bool>> Result(Grid grid, Player player, Tile tile, Action<Player?> callback)
        {
            var json = await SendGet("result",
                new Tuple<string, string>("board", grid.ToString()),
                new Tuple<string, string>("player", player.ToChar().ToString()),
                new Tuple<string, string>("position", tile.name));
            
            var response = JObject.Parse(json);
            HandleGameOver(response, callback);

            return new Tuple<char[][], bool>(
                ParseGrid((string) response["board"]), 
                IsGameOver(response)
                );
        }

        public async UniTask<ICollection<Tile>> Actions(Grid grid)
        {
            var json = await SendGet("actions", 
                new Tuple<string, string>("board", grid.ToString()), 
                new Tuple<string, string>("player", Player.Human.ToChar().ToString()));
            
            var actions = JsonConvert.DeserializeObject<HashSet<string>>(json);
            Debug.Assert(actions != null, "Invalid format of Json from the server");
            return grid.GetComponentsInChildren<Tile>()
                .Where(t => actions.Contains(t.name))
                .ToList();
        }
        
        /// <summary>
        /// Returns the decision of the AI from the given grid
        /// </summary>
        /// <param name="grid">The grid to decide the next action of the AI</param>
        /// <param name="callback">The method to be called when the game is over</param>
        /// <returns></returns>
        public async UniTask<Tuple<Tile, char[][], bool>> Decide(Grid grid, Action<Player?> callback)
        {
            var json = await SendGet("decide", 
                new Tuple<string, string>("board", grid.ToString()), 
                new Tuple<string, string>("intelligence", _intelligence.ToString())
                );

            var response = JObject.Parse(json);
            HandleGameOver(response["result"], callback);
            
            return new Tuple<Tile, char[][], bool>(
                string.IsNullOrEmpty((string) response["decision"]) ? null : grid.GetTile((string) response["decision"]),
                ParseGrid((string) response["result"]["board"]),
                IsGameOver(response["result"])
                );
        }

        private static bool IsGameOver(JToken result)
        {
            return result["winner"] != null;
        }

        private static void HandleGameOver(JToken result, Action<Player?> callback)
        {
            if (!IsGameOver(result))
            {
                return;
            }
            
            callback(string.IsNullOrEmpty((string) result["winner"]) ? 
                null : 
                PlayerMethods.Parse((char) result["winner"])
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