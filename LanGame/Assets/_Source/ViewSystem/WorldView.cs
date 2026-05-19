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
        [SerializeField] private BulletNetworkView bulletPrefab;

        private readonly Dictionary<string, PlayerNetworkView> players = new();
        private readonly Dictionary<string, BulletNetworkView> bullets = new();

        private void Update()
        {
            ServerStateMessage state = networkClient.LastState;

            if (state == null)
            {
                return;
            }

            UpdatePlayers(state);
            UpdateBullets(state);
        }

        private void UpdatePlayers(ServerStateMessage state)
        {
            if (state.players == null)
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

        private void UpdateBullets(ServerStateMessage state)
        {
            if (state.bullets == null)
            {
                return;
            }

            foreach (var pair in state.bullets)
            {
                string bulletId = pair.Key;
                BulletState bulletState = pair.Value;

                if (!bullets.ContainsKey(bulletId))
                {
                    BulletNetworkView view = Instantiate(bulletPrefab);
                    bullets.Add(bulletId, view);
                }

                bullets[bulletId].UpdateView(
                    bulletState.x,
                    bulletState.y);
            }

            List<string> bulletsToRemove = new();

            foreach (var pair in bullets)
            {
                if (!state.bullets.ContainsKey(pair.Key))
                {
                    bulletsToRemove.Add(pair.Key);
                }
            }

            foreach (string bulletId in bulletsToRemove)
            {
                Destroy(bullets[bulletId].gameObject);
                bullets.Remove(bulletId);
            }
        }
    }
}