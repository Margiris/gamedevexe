using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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

            if (StaticsConfig.IsServer)
                StartGameServer();
            else
                ConnectToServer();
        }

        public void StartGameServer()
        {
            var arguments = Environment.GetCommandLineArgs();

            if (arguments.Length < 2) return;

            ipInput.text = (int.Parse(arguments[1]) - StaticsConfig.PORT_OFFSET).ToString();
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
            webSocket = new WebSocket(new Uri("ws://" + StaticsConfig.SERVER_IP + ":" + StaticsConfig.SERVER_PORT));
            webSocket.Connect();
        }

        public void OnClickHost()
        {
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            StartCoroutine(IsPortAvailable(!IsServerRunning));
            StartCoroutine(WaitForGameServerStartAndJoin());
        }

        private IEnumerator WaitForGameServerStartAndJoin()
        {
            lobbyManager.SetServerInfo("Connecting...", ipInput.text);

            int port = int.Parse(ipInput.text) + StaticsConfig.PORT_OFFSET;

            while (!IsServerRunning)
            {
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(IsPortAvailable());
            }

            lobbyManager.ChangeTo(lobbyPanel);
            lobbyManager.networkAddress = StaticsConfig.SERVER_IP;
            lobbyManager.networkPort = port;

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();
            lobbyManager.StartClient();
        }

        public void OnClickDedicated()
        {
            lobbyManager.ChangeTo(null);
            lobbyManager.networkPort = int.Parse(ipInput.text) + StaticsConfig.PORT_OFFSET;
            lobbyManager.StartServer();

            lobbyManager.backDelegate = lobbyManager.StopServerClbk;

            lobbyManager.SetServerInfo("Dedicated Server", ipInput.text);
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
            if (StaticsConfig.IsServer) return;

            joinButton.interactable = false;
            StartCoroutine(IsPortAvailable());
        }

        public IEnumerator IsPortAvailable(bool requestingServerStart = false)
        {
            bool available = false;
            int port;
            if (!int.TryParse(ipInput.text, out port)) yield break;

            Debug.Log("sending message " + requestingServerStart + "\t" + (port + StaticsConfig.PORT_OFFSET));
            // send message
            webSocket.SendString(requestingServerStart + "\t" + (port + StaticsConfig.PORT_OFFSET));

            float startTime = Time.time;
            float elapsedTime = 0;

            // read response
            string message = webSocket.RecvString();

            // wait for message to not be empty until timeout
            while (message == null && elapsedTime < StaticsConfig.RESPONSE_TIMEOUT)
            {
                yield return new WaitForSeconds(0.5f);
                elapsedTime = Time.time - startTime;
                message = webSocket.RecvString();
            }

            // break if not received message in time
            if (message == null)
                yield break;

            PortData data = JsonUtility.FromJson<PortData>(message);

            portErrorMessage.text = StaticsConfig.PortErrorMessages[data.result];
            Debug.Log(
                string.Format("response was {0}, port {1} will be used.", data.result, data.suggestedPort));
            switch (data.result)
            {
                case 0:
                    IsServerRunning = false;
                    available = true;
                    break;
                case 1:
                    IsServerRunning = true;
                    available = true;
                    break;
                case 2:
                    IsServerRunning = false;
                    portErrorMessage.text +=
                        ", suggested ID: " + (data.suggestedPort - StaticsConfig.PORT_OFFSET);
                    available = false;
                    break;
            }


            if (webSocket.error != null)
                Debug.LogError("Error: " + webSocket.error);

            joinButton.interactable = available;
        }
    }
}