using Mirror;
using PlayerSystem;
using RoundSystem;
using TeamSystem;
using UnityEngine;

namespace NetworkSystem
{
    public class GameNetworkManager : NetworkManager
    {
        private int connectedPlayersCount;

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            TeamType team = connectedPlayersCount % 2 == 0
                ? TeamType.Purple
                : TeamType.Yellow;

            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;

            if (RoundManager.Instance != null)
            {
                Transform spawnTransform =
                    RoundManager.Instance.GetSpawnTransform(team);

                if (spawnTransform != null)
                {
                    spawnPosition = spawnTransform.position;
                    spawnRotation = spawnTransform.rotation;

                    Debug.Log($"Spawn found for {team}: {spawnPosition}");
                }
                else
                {
                    Debug.LogError($"Spawn not found for team: {team}");
                }
            }
            else
            {
                Debug.LogError("RoundManager.Instance is null");
            }

            GameObject playerObject = Instantiate(
                playerPrefab,
                spawnPosition,
                spawnRotation);

            MirrorPlayer player = playerObject.GetComponent<MirrorPlayer>();

            player.ServerInitialize(team);

            connectedPlayersCount++;

            NetworkServer.AddPlayerForConnection(conn, playerObject);

            if (RoundManager.Instance != null)
            {
                RoundManager.Instance.ServerRegisterPlayer(player);
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                MirrorPlayer player =
                    conn.identity.GetComponent<MirrorPlayer>();

                if (player != null && RoundManager.Instance != null)
                {
                    RoundManager.Instance.ServerUnregisterPlayer(player);
                }
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            connectedPlayersCount = 0;

            base.OnStopServer();
        }
    }
}