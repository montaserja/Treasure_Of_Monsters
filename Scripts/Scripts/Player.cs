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
        /*players1 = GameObject.FindGameObjectsWithTag("Player");
        players2 = GameObject.FindGameObjectsWithTag("Player2");
        Player_Sync[] s = new Player_Sync[players1.Length];
        Player_Sync[] s2 = new Player_Sync[players2.Length];


        for (int i = 0; i < players1.Length; i++)
            s[i] = players1[i].GetComponent<Player_Sync>();

        for (int i = 0; i < players2.Length; i++)
            s2[i] = players2[i].GetComponent<Player_Sync>();

        for (int i = 0; i < players1.Length; i++)
        {
            for (int j = 0; j < players2.Length; j++)
            {
                if (s[i].team == s2[j].team)
                {
                    Debug.Log(s[i].team +" "+i+ "  "+j+" "+s2[j].team);
                    Vector3 temp = players1[i].transform.position;
                    temp.y = temp.y + 5f;
                    temp.x = temp.x - 1f;
                    Player2Camera player2Cam = players2[j].GetComponentInChildren<Player2Camera>();
                    if(players1[i]!=null)
                    player2Cam.setplayer(players1[i].transform);
                    Debug.Log("players1" + players1[i] + " playercam: " + player2Cam);
                    //this.transform.position = temp;
                    players2[j].transform.position = temp ;
                }
            }
        }
        
    */




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
