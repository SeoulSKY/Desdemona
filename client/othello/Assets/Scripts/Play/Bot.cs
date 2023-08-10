using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CandyCoded.env;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Play
{
    public class Bot : MonoBehaviour
    {
        private string _host;
        private uint _intelligence = 1;

        [Serializable]
        private class DecideData
        {
            [CanBeNull]
            public string decision;
            public ResultData result;
        }

        [Serializable]
        private class ResultData
        {
            public string board;
        }

        [Serializable]
        private class GameOverData : ResultData
        {
            public char winner;
        }
        
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

                Debug.Log($"Response from the server:\n{request.downloadHandler.text}");
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
        /// <param name="callback1">The method to be called with the result</param>
        /// <param name="callback2">The method to be called when the game is over</param>
        /// <returns></returns>
        public IEnumerator Result(Grid grid, Player player, Tile tile, Action<char[][]> callback1, Action<Player?> callback2)
        {
            yield return SendGet("result", s =>
                {
                    var result = JsonUtility.FromJson<ResultData>(s) ?? JsonUtility.FromJson<GameOverData>(s);
                    callback1(ParseGrid(result.board));

                    if (result is GameOverData gameOverData)
                    {
                        callback2(PlayerMethods.CanParse(gameOverData.winner) ? PlayerMethods.Parse(gameOverData.winner) : null);
                    }
                },
                new Tuple<string, string>("board", grid.ToString()),
                new Tuple<string, string>("player", player.ToChar().ToString()),
                new Tuple<string, string>("position", tile.name)
                );
        }

        public IEnumerator Actions(Grid grid, Action<ICollection<Tile>> callback)
        {
            yield return SendGet("actions", s => 
                {
                    var actions = JsonConvert.DeserializeObject<HashSet<string>>(s);
                    Debug.Assert(actions != null, "Invalid format of Json from the server");
                    callback(grid.GetComponentsInChildren<Tile>().Where(t => actions.Contains(t.name)).ToList());
                }, new Tuple<string, string>("board", grid.ToString()), 
                new Tuple<string, string>("player", Player.Human.ToChar().ToString())
                );
        }
        
        /// <summary>
        /// Returns the decision of the AI from the given grid
        /// </summary>
        /// <param name="grid">The grid to decide the next action of the AI</param>
        /// <param name="callback1">The method to be called with the result</param>
        /// <param name="callback2">The method to be called when the game is over</param>
        /// <returns></returns>
        public IEnumerator Decide(Grid grid, Action<Tile, char[][]> callback1, Action<Player?> callback2)
        {
            yield return SendGet("decide", s =>
                {
                    var response = JsonUtility.FromJson<DecideData>(s);
                    callback1(
                        response.decision == null ? null : grid.GetTile(response.decision),
                        ParseGrid(response.result.board)
                    );
                    
                    // TODO fix bug not being true when the game is over
                    if (response.result is GameOverData gameOverData)
                    {
                        Debug.Log("Test");
                        callback2(PlayerMethods.CanParse(gameOverData.winner) ? PlayerMethods.Parse(gameOverData.winner) : null);
                    }
                }, new Tuple<string, string>("board", grid.ToString()), 
                new Tuple<string, string>("intelligence", _intelligence.ToString())
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