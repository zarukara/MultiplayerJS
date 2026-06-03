using Mirror;
using PlayerSystem;
using RoundSystem;
using TMPro;
using UnityEngine;

namespace UISystem
{
    public class GameUI : MonoBehaviour
    {
        [Header("Player UI")]
        [SerializeField] private TMP_Text healthText;

        [Header("Teams Players UI")]
        [SerializeField] private TMP_Text redPlayersText;
        [SerializeField] private TMP_Text bluePlayersText;

        [Header("Teams Score UI")]
        [SerializeField] private TMP_Text redScoreText;
        [SerializeField] private TMP_Text blueScoreText;

        private MirrorPlayer localPlayer;

        private void Update()
        {
            FindLocalPlayerIfNeeded();

            UpdateHealthText();
            UpdateTeamsPlayersText();
            UpdateTeamsScoreText();
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

            localPlayer =
                NetworkClient.localPlayer.GetComponent<MirrorPlayer>();
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
                SetText(redPlayersText, "0");
                SetText(bluePlayersText, "0");
                return;
            }

            SetText(redPlayersText, roundManager.RedPlayersCount.ToString());
            SetText(bluePlayersText, roundManager.BluePlayersCount.ToString());
        }

        private void UpdateTeamsScoreText()
        {
            RoundManager roundManager = RoundManager.Instance;

            if (roundManager == null)
            {
                SetText(redScoreText, "0");
                SetText(blueScoreText, "0");
                return;
            }

            SetText(redScoreText, roundManager.RedScore.ToString());
            SetText(blueScoreText, roundManager.BlueScore.ToString());
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text == null)
            {
                return;
            }

            text.text = value;
        }
    }
}