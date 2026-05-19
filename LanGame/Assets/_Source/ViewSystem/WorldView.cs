using System.Collections.Generic;
using NetworkSystem;
using PlayerSystem;
using UnityEngine;

namespace ViewSystem
{
    public class WorldView : MonoBehaviour
    {
        [SerializeField] private NetworkClient networkClient;
        [SerializeField] private PlayerNetworkView playerPrefab;

        private readonly Dictionary<string, PlayerNetworkView> players = new();

        private void Update()
        {
            ServerStateMessage state = networkClient.LastState;

            if (state == null || state.players == null)
            {
                return;
            }

            foreach (var pair in state.players)
            {
                string playerId = pair.Key;
                PlayerState playerState = pair.Value;

                if (!players.ContainsKey(playerId))
                {
                    PlayerNetworkView view = Instantiate(playerPrefab);
                    bool isLocal = playerId == networkClient.PlayerId;

                    view.Init(isLocal);
                    players.Add(playerId, view);
                }

                players[playerId].UpdateView(
                    playerState.x,
                    playerState.y,
                    playerState.isDead);
            }
        }
    }
}