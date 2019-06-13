using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace Prototype.NetworkLobby
{
    [Serializable]
    public class PortData
    {
        /* result values:
            0 - port not occupied and server not running on that port
            1 - port in use by BioDude server
            2 - port in use by another process
        */
        public int result;
        public int suggestedPort;
    }

    //Main menu, mainly only a bunch of callback called by the UI (setup throught the Inspector)
    public class LobbyMainMenu : MonoBehaviour
    {
        public LobbyManager lobbyManager;

        public RectTransform lobbyServerList;
        public RectTransform lobbyPanel;

        public InputField ipInput;
        public InputField matchNameInput;

        public Button joinButton;
        public Button dedicatedServerStartButton;

        public Text portErrorMessage;

        private bool IsServerRunning;

        private WebSocket webSocket;
        Queue<byte[]> socketMessages = new Queue<byte[]>();
        bool socketIsConnected;
        string socketError;

        private bool initialized;

        public void OnEnable()
        {
            joinButton.gameObject.SetActive(!StaticsConfig.IsServer);
            dedicatedServerStartButton.gameObject.SetActive(StaticsConfig.IsServer);

            joinButton.interactable = false;

            lobbyManager.topPanel.ToggleVisibility(true);

            ipInput.onEndEdit.RemoveAllListeners();
            ipInput.onEndEdit.AddListener(onEndEditIP);

            matchNameInput.onEndEdit.RemoveAllListeners();
            matchNameInput.onEndEdit.AddListener(onEndEditGameName);

#if !UNITY_WEBGL
            StartGameServer();
#else
            ConnectToServer();
#endif
        }

        public void StartGameServer()
        {
            var arguments = Environment.GetCommandLineArgs();

            if (arguments.Length < 2) return;

            ipInput.text = arguments[1];
            lobbyManager.backButton.interactable = false;
            initialized = true;
        }

        public void Update()
        {
            if (!initialized) return;

            OnClickDedicated();
            initialized = false;
        }

        private void ConnectToServer()
        {
            webSocket = new WebSocket(new Uri("ws://localhost:" + StaticsConfig.SERVER_PORT));
            webSocket.Connect();
        }

        public void OnClickHost()
        {
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            if (!IsPortAvailable(!IsServerRunning)) return;

            lobbyManager.ChangeTo(lobbyPanel);

            lobbyManager.networkAddress = StaticsConfig.SERVER_IP;
            lobbyManager.networkPort = int.Parse(ipInput.text) + StaticsConfig.PORT_OFFSET;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        }

        public void OnClickDedicated()
        {
            lobbyManager.ChangeTo(null);
            lobbyManager.networkPort = int.Parse(ipInput.text) + StaticsConfig.PORT_OFFSET;
            lobbyManager.StartServer();

            lobbyManager.backDelegate = lobbyManager.StopServerClbk;

            lobbyManager.SetServerInfo("Dedicated Server", lobbyManager.networkAddress);
        }

        public void OnClickCreateMatchmakingGame()
        {
            lobbyManager.StartMatchMaker();
            lobbyManager.matchMaker.CreateMatch(
                matchNameInput.text,
                (uint) lobbyManager.maxPlayers,
                true,
                "", "", "", 0, 0,
                lobbyManager.OnMatchCreate);

            lobbyManager.backDelegate = lobbyManager.StopHost;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Matchmaker Host", lobbyManager.matchHost);
        }

        public void OnClickOpenServerList()
        {
            lobbyManager.StartMatchMaker();
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
            lobbyManager.ChangeTo(lobbyServerList);
        }

        void onEndEditIP(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickJoin();
            }
        }

        void onEndEditGameName(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickCreateMatchmakingGame();
            }
        }

        public void OnPortChanged()
        {
            if (!StaticsConfig.IsServer)
                joinButton.interactable = IsPortAvailable();
            Debug.Log(IsServerRunning);
        }

        public bool IsPortAvailable(bool requestingServerStart = false)
        {
            int port;
            if (!int.TryParse(ipInput.text, out port)) return false;

            Debug.Log("sending message " + requestingServerStart + "\t" + (port + StaticsConfig.PORT_OFFSET));
            // send message
            SendString(requestingServerStart + "\t" + (port + StaticsConfig.PORT_OFFSET));

            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed.TotalMilliseconds < StaticsConfig.RESPONSE_TIMEOUT)
            {
                // read response
                string message = ReceiveString();

                // check if message is not empty
                if (message != null)
                {
                    Debug.Log(message);
                    PortData data = JsonUtility.FromJson<PortData>(message);

                    portErrorMessage.text = StaticsConfig.PortErrorMessages[data.result];
                    Debug.Log(
                        string.Format("response was {0}, port {1} will be used.", data.result, data.suggestedPort));
                    switch (data.result)
                    {
                        case 0:
                            IsServerRunning = false;
                            return true;
                        case 1:
                            IsServerRunning = true;
                            return true;
                        case 2:
                            IsServerRunning = false;
                            portErrorMessage.text +=
                                ", suggested ID: " + (data.suggestedPort - StaticsConfig.PORT_OFFSET);
                            return false;
                    }
                }

                if (socketError != null)
                    Debug.LogError("Error: " + socketError);

                Thread.Sleep(StaticsConfig.RESPONSE_TIMEOUT / 100);
            }

            return false;
        }

        private void SendString(string str)
        {
            webSocket.Send(Encoding.UTF8.GetBytes(str));
        }

        public string ReceiveString()
        {
            return socketMessages.Count == 0 ? null : Encoding.UTF8.GetString(socketMessages.Dequeue());
        }
    }
}