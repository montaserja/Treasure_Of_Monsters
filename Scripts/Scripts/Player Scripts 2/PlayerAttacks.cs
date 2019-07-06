// In this script we will make tha player attack types. 


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;



public class PlayerAttacks : NetworkBehaviour
{
    private const int WIndex = 0, EIndex = 1, FIndex = 2, HIndex = 3;
    public bool HasFirePower, HasElectricPower, HasWaterPower,HasHealthPower, canShoot = false;
    private NetworkAnimator anim;
    public GameObject effect1;
    public Transform effect2;
    public Transform effect3;
    public GameObject ShootingPoint;
    public Camera cam;
    private PlayerHealth health;
    private int attacktype;

    private Player_Sync sync;
    private takeGuns takeGuns;
    private int selected=-1;
    private GameObject myTeamMate;
    public bool selfControl = false;

    [SyncVar]public int otherPlayerSelected = -1;



    private void Start()
    {
        anim = GetComponent<NetworkAnimator>();
        health = GetComponent<PlayerHealth>();
        //transform.position = new Vector3(-80, 0.3f, 409);
        sync = this.GetComponent<Player_Sync>();
    }
    void Update()
    {
        if (!isLocalPlayer)
            return;
        /*************************************************************/

        if (Input.GetKeyDown(KeyCode.P))
        {
            selfControl = !selfControl;
        }
        if (myTeamMate != null && takeGuns!=null) {
            
            if (Input.GetKeyDown(KeyCode.Mouse0) && HasFirePower && GetComponent<AimBehaviourBasic>().Aiming() && !GetComponent<PlayerHealth>().death)
            {
                //ShootingPoint.transform.position = cam.transform.position;
                ShootingPoint.transform.rotation = cam.transform.rotation;
                FireAttack(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                //anim.SetTrigger("Attacking");
                CmdDoFire(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                if (isServer)
                {
                    RpcDoFire(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                }

            }
            /*************************************************************/
            if (Input.GetKeyDown(KeyCode.Mouse0) && HasElectricPower && GetComponent<AimBehaviourBasic>().Aiming() && !GetComponent<PlayerHealth>().death)
            {
                ShootingPoint.transform.rotation = cam.transform.rotation;
                ElectrecAttack(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                CmdDoElictric(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                if (isServer)
                {
                    RpcDoElictric(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                }
            }
            /*************************************************************/
            if (Input.GetKeyDown(KeyCode.Mouse0) && HasWaterPower && GetComponent<AimBehaviourBasic>().Aiming() && !GetComponent<PlayerHealth>().death)
            {
                ShootingPoint.transform.rotation = cam.transform.rotation;
                WaterAttack(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                CmdDoWater(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                if (isServer)
                {
                    RpcDoWater(this.ShootingPoint.transform.position, this.ShootingPoint.transform.rotation);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && HasHealthPower && !GetComponent<PlayerHealth>().death)
            {
                PlayerHealth health = this.GetComponent<PlayerHealth>() ;
                if (health.health < 100)
                    if (health.health > 90)
                        health.health = 100;
                    else
                        this.GetComponent<PlayerHealth>().health += 10;
                
            }
            PlayerAttackType();
        }
        else
        {
            sync = this.GetComponent<Player_Sync>();
            myTeamMate = sync.getTeamMate();
            if (myTeamMate != null)
                takeGuns = myTeamMate.GetComponent<takeGuns>();
            else
                print("teammate is null");
            
        }

    }

    //in this method the player choos which attack type he want.
    void PlayerAttackType()
    {


        if (selfControl) {
            // pressing numper 1 will chosse the electric attack
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                HasFirePower = false;
                HasElectricPower = true;
                HasWaterPower = false;
                attacktype = 1;

                canShoot = true;
            }
            // pressing numper 2 will chosse the water attack
            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                HasFirePower = false;
                HasElectricPower = false;
                HasWaterPower = true;
                attacktype = 2;
                canShoot = true;

            }
            // pressing numper 3 will chosse the fire attack
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                HasFirePower = true;
                HasElectricPower = false;
                attacktype = 3;
                HasWaterPower = false;
                canShoot = true;
            }
        }
        else
        {
            //if (takeGuns.selected == EIndex)
            if (otherPlayerSelected == EIndex)
            {
                HasFirePower = false;
                HasElectricPower = true;
                HasWaterPower = false;
                attacktype = 1;

                canShoot = true;
                HasHealthPower = false;
                
            }else if (otherPlayerSelected == FIndex)
            {
                HasFirePower = true;
                HasElectricPower = false;
                attacktype = 3;
                HasWaterPower = false;
                canShoot = true;
                HasHealthPower = false;
            }else if (otherPlayerSelected == WIndex)
            {
                HasFirePower = false;
                HasElectricPower = false;
                HasWaterPower = true;
                attacktype = 2;
                canShoot = true;
                HasHealthPower = false;
            }else if (otherPlayerSelected == HIndex)
            {
                HasFirePower = false;
                HasElectricPower = false;
                HasWaterPower = false;
                HasHealthPower = true;

                canShoot = true;
            }
            else
            {
                HasFirePower = false;
                HasElectricPower = false;
                HasWaterPower = false;
                attacktype = 0;
                HasHealthPower = false;

                canShoot = false;
            }
        }


    }

    //this method make a spawn for the fireBall attack.
    void FireAttack(Vector3 from, Quaternion rotation)
    {
        Vector3 temp = ShootingPoint.transform.position;
        ShootingPoint.transform.position -= ((ShootingPoint.transform.position - cam.transform.position) / 10);
        GameObject bullet = (GameObject)Instantiate(effect3.gameObject, ShootingPoint.transform.position, rotation);
        Destroy(bullet, 5);
        createRayCast();
        ShootingPoint.transform.position = temp;
    }

    //this method make a spawn for the Electrec attack.
    void ElectrecAttack(Vector3 from, Quaternion rotation)
    {
        Vector3 temp = ShootingPoint.transform.position;
        ShootingPoint.transform.position -= ((ShootingPoint.transform.position - cam.transform.position) / 10);
        GameObject bullet = (GameObject)Instantiate(effect2.gameObject, ShootingPoint.transform.position, rotation);
        Destroy(bullet, 5);
        createRayCast();
        ShootingPoint.transform.position = temp;
    }

    //this method make a spawn for the Ice attack.
    void WaterAttack(Vector3 from, Quaternion rotation)
    {
        Vector3 temp = ShootingPoint.transform.position;
        ShootingPoint.transform.position -= ((ShootingPoint.transform.position - cam.transform.position) / 10);
        GameObject bullet = (GameObject)Instantiate(effect1.gameObject, ShootingPoint.transform.position, rotation);
        Destroy(bullet, 5);
        createRayCast();
        ShootingPoint.transform.position = temp;
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
    void CmdDoElictric(Vector3 from, Quaternion rotation)
    {
        ElectrecAttack(from, rotation);
        RpcDoElictric(from, rotation);
    }
    /*it make the spawn from the server appear in the client*/
    [ClientRpc]
    void RpcDoElictric(Vector3 from, Quaternion rotation)
    {
        ElectrecAttack(from, rotation);
    }
    /// ////////////////////////////////////////////////////
    /*it make the spawn from the clients appear in the server*/
    [Command]
    void CmdDoWater(Vector3 from, Quaternion rotation)
    {
        WaterAttack(from, rotation);
        RpcDoWater(from, rotation);
    }
    /*it make the spawn from the server appear in the client*/
    [ClientRpc]
    void RpcDoWater(Vector3 from, Quaternion rotation)
    {
        WaterAttack(from, rotation);
    }


    void createRayCast()
    {
        // Cast the shot to find a target.
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit = default(RaycastHit);
        // Target was hit.
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.transform != this.transform)
            {                
                if (hit.collider.gameObject != null)
                {
                    CmdDealDamage(hit.collider.gameObject, attacktype,gameObject);
                }
            }
        }
    }

    [Command]void CmdDealDamage(GameObject damaged, int attacktype, GameObject damager)
    {
        if (damaged != null)
        {
            if (damaged.GetComponent<PlayerHealth>() != null)
            {
                PlayerHealth PH = damaged.GetComponent<PlayerHealth>();
                if (!PH)
                {
                    return;
                }
                PH.dealDamage(attacktype, damager);
            }

            else if (damaged.GetComponent<SmallMonsters>() != null)
            {
                SmallMonsters SM = damaged.GetComponent<SmallMonsters>();
                if (!SM)
                {
                    return;
                }
                SM.dealDamage(attacktype, damager);
            }

            else if (damaged.GetComponent<monsterScript>() != null)
            {
                monsterScript MS = damaged.GetComponent<monsterScript>();
                if (!MS)
                {
                    return;
                }
                MS.dealDamage(attacktype, damager);
            }
        }
    }
}
