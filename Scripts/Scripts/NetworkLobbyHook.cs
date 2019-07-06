using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class NetworkLobbyHook : LobbyHook {
    private LobbyPlayer lobby;
    private GameObject player;
    private NetworkManager myManager;
    public GameObject Player2;
    public GameObject Player1;

    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager,GameObject lobbyPlayer,GameObject gamePlayer)
    {
     lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        var conn = lobby.GetComponent<NetworkIdentity>().connectionToClient;
        respawn sync_team = gamePlayer.GetComponent<respawn>();
        createHouses create = gamePlayer.GetComponent<createHouses>();
        create.numOfPlayers = lobby.playersNum;
        sync_team.team = lobby.dropDown.value;
        if (lobby.playerPrefab == 0)
        {
            // Destroy(gamePlayer);
            //var player = Instantiate<GameObject>(Player2, Vector3.zero, transform.rotation);

           // player = null;
           // player = (GameObject)Instantiate(Player2, Vector3.zero, Quaternion.identity);
         
            sync_team.playerPrefab = 0;
           // Destroy(lobby.GetComponent<NetworkIdentity>().gameObject);
           // NetworkServer.ReplacePlayerForConnection(conn, player, 0);
            // NetworkServer.ReplacePlayerForConnection(gamePlayer.GetComponent<NetworkConnection>(), player, 0);
        }
        else
        {
            sync_team.playerPrefab  =1;
            // Destroy(gamePlayer);
            // player = null;
            /*  var player = Instantiate<GameObject>(Player1, Vector3.zero, transform.rotation);
             // player = (GameObject)Instantiate(Player1, Vector3.zero, Quaternion.identity);
              Player_Sync sync_team = player.GetComponent<Player_Sync>();
              sync_team.team = lobby.dropDown.value;
              sync_team.playerPrefab = 0;
              Destroy(lobby.GetComponent<NetworkIdentity>().gameObject);
              NetworkServer.ReplacePlayerForConnection(conn, player, 0);
              print("hii");*/
            //Destroy(gamePlayer.GetComponent<NetworkIdentity>().gameObject);
            // NetworkServer.ReplacePlayerForConnection(gamePlayer.GetComponent<NetworkIdentity>().connectionToClient, player, 0);
        }
        //Debug.Log(lobby.dropDown.value + "   prefab: "+ lobby.playerPrefab);
       // this.player = gamePlayer;
        
        
       // this.myManager = manager;
       
        

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
