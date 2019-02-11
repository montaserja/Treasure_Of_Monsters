using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Sync : NetworkBehaviour
{
    
    public int team;
    public int playerPrefab;
    public GameObject Player1;
    public GameObject Player0;
    public bool changed=false;
    public bool findTeamMate = false;
    private GameObject[] players1;
    private GameObject[] players2;
    private PlayerHealth health;
    public bool respawn = false;
    public bool defined = false;


    private void Start()
    {
        if (!isLocalPlayer)
            return;
    }

    //all the players send the team number to rest of the players
    private void Update()
    {
        //Sync the team and the player prefab to all of the clients
        Cmdteam();
        CmdPrefab();
        if (isServer)
        {
            Rpcteam(team);
            RpcPrefab(playerPrefab);
        }


        if (playerPrefab == 1)
        {
            //search for the player with the same team
            find();
        }else if (playerPrefab==0)
        {

            if (!defined) { 
            health = GetComponent<PlayerHealth>();
            }
            if (health!=null && health.death)
            {
                //if the player dies respawn him to the second prefab
                StartCoroutine(gg());
            }
          
        }

   

    }

    //fuc that the clients do in the server
    [Command]
    void Cmdteam()
    {
        this.team = team;
        Rpcteam(team);
    }

    //the server send the team to all of the players
    [ClientRpc]
  void  Rpcteam(int team)
    {
        this.team = team;
    }

    //fuc that the clients do in the server
    [Command]
    void CmdPrefab()
    {
        this.playerPrefab = playerPrefab;
        RpcPrefab(playerPrefab);
    }

    //the server send the team to all of the players
    [ClientRpc]
    void RpcPrefab(int playerPrefab)
    {
        this.playerPrefab = playerPrefab;
    }


    [Command]
    void CmdSyncTeamPos()
    {
    
        RpcSyncTeamPos(transform.position,team,playerPrefab);
    }

    [ClientRpc]
    void RpcSyncTeamPos(Vector3 pos,int team,int playerNum)
    {
        if (isLocalPlayer)
            return;
        // Debug.Log(playerNum + "    "+ playerPrefab);
        if (playerNum == 2) {
            Debug.Log(team + "    " + playerPrefab);
            if (this.team == team)
            {
                Vector3 temp = pos;
                temp.y = pos.y + 50f;
                this.transform.position = temp;
            }
        }
    }

    private void setTeam(int team)
    {
        this.team = team;
    }




    /**************************************************************************************/

    private void find()
    {
        if (!findTeamMate)
        {
            //get all the players in the game that thier tags is "Player"
            players1 = GameObject.FindGameObjectsWithTag("Player");
            Player_Sync[] s = new Player_Sync[players1.Length];
            for (int i = 0; i < players1.Length; i++)
                s[i] = players1[i].GetComponent<Player_Sync>();

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i].team == this.team)
                {
                    Debug.Log(s[i].team + " myteam : " +this.team);
                    Vector3 temp = players1[i].transform.position;
                    temp.y = temp.y + 5f;
                    temp.x = temp.x - 1f;
                    Player2Camera player2Cam = GetComponentInChildren<Player2Camera>();
                    if (player2Cam != null) { 
                        player2Cam.setplayer(players1[i].transform);
                        findTeamMate = true;
                        health = players1[i].GetComponent<PlayerHealth>();
                    }

                }
            }
        }
        else
        {
       

            if (health.death)
            {
                StartCoroutine(gg());
            }
          
        }
    }


    /************************************************************************************/
    public void Respawn()
    {
        var conn = GetComponent<NetworkIdentity>().connectionToClient;
        var playerToSpawn = Instantiate<GameObject>(Player0, Vector3.zero, transform.rotation); ;
        if (playerPrefab == 0)
        {
            Destroy(playerToSpawn);
            playerToSpawn = Instantiate<GameObject>(Player1, Vector3.zero, transform.rotation);
        }


        Player_Sync sync_team = playerToSpawn.GetComponent<Player_Sync>();

        if (playerPrefab == 0)
        {

            sync_team.playerPrefab = 1;
         
          
        }
        else if(playerPrefab == 1)
        {
            sync_team.playerPrefab = 0;
        }
        sync_team.team = team;
        sync_team.changed = true;
        // RpcSpawn(playerToSpawn,sync_team);
        Destroy(GetComponent<NetworkIdentity>().gameObject);
        NetworkServer.ReplacePlayerForConnection(conn, playerToSpawn, 0);
    }


    [Command]
    void CmdRespawn()
    {
        Respawn();
    }


    IEnumerator gg()
    {
        yield return new WaitForSecondsRealtime(7);
        if (isServer)
            Respawn();
        else
            CmdRespawn();

    }
}
