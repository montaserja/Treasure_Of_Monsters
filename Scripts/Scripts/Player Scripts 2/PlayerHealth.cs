using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour
{
    public float health = 100;
    public bool death = false;
    private NetworkAnimator anim;

    // Use this for initialization
    void Start () {
        anim = GetComponent<NetworkAnimator>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.L)){
            health = 0;
            anim.SetTrigger("Death");
            this.GetComponent<CapsuleCollider>().direction = 0;
        }

        if (!isLocalPlayer)
            return;

        if (health <= 0)
        {
            death = true;
        }

        if (isServer)
        {
            RpcDeath(death);
        }
        else
        {
            CmdDeath(death);
        }

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


    private void OnParticleCollision(GameObject collision)
    {
        if (death)
        {
           
            anim.SetTrigger("Death");
            this.GetComponent<CapsuleCollider>().direction = 0;
        }

        if (collision.tag == "FireBall" && !death)
        {
                health-=5;
        }
        else if (collision.tag == "PlayerFireBall" && !death)
        {
                health -= 5;
        }
    }
}
