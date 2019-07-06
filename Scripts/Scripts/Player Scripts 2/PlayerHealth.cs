using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int playerKills = 0;
    [SyncVar]
    public int BigMonstersKills = 0;
    [SyncVar]
    public int SmallMonstersKills = 0;
    [SyncVar]
    public float health = 100;

    public bool death = false;
    private NetworkAnimator anim;
    public Text PlayerskilledReference;
    public Text SmallMontsersKillsReference;
    public Text BigMonsterskilledReference;
    public Text hints;
    public bool hasDoorKey;
    public Text fpsText;
    float deltaTime;
    // Use this for initialization
    void Start()
    {
        anim = GetComponent<NetworkAnimator>();
        InvokeRepeating("updateText", 1f, 1f);
        hasDoorKey = false;
    }

    public void updateText()
    {
        PlayerskilledReference.text = "Kill " + playerKills;
        SmallMontsersKillsReference.text = "Small Monsters  " + SmallMonstersKills;
        BigMonsterskilledReference.text = "Big Monsters " + BigMonstersKills;

        if (playerKills < 1 || SmallMonstersKills < 4)
        {
            hints.text = "Kill "+ (1 - playerKills) +" player and"+ (4-SmallMonstersKills) +"small monsters to get houses key";
        }
        else if (BigMonstersKills < 2 )
        {
            hints.text = "Kill "+ (2 - BigMonstersKills) +" big monsters to get the treasure key";
        }
        else
        {
            this.GetComponent<status>().hasKey = true;
            hints.text = "you have the keys";
        }
         deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = "fbs: "+Mathf.Ceil(fps).ToString();

    }
    // Update is called once per frame
    bool canDie = true;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            playerKills = 1;
            SmallMonstersKills = 4;
            BigMonstersKills = 2; 
        }

            if (Input.GetKeyDown(KeyCode.L))
        {
            health = 0;
            anim.SetTrigger("Death");
            this.GetComponent<CapsuleCollider>().direction = 0;
        }

        //die on all clients
        if(canDie && death)
        {
            canDie = false;
            Invoke("returnDeath", 10);
            anim.SetTrigger("Death");
        }


        if (health <= 0)
        {
            death = true;
        }
        else
        {
            death = false;
        }


    }
    public void returnDeath()
    {
        canDie = true;
    }

    [Command]
    void CmdDeath(bool death)
    {
        this.death = death;
        RpcDeath(death);
    }

    [ClientRpc]
    void RpcDeath(bool death)
    {
        this.death = death;
    }

    void OnTriggerStay(Collider other)
    {
        if (death)
        {

            //anim.SetTrigger("Death");
            this.GetComponent<CapsuleCollider>().direction = 0;
        }
        if (other.gameObject.tag == "Wolf")
        {
            health -= 0.1f;
        }

        if (other.gameObject.tag == "Goblin")
        {
            health -= 0.1f;
        }

        if (other.gameObject.tag == "ORC")
        {
            health -= 0.1f;
        }



    }

    private void OnParticleCollision(GameObject collision)
    {
        if (death)
        {

           // anim.SetTrigger("Death");
            this.GetComponent<CapsuleCollider>().direction = 0;
        }

        if (collision.tag == "FireBall" && !death)
        {
            health -= 5;
        }

        else if (collision.tag == "Electric_spider" && !death)
        {
            health -= 5;
        }
        else if (collision.tag == "golemWaterAttack" && !death)
        {
            health -= 0.5f;
        }

    }
    public bool isDeath()
    {
        return death;
    }

    public void dealDamage(int attacktype, GameObject damager)
    {
        if (!death && health > 0)
        {
            this.health -= 5;

            if (health <= 0)
            {
                PlayerHealth PH = damager.GetComponent<PlayerHealth>();
                if (!PH)
                    return;
                PH.playerKills++;
            }
        }
    }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.GetComponent<door>() != null && playerKills >= 1 && SmallMonstersKills >= 4)
        {
            obj.GetComponent<door>().hasKey = true;
            hasDoorKey = true;
        }
    }

}