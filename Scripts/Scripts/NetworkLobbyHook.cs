using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class NetworkLobbyHook : LobbyHook {
    private LobbyPlayer lobby;
    private GameObject player;
    private NetworkManager myManager;

public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager,GameObject lobbyPlayer,GameObject gamePlayer)
    {
     lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        this.player = gamePlayer;
        
        this.myManager = manager;
       
        

        // Debug.Log(lobby.playerName);
    }

    




    public string getName()
    {
        return "";
       // Debug.Log(lobby.playerName);
      //  return lobby.playerName;
    }

    public string GetColor()
    {
        return ColorUtility.ToHtmlStringRGBA(lobby.playerColor);
    }
}
