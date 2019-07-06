using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

    public Camera myCam;
    private Camera Cam2;

    private GameObject[] players1;
    private GameObject[] players2;

    private Rigidbody rb;



    [SerializeField]

    Behaviour[] componentsToDisable;
    private float lookSensitivity = 3f;

    // Use this for initialization
    void Start () {

        if (!isLocalPlayer)

        {

            DisableComponents();

        }

        else

        {

            // Camera.main.gameObject.SetActive(false);

            Cam2 = Camera.main;

            if (Cam2 != null)
            {
                Cam2.gameObject.SetActive(false);
            }
            if (myCam != null)
            {
                myCam.gameObject.SetActive(true);

            }





            // StartCoroutine(UpdatePosition());

        }

        rb = GetComponent<Rigidbody>();


        transform.position = new Vector3(Random.Range(-200f, -170f), Random.Range(400f, 500f), Random.Range(-200f, -170f));
     



    }
	
	// Update is called once per frame
    /*here each player of type 2 get all the players in the game and search for the player in the same team to set this postion around his position*/
	void Update () {
        if (!isLocalPlayer)
            return;

        if (Camera.main!=null &&Camera.main.enabled)
        {
            Camera.main.gameObject.SetActive(false);
        }       
      




    }
 
  
    //to disable the components in non local players machine
    void DisableComponents()

    {

        for (int i = 0; i < componentsToDisable.Length; i++)

        {

            componentsToDisable[i].enabled = false;

        }

    }
}
