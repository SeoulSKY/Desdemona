using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.Assertions;

namespace Play
{
    public class Bot : MonoBehaviour
    {
        [SerializeField] private uint intelligence = 1;

        private string _host;

        private void Awake()
        {
            _host = "http://127.0.0.1:8000";
        }

        private string Url(string endPoint, params Tuple<string, string>[] parameters)
        {
            var url = $"{_host}/{endPoint}";
            return parameters.Length == 0 ? url : $"{url}?{string.Join('&', parameters.Select(t => $"{t.Item1}={t.Item2}"))}";
        }

        private async UniTask<string> SendGet(string endPoint, params Tuple<string, string>[] parameters)
        {
            var url = Url(endPoint, parameters);
            using (var request = UnityWebRequest.Get(url))
            {
                Debug.Log($"Sending GET to {url}");
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
        /// <param name="boardGrid">The grid to apply</param>
        /// <param name="player">The current player</param>
        /// <param name="position">The position to place a disk</param>
        /// <param name="onGameOver">The callback to be called with the game is over</param>
        /// <returns>The resulted board and whether the game is over or not</returns>
        public async UniTask<Tuple<char[][], bool>> Result(BoardGrid boardGrid, Player player, BoardGrid.Position position, Action<Player?> onGameOver)
        {
            var json = await SendGet("result",
                new Tuple<string, string>("board", boardGrid.ToString()),
                new Tuple<string, string>("player", player.ToChar().ToString()),
                new Tuple<string, string>("position", position.ToString()));
            
            var response = JObject.Parse(json);
            HandleGameOver(response, onGameOver);

            return new Tuple<char[][], bool>(
                ParseGrid((string) response["board"]), 
                IsGameOver(response)
                );
        }

        public async UniTask<ICollection<BoardGrid.Position>> Actions(BoardGrid boardGrid)
        {
            var json = await SendGet("actions", 
                new Tuple<string, string>("board", boardGrid.ToString()), 
                new Tuple<string, string>("player", Player.Human.ToChar().ToString()));
            
            var actions = JsonConvert.DeserializeObject<List<string>>(json);
            Assert.IsNotNull(actions, "Invalid format of Json from the server");

            return actions.Select(boardGrid.Find).ToHashSet();
        }
        
        /// <summary>
        /// Returns the decision of the AI from the given grid
        /// </summary>
        /// <param name="boardGrid">The grid to decide the next action of the AI</param>
        /// <param name="onGameOver">The method to be called when the game is over</param>
        /// <returns>The selected position to place a disk from the AI, the resulted board and weather the game is over or not</returns>
        public async UniTask<Tuple<BoardGrid.Position, char[][], bool>> Decide(BoardGrid boardGrid, Action<Player?> onGameOver)
        {
            var json = await SendGet("decide", 
                new Tuple<string, string>("board", boardGrid.ToString()), 
                new Tuple<string, string>("intelligence", intelligence.ToString())
                );

            var response = JObject.Parse(json);
            Assert.IsNotNull(response["result"], "Invalid format of Json from the server");

            HandleGameOver(response["result"], onGameOver);

            return new Tuple<BoardGrid.Position, char[][], bool>(
                string.IsNullOrEmpty((string) response["decision"]) ? null : boardGrid.Find((string) response["decision"]),
                ParseGrid((string) response["result"]["board"]),
                IsGameOver(response["result"])
                );
        }

        private static bool IsGameOver(JToken result)
        {
            return result["winner"] != null;
        }

        private static void HandleGameOver(JToken result, Action<Player?> onGameOver)
        {
            if (!IsGameOver(result))
            {
                return;
            }
            
            onGameOver(string.IsNullOrEmpty((string) result["winner"]) ? 
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