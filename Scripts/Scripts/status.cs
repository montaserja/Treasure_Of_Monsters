using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class status :  NetworkBehaviour{
    public Text GameOver;
    private Animator anim;
    public bool hasKey;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        hasKey = false;
	}
	
	// Update is called once per frame
	void Update () {

    }
    void OnTriggerStay(Collider obj)
    {
        if (obj.gameObject.tag == "treasure" && Input.GetKeyUp(KeyCode.E) && hasKey)//if the player have the key of the treasure
        {
			//open the treasure
            obj.GetComponent<treasure_box>().close=false;
            Vector3 direction = obj.transform.position - this.transform.position;
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), 1f);
            anim.SetTrigger("open_tresure");
            Debug.Log("winner");
            if (isServer)
            {
                RpcGameOver(this.gameObject);
            }
            else
            {
                CmdGameOver(this.gameObject);
            }
        }
    }

    [Command] void CmdGameOver(GameObject winner)
    {
        RpcGameOver(winner);
    }

    [ClientRpc]void RpcGameOver(GameObject winner)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<AimBehaviourBasic>().enabled = false;
            players[i].GetComponent<MoveBehaviour>().enabled = false;
            players[i].GetComponent<PlayerAttacks>().enabled = false;
            players[i].GetComponent<PlayerHealth>().enabled = false;
            if (players[i] != winner)
            {
                players[i].GetComponent<status>().GameOver.text = "You Lose ):";
            }
            else if (players[i] == winner)
            {
                players[i].GetComponent<status>().GameOver.text = "You Win";
            }
        }
    }
}
