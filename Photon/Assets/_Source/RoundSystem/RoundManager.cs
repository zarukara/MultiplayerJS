using System.Collections;
using System.Collections.Generic;
using Mirror;
using PlayerSystem;
using SpawnSystem;
using TeamSystem;
using UnityEngine;

namespace RoundSystem
{
    public class RoundManager : NetworkBehaviour
    {
        public static RoundManager Instance { get; private set; }

        [Header("Round Settings")]
        [SerializeField] private int roundsToWin = 12;
        [SerializeField] private int minPlayersToStart = 2;
        [SerializeField] private float nextRoundDelay = 3f;

        [SyncVar] private int purpleScore;
        [SyncVar] private int yellowScore;
        [SyncVar] private int currentRound;
        [SyncVar] private bool roundActive;
        [SyncVar] private bool matchFinished;

        [SyncVar] private int purplePlayersCount;
        [SyncVar] private int yellowPlayersCount;

        [SyncVar] private string roundWinnerText;
        [SyncVar] private string matchWinnerText;

        private readonly List<MirrorPlayer> players = new List<MirrorPlayer>();

        public int PurpleScore => purpleScore;
        public int YellowScore => yellowScore;
        public int CurrentRound => currentRound;
        public bool RoundActive => roundActive;
        public bool MatchFinished => matchFinished;

        public int PurplePlayersCount => purplePlayersCount;
        public int YellowPlayersCount => yellowPlayersCount;

        public string RoundWinnerText => roundWinnerText;
        public string MatchWinnerText => matchWinnerText;

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartServer()
        {
            purpleScore = 0;
            yellowScore = 0;
            currentRound = 0;
            roundActive = false;
            matchFinished = false;

            purplePlayersCount = 0;
            yellowPlayersCount = 0;

            roundWinnerText = string.Empty;
            matchWinnerText = string.Empty;
        }

        [Server]
        public void ServerRegisterPlayer(MirrorPlayer player)
        {
            if (players.Contains(player))
            {
                return;
            }

            players.Add(player);

            UpdateTeamPlayersCount();

            Debug.Log("Player registered in RoundManager: " + player.netId);

            TryStartFirstRound();
        }

        [Server]
        public void ServerUnregisterPlayer(MirrorPlayer player)
        {
            if (players.Contains(player))
            {
                players.Remove(player);
            }

            UpdateTeamPlayersCount();

            if (roundActive)
            {
                ServerCheckRoundState();
            }
        }

        [Server]
        private void UpdateTeamPlayersCount()
        {
            purplePlayersCount = 0;
            yellowPlayersCount = 0;

            foreach (MirrorPlayer player in players)
            {
                if (player == null)
                {
                    continue;
                }

                if (player.Team == TeamType.Purple)
                {
                    purplePlayersCount++;
                }
                else if (player.Team == TeamType.Yellow)
                {
                    yellowPlayersCount++;
                }
            }
        }

        [Server]
        public void ServerCheckRoundState()
        {
            if (!roundActive)
            {
                return;
            }

            if (matchFinished)
            {
                return;
            }

            int purpleAlive = 0;
            int yellowAlive = 0;
            int purplePlayers = 0;
            int yellowPlayers = 0;

            foreach (MirrorPlayer player in players)
            {
                if (player == null)
                {
                    continue;
                }

                if (player.Team == TeamType.Purple)
                {
                    purplePlayers++;

                    if (!player.IsDead)
                    {
                        purpleAlive++;
                    }
                }

                if (player.Team == TeamType.Yellow)
                {
                    yellowPlayers++;

                    if (!player.IsDead)
                    {
                        yellowAlive++;
                    }
                }
            }

            if (purplePlayers == 0 || yellowPlayers == 0)
            {
                return;
            }

            if (purpleAlive <= 0 && yellowAlive > 0)
            {
                ServerEndRound(TeamType.Yellow);
            }
            else if (yellowAlive <= 0 && purpleAlive > 0)
            {
                ServerEndRound(TeamType.Purple);
            }
        }

        [Server]
        public Vector3 GetSpawnPosition(TeamType team)
        {
            TeamSpawnPoint spawnPoint = GetSpawnPoint(team);

            if (spawnPoint == null)
            {
                return Vector3.zero;
            }

            return spawnPoint.GetRandomSpawnPosition();
        }

        [Server]
        public Quaternion GetSpawnRotation(TeamType team)
        {
            TeamSpawnPoint spawnPoint = GetSpawnPoint(team);

            if (spawnPoint == null)
            {
                return Quaternion.identity;
            }

            return spawnPoint.GetSpawnRotation();
        }

        [Server]
        private void TryStartFirstRound()
        {
            if (roundActive)
            {
                return;
            }

            if (matchFinished)
            {
                return;
            }

            if (players.Count < minPlayersToStart)
            {
                return;
            }

            StartCoroutine(StartNewRoundRoutine(1f));
        }

        [Server]
        private void ServerEndRound(TeamType winnerTeam)
        {
            roundActive = false;

            if (winnerTeam == TeamType.Purple)
            {
                purpleScore++;
                roundWinnerText = "PURPLE TEAM WINS ROUND";
            }
            else
            {
                yellowScore++;
                roundWinnerText = "YELLOW TEAM WINS ROUND";
            }

            Debug.Log(roundWinnerText);

            if (purpleScore >= roundsToWin)
            {
                ServerEndMatch(TeamType.Purple);
                return;
            }

            if (yellowScore >= roundsToWin)
            {
                ServerEndMatch(TeamType.Yellow);
                return;
            }

            StartCoroutine(StartNewRoundRoutine(nextRoundDelay));
        }

        [Server]
        private void ServerEndMatch(TeamType winnerTeam)
        {
            matchFinished = true;
            roundActive = false;

            matchWinnerText = winnerTeam == TeamType.Purple
                ? "PURPLE TEAM WINS MATCH"
                : "YELLOW TEAM WINS MATCH";

            Debug.Log(matchWinnerText);
        }

        [Server]
        private IEnumerator StartNewRoundRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (matchFinished)
            {
                yield break;
            }

            ServerStartNewRound();
        }

        [Server]
        private void ServerStartNewRound()
        {
            currentRound++;
            roundActive = true;
            roundWinnerText = string.Empty;

            RespawnAllPlayers();

            Debug.Log("Round started: " + currentRound);
        }

        [Server]
        private void RespawnAllPlayers()
        {
            foreach (MirrorPlayer player in players)
            {
                if (player == null)
                {
                    continue;
                }

                Vector3 spawnPosition = GetSpawnPosition(player.Team);
                Quaternion spawnRotation = GetSpawnRotation(player.Team);

                player.Respawn(spawnPosition, spawnRotation);
            }
        }

        [Server]
        private TeamSpawnPoint GetSpawnPoint(TeamType team)
        {
            TeamSpawnPoint[] spawnPoints =
                FindObjectsOfType<TeamSpawnPoint>();

            List<TeamSpawnPoint> teamSpawns = new List<TeamSpawnPoint>();

            foreach (TeamSpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint.Team == team)
                {
                    teamSpawns.Add(spawnPoint);
                }
            }

            if (teamSpawns.Count == 0)
            {
                Debug.LogError("No spawn points for team: " + team);
                return null;
            }

            int index = Random.Range(0, teamSpawns.Count);

            return teamSpawns[index];
        }
    }
}