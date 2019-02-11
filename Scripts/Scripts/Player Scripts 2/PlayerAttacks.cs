// In this script we will make tha player attack types. 


using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;



public class PlayerAttacks : NetworkBehaviour
{

    private bool HasFirePower, HasElectricPower, HasWaterPower, canShoot = false;
    private NetworkAnimator anim;
    public Transform effect1;
    public Transform effect2;
    public Transform effect3;
    public GameObject ShootingPoint;
    public Camera cam;
    private PlayerHealth health;




    private void Start()
    {
        anim = GetComponent<NetworkAnimator>();
        health = GetComponent<PlayerHealth>();
        //transform.position = new Vector3(-80, 0.3f, 409);
    }
    void Update()
    {
        /*CmdDeath(death);
        if (isServer)
            RpcDeath(death);
            */


        if (!isLocalPlayer)
            return;
        /*************************************************************/
        if (Input.GetKeyDown(KeyCode.Mouse0) && HasFirePower && GetComponent<AimBehaviourBasic>().Aiming() && !GetComponent<PlayerHealth>().death)
        {
            //ShootingPoint.transform.position = cam.transform.position;
            ShootingPoint.transform.rotation = cam.transform.rotation;
            FireAttack(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
            //anim.SetTrigger(MyTags.FireballAttack_TRIGGER);
            CmdDoFire(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
            if (isServer)
            {
                RpcDoFire(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
            }
        }
        /*************************************************************/
        if (Input.GetKeyDown(KeyCode.Mouse0) && HasElectricPower && GetComponent<AimBehaviourBasic>().Aiming() && !GetComponent<PlayerHealth>().death)
        {
            ElectrecAttack();
            CmdDoElictric();
            if (isServer)
            {
                RpcDoElictric();
            }
        }
        /*************************************************************/
        if (Input.GetKeyDown(KeyCode.Mouse0) && HasWaterPower && GetComponent<AimBehaviourBasic>().Aiming() && !GetComponent<PlayerHealth>().death)
        {
            WaterAttack();
            CmdDoWater();
            if (isServer)
            {
                RpcDoWater();
            }
        }
        PlayerAttackType();

    }

    //in this method the player choos which attack type he want.
    void PlayerAttackType()
    {
        // pressing numper 1 will chosse the water attack
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            HasFirePower = false;
            HasElectricPower = false;
            HasWaterPower = true;

            canShoot = true;
        }
        // pressing numper 2 will chosse the electric attack
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            HasFirePower = false;
            HasElectricPower = true;
            HasWaterPower = false;

            canShoot = true;

        }
        // pressing numper 3 will chosse the fire attack
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            HasFirePower = true;
            HasElectricPower = false;
            HasWaterPower = false;
            canShoot = true;
        }


    }

    //this method make a spawn for the fireBall attack.
    void FireAttack(Vector3 from, Quaternion rotation)
    {
        Vector3 temp = ShootingPoint.transform.position;
        ShootingPoint.transform.position -= ((ShootingPoint.transform.position - cam.transform.position) / 10);

        GameObject bullet = (GameObject)Instantiate(effect3.gameObject, ShootingPoint.transform.position, rotation);
        Destroy(bullet, 5);
        ShootingPoint.transform.position = temp;
    }

    //this method make a spawn for the Electrec attack.
    void ElectrecAttack()
    {
        GameObject bullet = (GameObject)Instantiate(effect2.gameObject, ShootingPoint.transform.position, cam.transform.rotation);
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 5);
    }

    //this method make a spawn for the Ice attack.
    void WaterAttack()
    {
        GameObject bullet = (GameObject)Instantiate(effect1.gameObject, ShootingPoint.transform.position, cam.transform.rotation);
        NetworkServer.Spawn(bullet);
        Destroy(bullet, 5);
    }


    /*it make the spawn from the clients appear in the server*/
    [Command]
    void CmdDoFire(Vector3 from, Quaternion rotation)
    {
        FireAttack(from, rotation);
        RpcDoFire(from, rotation);
    }
    /*it make the spawn from the server appear in the client*/
    [ClientRpc]
    void RpcDoFire(Vector3 from, Quaternion rotation)
    {
        FireAttack(from, rotation);
    }
    /// ////////////////////////////////////////////////////
    /*it make the spawn from the clients appear in the server*/
    [Command]
    void CmdDoElictric()
    {
        ElectrecAttack();
        RpcDoElictric();
    }
    /*it make the spawn from the server appear in the client*/
    [ClientRpc]
    void RpcDoElictric()
    {
        ElectrecAttack();
    }
    /// ////////////////////////////////////////////////////
    /*it make the spawn from the clients appear in the server*/
    [Command]
    void CmdDoWater()
    {
        WaterAttack();
        RpcDoWater();
    }
    /*it make the spawn from the server appear in the client*/
    [ClientRpc]
    void RpcDoWater()
    {
        WaterAttack();
    }
}
