using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace NetworkSystem
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector3 Direction;
    }

    public class FusionRoomManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private string roomName = "TestRoom";
        [SerializeField] private int maxPlayers = 2;
        [SerializeField] private NetworkPrefabRef playerPrefab;

        private NetworkRunner _runner;

        private readonly Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();

        private async void Start()
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);

            StartGameResult result = await _runner.StartGame(
                new StartGameArgs()
                {
                    GameMode = GameMode.AutoHostOrClient,
                    SessionName = roomName,
                    PlayerCount = maxPlayers
                });

            if (result.Ok)
            {
                Debug.Log("Connected to room");
            }
            else
            {
                Debug.Log("Failed to connect to room: " + result.ShutdownReason);
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("Player joined: " + player);

            if (!runner.IsServer)
            {
                return;
            }

            Vector3 spawnPosition = GetSpawnPosition(player);

            NetworkObject spawnedPlayer = runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player);

            _spawnedPlayers[player] = spawnedPlayer;

            if (_spawnedPlayers.Count >= maxPlayers)
            {
                Debug.Log("Max players reached");
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer)
            {
                return;
            }

            if (_spawnedPlayers.TryGetValue(player, out NetworkObject spawnedPlayer))
            {
                runner.Despawn(spawnedPlayer);
                _spawnedPlayers.Remove(player);
            }
        }

        private Vector3 GetSpawnPosition(PlayerRef player)
        {
            int index = player.RawEncoded % maxPlayers;

            return new Vector3(index * 3f, 1f, 0f);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData data = new NetworkInputData();

            if (Input.GetKey(KeyCode.W))
            {
                data.Direction += Vector3.forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                data.Direction += Vector3.back;
            }

            if (Input.GetKey(KeyCode.A))
            {
                data.Direction += Vector3.left;
            }

            if (Input.GetKey(KeyCode.D))
            {
                data.Direction += Vector3.right;
            }

            input.Set(data);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log("Disconnected from server");
        }

        public void OnConnectRequest(
            NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(
            NetworkRunner runner,
            NetAddress remoteAddress,
            NetConnectFailedReason reason)
        {
            Debug.Log("Failed to connect to server");
        }

        public void OnUserSimulationMessage(
            NetworkRunner runner,
            SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(
            NetworkRunner runner,
            List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(
            NetworkRunner runner,
            Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(
            NetworkRunner runner,
            HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(
            NetworkRunner runner,
            PlayerRef player,
            ReliableKey key,
            float progress)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnInputMissing(
            NetworkRunner runner,
            PlayerRef player,
            NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Connected to server");
        }
    }
}