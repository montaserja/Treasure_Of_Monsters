using UnityEngine;

using UnityEngine.Networking;





public class PlayerSetup : NetworkBehaviour
{



    // public float positionUpdateRate = 0.2f;

    // public float smooth = 15;



    private NetworkLobbyHook lobby;



    [SerializeField]

    Behaviour[] componentsToDisable;



    [SerializeField]

    string remoteLayerName = "RemotePlayer";


    public LayerMask mask = -1;
    float radius;




    Camera sceneCamera;





    Animator animator;

    [SyncVar(hook = "OnChangeAnimation")]

    public string animState = "Idle";



    void OnChangeAnimation(string aS)

    {

        if (isLocalPlayer)

            return;

        UpdateAnimationState(aS);

    }



    [Command]

    public void CmdChangeAnimState(string aS)

    {

        UpdateAnimationState(aS);

    }

    void UpdateAnimationState(string aS)

    {

        if (animState == aS)

            return;

        animState = aS;

        if (animState == "walk")

            animator.SetBool("walk", true);

        if (animState == "Stop")

            animator.SetBool("Stop", true);

        if (animState == "Dead")

            animator.SetBool("Dead", true);

        if (animState == "RunJump")

            animator.SetBool("RunJump", true);

        if (animState == "StopedJump")

            animator.SetBool("StopedJump", true);



    }



    // Vector3 playerPosition;

    // Transform myTransform;



    // Use this for initialization

    void Start()
    {

        //lobby = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyHook>();

        transform.position = new Vector3(Random.Range(-500f, 1000f), Random.Range(400f, 500f), Random.Range(-700f, 200f));

        // set the vertical offset to the object's collider bounds' extends
        if (GetComponent<Collider>() != null)
        {
            radius = GetComponent<Collider>().bounds.extents.y;
        }
        else
        {
            radius = 1f;
        }

        // raycast to find the y-position of the masked collider at the transforms x/z
        RaycastHit hit;
        // note that the ray starts at 100 units
        Ray ray = new Ray(transform.position + Vector3.up * 100, Vector3.down);

        Debug.Log("brabra");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            Debug.Log("bra");
            if (hit.collider != null)
            {
                Debug.Log("joa");
                // this is where the gameobject is actually put on the ground
                transform.position = new Vector3(transform.position.x, hit.point.y + radius, transform.position.z);
            }


        }

        lobby = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyHook>();

        animator = GetComponentInChildren<Animator>();

        // myTransform = transform;

        animator.SetBool("Idle", true);

        if (isLocalPlayer)

        {

            GetComponent<PlayerScript>().enabled = true;

        }

        else

        {

            GetComponent<PlayerScript>().enabled = false;

        }





        if (!isLocalPlayer)

        {

            DisableComponents();

            AssignRemoteLayer();

        }

        else

        {

            // Camera.main.gameObject.SetActive(false);

            sceneCamera = Camera.main;

            if (sceneCamera != null)

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

        transform.name = lobby.getName();

    }



    void AssignRemoteLayer()

    {

        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);

    }



    void OnDisable()

    {

        if (sceneCamera != null)

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

