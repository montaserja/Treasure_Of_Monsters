using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player_Sync : NetworkBehaviour
{
    [SyncVar]
    Vector3 playerPosition;

    [SyncVar] private Quaternion syncPlayerRotation;
    [SyncVar] private Quaternion syncCamRotation;



    [SerializeField]
    Transform myTransform;
    [SerializeField]
    Transform CamTransform;
    [SerializeField]
    float lerpRate = 15f;


  /*  // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}*/



    void FixedUpdate()
    {
        TransmitPosition();
        LerpPosition();
     //   LerpRotation();
    }


  /*  void LerpRotation() {
        if (isLocalPlayer)
        {
            return;
        }
        
      
    }*/

    //Transform the Remote Player position to the server Player 
    void LerpPosition()
    {
        if (isLocalPlayer)
        {
            return;
        }
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);
        CamTransform.rotation = Quaternion.Lerp(CamTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);
        myTransform.position = Vector3.Lerp(myTransform.position, playerPosition, Time.deltaTime * lerpRate);
    }
    
    //Called just in the server side which takes the client position
    [Command]
    void CmdSendPosition(Vector3 position,Quaternion PlayerRot,Quaternion CamRot)
    {
        syncPlayerRotation = PlayerRot;
        syncCamRotation = CamRot;
        playerPosition = position;
       
    }


    //on the client side send to the server
    [ClientCallback]
    void TransmitPosition()
    {

        if (isLocalPlayer) {

        CmdSendPosition(myTransform.position,myTransform.rotation,CamTransform.rotation);
        }
    }
}
