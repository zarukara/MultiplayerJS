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

        [SyncVar] private int redScore;
        [SyncVar] private int blueScore;
        [SyncVar] private int currentRound;
        [SyncVar] private bool roundActive;
        [SyncVar] private bool matchFinished;

        [SyncVar] private int redPlayersCount;
        [SyncVar] private int bluePlayersCount;

        [SyncVar] private string roundWinnerText;
        [SyncVar] private string matchWinnerText;

        private readonly List<MirrorPlayer> players = new List<MirrorPlayer>();

        public int RedScore => redScore;
        public int BlueScore => blueScore;
        public int CurrentRound => currentRound;
        public bool RoundActive => roundActive;
        public bool MatchFinished => matchFinished;

        public int RedPlayersCount => redPlayersCount;
        public int BluePlayersCount => bluePlayersCount;

        public string RoundWinnerText => roundWinnerText;
        public string MatchWinnerText => matchWinnerText;

        private void Awake()
        {
            Instance = this;
        }

        public override void OnStartServer()
        {
            redScore = 0;
            blueScore = 0;
            currentRound = 0;
            roundActive = false;
            matchFinished = false;

            redPlayersCount = 0;
            bluePlayersCount = 0;

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
            redPlayersCount = 0;
            bluePlayersCount = 0;

            foreach (MirrorPlayer player in players)
            {
                if (player == null)
                {
                    continue;
                }

                if (player.Team == TeamType.Red)
                {
                    redPlayersCount++;
                }
                else if (player.Team == TeamType.Blue)
                {
                    bluePlayersCount++;
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

            int redAlive = 0;
            int blueAlive = 0;
            int redPlayers = 0;
            int bluePlayers = 0;

            foreach (MirrorPlayer player in players)
            {
                if (player == null)
                {
                    continue;
                }

                if (player.Team == TeamType.Red)
                {
                    redPlayers++;

                    if (!player.IsDead)
                    {
                        redAlive++;
                    }
                }

                if (player.Team == TeamType.Blue)
                {
                    bluePlayers++;

                    if (!player.IsDead)
                    {
                        blueAlive++;
                    }
                }
            }

            if (redPlayers == 0 || bluePlayers == 0)
            {
                return;
            }

            if (redAlive <= 0 && blueAlive > 0)
            {
                ServerEndRound(TeamType.Blue);
            }
            else if (blueAlive <= 0 && redAlive > 0)
            {
                ServerEndRound(TeamType.Red);
            }
        }

        [Server]
        public Transform GetSpawnTransform(TeamType team)
        {
            TeamSpawnPoint spawnPoint = GetSpawnPoint(team);

            if (spawnPoint == null)
            {
                return null;
            }

            return spawnPoint.transform;
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

            if (winnerTeam == TeamType.Red)
            {
                redScore++;
                roundWinnerText = "Red team wins round";
            }
            else
            {
                blueScore++;
                roundWinnerText = "Blue team wins round";
            }

            Debug.Log(roundWinnerText);

            if (redScore >= roundsToWin)
            {
                ServerEndMatch(TeamType.Red);
                return;
            }

            if (blueScore >= roundsToWin)
            {
                ServerEndMatch(TeamType.Blue);
                return;
            }

            StartCoroutine(StartNewRoundRoutine(nextRoundDelay));
        }

        [Server]
        private void ServerEndMatch(TeamType winnerTeam)
        {
            matchFinished = true;
            roundActive = false;

            matchWinnerText = winnerTeam == TeamType.Red
                ? "Red team wins match"
                : "Blue team wins match";

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

                TeamSpawnPoint spawnPoint = GetSpawnPoint(player.Team);

                if (spawnPoint == null)
                {
                    player.Respawn(Vector3.zero, Quaternion.identity);
                    continue;
                }

                player.Respawn(
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation);
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