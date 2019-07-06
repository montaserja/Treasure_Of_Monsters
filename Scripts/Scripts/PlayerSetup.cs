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













    // Vector3 playerPosition;

    // Transform myTransform;



    // Use this for initialization

    void Start()
    {
        //lobby = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyHook>();

        transform.position = new Vector3(Random.Range(-200f, -250f), Random.Range(400f, 500f), Random.Range(-200f, -250f));
       // transform.position = new Vector3(500, 192, 500);

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

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            if (hit.collider != null)
            {
                // this is where the gameobject is actually put on the ground
                transform.position = new Vector3(transform.position.x, hit.point.y + radius, transform.position.z);
            }


        }

        lobby = GameObject.Find("LobbyManager").GetComponent<NetworkLobbyHook>();









        if (!isLocalPlayer)

        {

            DisableComponents();

            AssignRemoteLayer();

        }

        else

        {

            // Camera.main.gameObject.SetActive(false);
           // print("ctre : "+componentsToDisable[0].gameObject.active);

            componentsToDisable[0].gameObject.SetActive(true);

            sceneCamera = Camera.main;

            if (sceneCamera != null)

            {

                sceneCamera.gameObject.SetActive(false);

            }





            // StartCoroutine(UpdatePosition());

        }



        //RegisterPlayer(); ibra 







    }




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

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (componentsToDisable[0].gameObject.active == false)
            {
                componentsToDisable[0].gameObject.SetActive(true);
            }
        }
    }



}