using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace NetworkSystem
{
    public class NetworkClient : MonoBehaviour
    {
        [SerializeField] private string serverIp = "127.0.0.1";
        [SerializeField] private int serverPort = 7777;

        public string PlayerId { get; private set; }

        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;

        private float inputX;
        private float inputY;

        private void Awake()
        {
            PlayerId = Guid.NewGuid().ToString();

            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(
                IPAddress.Parse(serverIp),
                serverPort);

            Debug.Log("Player ID: " + PlayerId);
        }

        private void Update()
        {
            ReadInput();
            SendInput();
        }

        private void ReadInput()
        {
            inputX = Input.GetAxisRaw("Horizontal");
            inputY = Input.GetAxisRaw("Vertical");

            Vector2 input = new Vector2(inputX, inputY);

            if (input.magnitude > 1)
            {
                input.Normalize();
                inputX = input.x;
                inputY = input.y;
            }
        }

        private void SendInput()
        {
            ClientInputMessage message = new ClientInputMessage
            {
                type = "input",
                id = PlayerId,
                inputX = inputX,
                inputY = inputY
            };

            SendMessageToServer(message);
        }

        private void SendMessageToServer(object message)
        {
            string json = JsonUtility.ToJson(message);
            byte[] data = Encoding.UTF8.GetBytes(json);

            udpClient.Send(data, data.Length, serverEndPoint);
        }

        private void OnApplicationQuit()
        {
            udpClient?.Close();
        }
    }
}