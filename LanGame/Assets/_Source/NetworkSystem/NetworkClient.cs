using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace NetworkSystem
{
    public class NetworkClient : MonoBehaviour
    {
        [SerializeField] private string serverIp = "127.0.0.1";
        [SerializeField] private int serverPort = 7777;

        public string PlayerId { get; private set; }

        public ServerStateMessage LastState { get; private set; }

        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private Thread receiveThread;

        private bool isRunning;

        private float inputX;
        private float inputY;

        private void Awake()
        {
            PlayerId = Guid.NewGuid().ToString();

            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(
                IPAddress.Parse(serverIp),
                serverPort);

            isRunning = true;

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            receiveThread.Start();

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

        private void ReceiveLoop()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);

                    string json = Encoding.UTF8.GetString(data);

                    ServerStateMessage state =
                        JsonConvert.DeserializeObject<ServerStateMessage>(json);

                    LastState = state;
                }
                catch (SocketException)
                {
                    // сокет закрыт при выходе из игры
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public void SendShoot(float dirX, float dirY)
        {
            ClientShootMessage message = new ClientShootMessage
            {
                type = "shoot",
                id = PlayerId,
                dirX = dirX,
                dirY = dirY
            };

            SendMessageToServer(message);
        }

        public void SendRestart()
        {
            ClientRestartMessage message = new ClientRestartMessage
            {
                type = "restart",
                id = PlayerId
            };

            SendMessageToServer(message);
        }

        private void SendMessageToServer(object message)
        {
            string json = JsonConvert.SerializeObject(message);
            byte[] data = Encoding.UTF8.GetBytes(json);

            udpClient.Send(data, data.Length, serverEndPoint);
        }

        private void OnApplicationQuit()
        {
            isRunning = false;
            udpClient?.Close();
        }
    }
}