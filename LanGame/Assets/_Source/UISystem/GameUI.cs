using NetworkSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private NetworkClient networkClient;

        [SerializeField] private TMP_Text hpText;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Button restartButton;

        private void OnEnable()
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDisable()
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        private void Update()
        {
            ServerStateMessage state = networkClient.LastState;

            if (state == null || state.players == null)
            {
                return;
            }

            if (!state.players.ContainsKey(networkClient.PlayerId))
            {
                return;
            }

            PlayerState localPlayer = state.players[networkClient.PlayerId];

            hpText.text = "" + localPlayer.hp;

            losePanel.SetActive(localPlayer.isDead);
        }

        private void OnRestartClicked()
        {
            networkClient.SendRestart();
        }
    }
}