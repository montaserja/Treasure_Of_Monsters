using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class respawn : NetworkBehaviour {
    public int team;
    public int playerPrefab=-1;
    public GameObject Player1;
    public GameObject Player0;
    private createHouses create;
  


    // Use this for initialization
    void Start () {
        if (!isLocalPlayer)
            return;
        create = this.GetComponent<createHouses>();
    }
	
	// Update is called once per frame
	void Update () {
        Cmdteam();
        CmdPrefab();
        if (isServer)
        {
            Rpcteam(team);
            RpcPrefab(playerPrefab);
        }

        if (playerPrefab == 0 || playerPrefab==1)
        {
           // Debug.Log("Update " + "prefab " + playerPrefab + " team " + team );
        
            if (isServer && create!=null&& create.mapIsOk) {

                Spawn(playerPrefab);
               
            }
            else if(!isServer)
                CmdSpawn(playerPrefab);
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
    void Rpcteam(int team)
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

        RpcSyncTeamPos(transform.position, team, playerPrefab);
    }

    [ClientRpc]
    void RpcSyncTeamPos(Vector3 pos, int team, int playerNum)
    {
        if (isLocalPlayer)
            return;
        // Debug.Log(playerNum + "    "+ playerPrefab);
        if (playerNum == 2)
        {
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
    void Spawn(int prefab)
    {
    
        var conn = GetComponent<NetworkIdentity>().connectionToClient;
        var playerToSpawn= Instantiate<GameObject>(Player1, Vector3.zero, transform.rotation); ;
        if (prefab == 0) {
            Destroy(playerToSpawn);
             playerToSpawn = Instantiate<GameObject>(Player0, Vector3.zero, transform.rotation);
        }


        Player_Sync sync_team = playerToSpawn.GetComponent<Player_Sync>();
        sync_team.playerPrefab = playerPrefab;
        sync_team.team = team;
        sync_team.changed = true;
        // RpcSpawn(playerToSpawn,sync_team);
        Destroy(GetComponent<NetworkIdentity>().gameObject);
        NetworkServer.ReplacePlayerForConnection(conn, playerToSpawn, 0);

    }

    [ClientRpc]
    void RpcSpawn()
    {


    }



    [Command]
    public void CmdSpawn(int prefab)
    {
        Spawn(prefab);
        // RpcSpawn();

        // GameObject go = (GameObject)Instantiate(Player1, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        // Destroy(GetComponent<NetworkIdentity>().gameObject);
        // NetworkServer.SpawnWithClientAuthority(go, connectionToClient);
    }
}
