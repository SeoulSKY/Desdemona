using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.Assertions;

namespace Play
{
    public readonly struct Result
    {
        public char[][] Board { get; }
        public bool IsGameOver { get; }

        private readonly Player? _winner;
        
        public Player? Winner
        {
            get
            {
                if (!IsGameOver)
                {
                    throw new InvalidOperationException("Cannot get the winner when the game is not over");
                }
                return _winner;
            }
        }

        public Result(char[][] board)
        {
            Board = board;
            IsGameOver = false;
            _winner = null;
        }

        public Result(char[][] board, Player? winner)
        {
            Board = board;
            IsGameOver = true;
            _winner = winner;
        }
    }

    public readonly struct Decision
    {
        [CanBeNull]
        public BoardGrid.Position Position { get; }
        public Result Result { get; }

        public Decision([CanBeNull] BoardGrid.Position position, Result result)
        {
            Position = position;
            Result = result;
        }
    }
    
    public class Bot : MonoBehaviour
    {
        private uint _intelligence;
        public void SetIntelligence(int value)
        {
            _intelligence = (uint) value;
        }

        private string _host;

        private void Awake()
        {
            _host = "http://localhost:8000/api";
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
        /// Returns the evaluation of the given board
        /// </summary>
        /// <param name="boardGrid">The board to evaluate</param>
        /// <returns>The evaluation</returns>
        public async UniTask<float> Evaluate(BoardGrid boardGrid)
        {
            return float.Parse(await SendGet("evaluate", new Tuple<string, string>("board", boardGrid.ToString())));
        }
        
        /// <summary>
        /// Returns the result of the given action applied to the board
        /// </summary>
        /// <param name="boardGrid">The grid to apply</param>
        /// <param name="player">The current player</param>
        /// <param name="position">The position to place a disk</param>
        /// <returns>The resulted board, whether the game is over or not, and the winner if game is over</returns>
        public async UniTask<Result> Result(BoardGrid boardGrid, Player player, BoardGrid.Position position)
        {
            var json = await SendGet("result",
                new Tuple<string, string>("board", boardGrid.ToString()),
                new Tuple<string, string>("player", player.ToChar().ToString()),
                new Tuple<string, string>("position", position.ToString()));
            
            var response = JObject.Parse(json);

            return IsGameOver(response) ? 
                new Result(ParseGrid((string)response["board"]), GetWinner(response)) : 
                new Result(ParseGrid((string)response["board"]));
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
        /// <returns>The selected position to place a disk from the AI, the resulted board,weather the game is over or not and the winner if the game is over</returns>
        public async UniTask<Decision> Decide(BoardGrid boardGrid)
        {
            var json = await SendGet("decide", 
                new Tuple<string, string>("board", boardGrid.ToString()), 
                new Tuple<string, string>("intelligence", _intelligence.ToString())
                );

            var response = JObject.Parse(json);
            Assert.IsNotNull(response["result"], "Invalid format of Json from the server");

            var result = IsGameOver(response["result"]) ?
                new Result(ParseGrid((string) response["result"]["board"]), GetWinner(response["result"])) :
                new Result(ParseGrid((string) response["result"]["board"]));

            return new Decision(
                string.IsNullOrEmpty((string) response["decision"]) ? null : boardGrid.Find((string)response["decision"]),
                result
            );
        }

        private static bool IsGameOver(JToken result)
        {
            return result["winner"] != null;
        }

        private static Player? GetWinner(JToken result)
        {
            if (!IsGameOver(result))
            {
                throw new InvalidOperationException("Cannot get the winner when the game is not over");
            }
            
            return string.IsNullOrEmpty((string) result["winner"]) ? null : PlayerMethods.Parse((char) result["winner"]);
        }
        
        private static char[][] ParseGrid(string s)
        {
            return s.Split("\n")
                .Select(line => line.ToCharArray())
                .ToArray();
        }
    }
}