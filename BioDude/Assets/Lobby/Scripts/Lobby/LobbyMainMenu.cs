using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Prototype.NetworkLobby
{
    [Serializable]
    public class PortData
    {
        public int result;
        public int port;
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

        private WebSocket w;

        public void OnEnable()
        {
            joinButton.gameObject.SetActive(!StaticsConfig.IsServer);
            dedicatedServerStartButton.gameObject.SetActive(StaticsConfig.IsServer);

            joinButton.enabled = false;

            lobbyManager.topPanel.ToggleVisibility(true);

            ipInput.onEndEdit.RemoveAllListeners();
            ipInput.onEndEdit.AddListener(onEndEditIP);

            matchNameInput.onEndEdit.RemoveAllListeners();
            matchNameInput.onEndEdit.AddListener(onEndEditGameName);

            Init();
        }

        IEnumerable Init()
        {
            w = new WebSocket(new Uri("ws://localhost:8000"));
            yield return StartCoroutine(w.Connect());
            Debug.Log("CONNECTED TO WEBSOCKETS");
        }

        public void OnClickHost()
        {
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            if (!IsPortAvailable(true)) return;

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
            lobbyManager.networkPort = int.Parse(ipInput.text);
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

        public bool IsPortAvailable(bool requestingServerStart = false)
        {
            int port = int.Parse(ipInput.text);

            // send message
            w.SendString(requestingServerStart + "\t" + port + "\t" + StaticsConfig.PORT_OFFSET);
            Debug.Log(requestingServerStart + "\t" + port + "\t" + StaticsConfig.PORT_OFFSET);

            // read response
            string message = w.RecvString();

            // check if message is not empty
            if (message != null)
            {
                PortData data = JsonUtility.FromJson<PortData>(message);

                portErrorMessage.text = StaticsConfig.PortErrorMessages[data.result];

                switch (data.result)
                {
                    case 0:
                        joinButton.enabled = true;
                        return true;
                    case 1:
                        portErrorMessage.text += ", suggested ID: " + data.port;
                        joinButton.enabled = false;
                        return false;
                    default:
                        joinButton.enabled = false;
                        return false;
                }
            }

            if (w.error != null)
                Debug.LogError("Error: " + w.error);

            return false;
        }
    }
}