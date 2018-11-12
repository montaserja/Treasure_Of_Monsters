using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour {
    private bool HasFirePower, HasElectricPower,HasWaterPower;


    public Transform prefabEffect;
    public GameObject ShootingPoint;
    private Rigidbody myBody;
    [SerializeField]

    Transform[] effects;



    // Use this for initialization
    void Start () {
        myBody = GetComponent<Rigidbody>();

        var tm = GetComponentInChildren<RFX4_TransformMotion>(true);
        if (tm != null)
        {
            tm.CollisionEnter += Tm_CollisionEnter;
        }
    }
	
	// Update is called once per frame
	void Update () {
        playerAttacks();
        PlayerAttackType();
    }

    private void Tm_CollisionEnter(object sender, RFX4_TransformMotion.RFX4_CollisionInfo e)
    {
        Debug.Log(e.Hit.transform.name); //will print collided object name to the console.
    }

    void playerAttacks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Vector3 rotation = new Vector3(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0);

            
            if (prefabEffect != null)
            {
                Instantiate(prefabEffect, ShootingPoint.transform.position, myBody.rotation);
            }
        }
    }

    void PlayerAttackType()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            HasFirePower = true;
            HasElectricPower = false;
            HasWaterPower = false;
            prefabEffect =effects[0];
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            HasFirePower = false;
            HasElectricPower = true;
            HasWaterPower = false;
            prefabEffect = effects[1];
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            HasFirePower = false;
            HasElectricPower = false;
            HasWaterPower = true;
            prefabEffect = effects[2];
        }
    }

}
