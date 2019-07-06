using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*script for first player when taking the powerups sending to the second player */

public class Collect_PowerUp : NetworkBehaviour {
    public int Water = 0,Elictricity=0,Fire=0,Health=0;
    public int WaterD = 0, ElictricityD = 0, FireD = 0;
    public int[] guns;
    private bool takeAble = false;
	// Use this for initialization
	void Start () {
        if(isLocalPlayer)
        guns = new int[4];
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
            return; 
    }

    void OnTriggerEnter(Collider collider)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (collider.tag == "ElictricPowerUp")
        {
            //guns[Elictricity]++;
            Elictricity++;
        }
        else if (collider.tag == "FirePowerUp")
        {
            // guns[Fire]++;
            Fire++;
        }
        else if (collider.tag == "HealthPowerUp")
        {
            //guns[Health]++;
            Health++;
        }
        else if (collider.tag == "WaterPoerUp")
        {
            //guns[Water]++;
            Water++;
        }
        else if (collider.tag == "WaterWallPower")
        {
            //guns[Water]++;
            WaterD++;
        }
        else if (collider.tag == "ElictricWallPower")
        {
            //guns[Water]++;
            ElictricityD++;
        }
        else if (collider.tag == "FireWallPower")
        {
            //guns[Water]++;
            FireD++;
        }

        if (isServer)
        {
            RpcTakedPowerUp(Water, Elictricity, Fire, Health,WaterD,ElictricityD,FireD);
        }
        else
        {
            CmdTakedPowerUp(Water, Elictricity, Fire, Health,WaterD,ElictricityD,FireD);
        }
    }

    public int[] getList()
    {
        return guns;
    }

    [Command]
    void CmdTakedPowerUp(int Water, int Elictricity, int Fire, int Health,int WaterD,int ElicricityD,int FireD)
    {
        this.Elictricity = Elictricity;
        this.Fire = Fire;
        this.Water = Water;
        this.Health = Health;
        this.WaterD = WaterD;
        this.ElictricityD = ElicricityD;
        this.FireD = FireD;
        RpcTakedPowerUp( Water, Elictricity, Fire, Health,WaterD,ElictricityD,FireD);
    }

    [ClientRpc]
    void RpcTakedPowerUp(int Water, int  Elictricity, int Fire, int Health, int WaterD, int ElicricityD, int FireD)
    {
        this.Elictricity = Elictricity;
        this.Fire = Fire;
        this.Water = Water;
        this.Health = Health;
        this.WaterD = WaterD;
        this.ElictricityD = ElictricityD;
        this.FireD = FireD;
        //this.guns = guns;
    }

}
