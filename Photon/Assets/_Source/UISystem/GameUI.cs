using Mirror;
using PlayerSystem;
using RoundSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class GameUI : MonoBehaviour
    {
        [Header("Player UI")]
        [SerializeField] private TMP_Text healthText;

        [Header("Teams Players UI")]
        [SerializeField] private TMP_Text purplePlayersText;
        [SerializeField] private TMP_Text yellowPlayersText;

        [Header("Teams Score UI")]
        [SerializeField] private TMP_Text purpleScoreText;
        [SerializeField] private TMP_Text yellowScoreText;

        [Header("Round UI")]
        [SerializeField] private TMP_Text roundText;

        [Header("Winner Banner UI")]
        [SerializeField] private GameObject winnerBannerPanel;
        [SerializeField] private TMP_Text winnerBannerText;

        [Header("Winner Banner Colors")]
        [SerializeField] private Color purpleWinnerColor;
        [SerializeField] private Color yellowWinnerColor;

        private MirrorPlayer localPlayer;
        private Image winnerBannerPanelImage;

        private void Awake()
        {
            CacheWinnerBannerImage();
        }

        private void Start()
        {
            SetPanelActive(winnerBannerPanel, false);
        }

        private void Update()
        {
            FindLocalPlayerIfNeeded();

            UpdateHealthText();
            UpdateTeamsPlayersText();
            UpdateTeamsScoreText();
            UpdateRoundText();
            UpdateWinnerBanner();
        }

        private void CacheWinnerBannerImage()
        {
            if (winnerBannerPanel == null)
            {
                return;
            }

            winnerBannerPanelImage = winnerBannerPanel.GetComponent<Image>();

            if (winnerBannerPanelImage == null)
            {
                winnerBannerPanelImage = winnerBannerPanel.GetComponentInChildren<Image>(true);
            }

            if (winnerBannerPanelImage == null)
            {
                Debug.LogError("WinnerBannerPanel does not have Image component");
            }
        }

        private void FindLocalPlayerIfNeeded()
        {
            if (localPlayer != null)
            {
                return;
            }

            if (NetworkClient.localPlayer == null)
            {
                return;
            }

            localPlayer = NetworkClient.localPlayer.GetComponent<MirrorPlayer>();
        }

        private void UpdateHealthText()
        {
            if (healthText == null)
            {
                return;
            }

            if (localPlayer == null)
            {
                healthText.text = " ";
                return;
            }

            healthText.text = " " + localPlayer.CurrentHealth;
        }

        private void UpdateTeamsPlayersText()
        {
            RoundManager roundManager = RoundManager.Instance;

            if (roundManager == null)
            {
                SetText(purplePlayersText, "0");
                SetText(yellowPlayersText, "0");
                return;
            }

            SetText(purplePlayersText, "PURPLE: " + roundManager.PurplePlayersCount);
            SetText(yellowPlayersText, "YELLOW: " + roundManager.YellowPlayersCount);
        }

        private void UpdateTeamsScoreText()
        {
            RoundManager roundManager = RoundManager.Instance;

            if (roundManager == null)
            {
                SetText(purpleScoreText, "0");
                SetText(yellowScoreText, "0");
                return;
            }

            SetText(purpleScoreText, roundManager.PurpleScore.ToString());
            SetText(yellowScoreText, roundManager.YellowScore.ToString());
        }

        private void UpdateRoundText()
        {
            RoundManager roundManager = RoundManager.Instance;

            if (roundManager == null)
            {
                SetText(roundText, "ROUND: -");
                return;
            }

            SetText(roundText, "ROUND: " + roundManager.CurrentRound);
        }

        private void UpdateWinnerBanner()
        {
            RoundManager roundManager = RoundManager.Instance;

            if (roundManager == null)
            {
                HideWinnerBanner();
                return;
            }

            if (string.IsNullOrEmpty(roundManager.MatchWinnerText) == false)
            {
                ShowWinnerBanner(roundManager.MatchWinnerText);
                return;
            }

            if (string.IsNullOrEmpty(roundManager.RoundWinnerText) == false)
            {
                ShowWinnerBanner(roundManager.RoundWinnerText);
                return;
            }

            HideWinnerBanner();
        }

        private void ShowWinnerBanner(string message)
        {
            SetWinnerBannerPanelColor(message);
            SetText(winnerBannerText, message);
            SetPanelActive(winnerBannerPanel, true);
        }

        private void HideWinnerBanner()
        {
            SetPanelActive(winnerBannerPanel, false);
            SetText(winnerBannerText, string.Empty);
        }

        private void SetWinnerBannerPanelColor(string message)
        {
            if (winnerBannerPanelImage == null)
            {
                CacheWinnerBannerImage();
            }

            if (winnerBannerPanelImage == null)
            {
                return;
            }

            if (message.Contains("PURPLE"))
            {
                winnerBannerPanelImage.color = purpleWinnerColor;
                return;
            }

            if (message.Contains("YELLOW"))
            {
                winnerBannerPanelImage.color = yellowWinnerColor;
            }
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text == null)
            {
                return;
            }

            text.text = value;
        }

        private void SetPanelActive(GameObject panel, bool value)
        {
            if (panel == null)
            {
                return;
            }

            if (panel.activeSelf == value)
            {
                return;
            }

            panel.SetActive(value);
        }
    }
}