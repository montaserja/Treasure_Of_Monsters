using UnityEngine;
using UnityEngine.Networking;


public class PlayerSetup : NetworkBehaviour {

   // public float positionUpdateRate = 0.2f;
   // public float smooth = 15;

   [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    Camera sceneCamera;


   // Vector3 playerPosition;
   // Transform myTransform;

	// Use this for initialization
	void Start () {

       // myTransform = transform;
       
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {
            // Camera.main.gameObject.SetActive(false);
            sceneCamera = Camera.main;
            if(sceneCamera != null)
            {
                sceneCamera.gameObject.SetActive(false);
            }

            
           // StartCoroutine(UpdatePosition());
        }

        RegisterPlayer();



    }

   /*  void Update()
    {
        LerpPosition();
    }

    void LerpPosition()
    {
        if (isLocalPlayer)
        {
            return;
        }

        myTransform.position = Vector3.Lerp(myTransform.position, playerPosition, Time.deltaTime * smooth);
    }

    IEnumerator UpdatePosition()
    {
        CmdSendPosition(myTransform.position);
        while (enabled)
        {
            yield return new WaitForSeconds(positionUpdateRate);
        }

    }

    [Command]
   void CmdSendPosition(Vector3 position)
    {
        playerPosition = position;
        RpcRecievePosition(position);
    }

    [ClientRpc]
    void RpcRecievePosition(Vector3 position)
    {
        playerPosition = position;
    }
    */

    void RegisterPlayer()
    {
        string _ID = "Player " + GetComponent<NetworkIdentity>().netId;
        transform.name = _ID;
    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void OnDisable()
    {
        if(sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

  
}

