using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Wall_Health : NetworkBehaviour {
    private ParticleSystem ps;
    private float rate;
    private float ElectDamage,FireDamage,WaterDamage;
    // Use this for initialization
    void Start () {
        ps = GetComponent<ParticleSystem>();
        
        if (this.tag == "ElectricWall")//set the damage for the wall
        {
            ElectDamage = 1f;
            FireDamage = 10f;
            WaterDamage = 1f;
        }
        else if(this.tag == "FireWall")
        {
            ElectDamage = 1f;
            FireDamage = 1f;
            WaterDamage = 10f;
        }
        else if(this.tag == "WaterWall")
        {
            ElectDamage = 10f;
            FireDamage = 1f;
            WaterDamage = 1f;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnParticleCollision(GameObject other)//taking damage
    {
        getDamage(other);
        var emission = ps.emission;
        if (isServer)
        {
            RpcGetDamage(other);
            if (emission.rate.constant <= 0)
            {
                RpcDestroy();
                if (this.gameObject != null)
                {
                    Destroy(this.gameObject);
                }
            }
        }
        else
        {
            CmdGetDamage(other);
            RpcGetDamage(other);
            if (emission.rate.constant <= 0)
            {
                CmdDestroy();
                if (this.gameObject != null)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    [Command]
    void CmdDestroy()
    {
        Destroy(this.gameObject);
    }

    [ClientRpc]
    void RpcDestroy()
    {
        if (!isServer && !isLocalPlayer)
            Destroy(this.gameObject);
    }

    [Command]
    void CmdGetDamage(GameObject other)
    {
        getDamage(other);
        RpcGetDamage(other);
    }
    [ClientRpc]
    void RpcGetDamage(GameObject other)
    {
        if(!isServer&&!isLocalPlayer)
        getDamage(other);
    }

    void getDamage(GameObject other)
    {
        var emission = ps.emission;
        if (other.tag == "PlayerFireBall")
        {
            rate = emission.rate.constant - FireDamage;

        }
        else if (other.tag == "ElectricPlayer")
        {

            rate = emission.rate.constant - ElectDamage;
        }
        else if (other.tag == "WaterAttackPlayer")
        {
            rate = emission.rate.constant - WaterDamage;
        }
        emission.rateOverTime = rate;
        
    }

}
