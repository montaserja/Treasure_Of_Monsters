using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Sync : NetworkBehaviour
{
    
    public int team;
    public int playerPrefab;

    private void Start()
    {
        if (!isLocalPlayer)
            return;

        team = 0;


    }

    //all the players send the team number to rest of the players
    private void Update()
    {

        Cmdteam();
        if (isServer)
        {
            Rpcteam(team);
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
}
