using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class takeGuns : NetworkBehaviour {
    private  int Water = 0, Elictricity = 0, Fire = 0, Health = 0;
    private int WaterD = 0, ElictricityD = 0, FireD = 0;
    private const int WIndex = 0, EIndex = 1, FIndex = 2, HIndex = 3,WdIndex=4,EdIndex=5,FdIndex=6;

    public GameObject Wall;
    public GameObject Wall1;
    public GameObject FireWall;
    public GameObject FWall;
    public GameObject EWall;
    public GameObject WWall;

    Ray myRay;
    public Camera Cam;
    RaycastHit hit;
    private bool isDefend=false;
    private GameObject wallToSpawn;
    Vector3 up = new Vector3(0, 3f, 0);

    [SerializeField]
    public Image[] gunsImages;
    [SerializeField]
    public Sprite[] images;

    private int[] taked;
    int index = 0;
    [SyncVar]public int selected=-1;
    public Image treasure_key;
    public Image doors_key;

    private GameObject myTeamMate;
    private Player_Sync sync;
    private Collect_PowerUp TeamMateCollect;
    private int[] guns;

    // Use this for initialization
    void Start () {
        if (!isLocalPlayer)
        {
            return;
        }
       for( int i=0; i<gunsImages.Length;i++)
        {
            if (gunsImages[i] != null)
            {
                gunsImages[i].enabled = false;
            }
        }
        sync = this.GetComponent<Player_Sync>();
        guns = new int[4];
        taked = new int[4];
        //treasure_key.enabled = false;
        doors_key.enabled = false;
        
    }
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
        {
            return;
        }

            CmdSelect(selected);
        /*get the team mate*/
        if (myTeamMate == null || TeamMateCollect ==null)
        {
            sync = this.GetComponent<Player_Sync>();
            myTeamMate = sync.getTeamMate();
            if (myTeamMate != null) {
                TeamMateCollect = myTeamMate.GetComponent<Collect_PowerUp>();
            }
           
        }

        if (myTeamMate != null)//treasure key 
        {
            try
            {
                if (myTeamMate.GetComponent<status>().hasKey)
                {
                    treasure_key.enabled = true;
                }

                if (myTeamMate.GetComponent<PlayerHealth>().hasDoorKey)
                {
                    doors_key.enabled = true;
                }
            }
            catch(System.Exception  e)
            {
                print(e);
            }
        }


        if (TeamMateCollect != null) {//when the teammate take somthing this player and set the correct image
            
            if(TeamMateCollect.Elictricity != guns[EIndex])
            {
                guns[EIndex]= TeamMateCollect.Elictricity;
                setPic(EIndex);
            }
            if (TeamMateCollect.Fire != guns[FIndex])
            {
                guns[FIndex]= TeamMateCollect.Fire;
                setPic(FIndex);
            }
            if (TeamMateCollect.Water != guns[WIndex])
            {
                guns[WIndex]= TeamMateCollect.Water;
                setPic(WIndex);
            }
            if (TeamMateCollect.Health != guns[HIndex])
            {
                guns[HIndex]= TeamMateCollect.Health;
                setPic(HIndex);
            }
            if (TeamMateCollect.WaterD != WaterD)
            {
                WaterD = TeamMateCollect.WaterD;
                setPic(WdIndex);
            }
            if (TeamMateCollect.ElictricityD != ElictricityD)
            {
                ElictricityD = TeamMateCollect.ElictricityD;
                setPic(EdIndex);
            }
            if (TeamMateCollect.FireD != FireD)
            {
                FireD = TeamMateCollect.FireD;
                setPic(FdIndex);
            }
        }


        //selecting the gun
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {   
            selectgun(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectgun(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectgun(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            selectgun(3);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            isDefend = !isDefend;
            if(wallToSpawn!=null)
                Destroy(wallToSpawn);
        }

        if (isDefend)//creating walls to defend
        {
            if (Cam != null) { 
                myRay = Cam.ScreenPointToRay(Input.mousePosition);
            }
            else
                print("cam is null");
            if (Physics.Raycast(myRay, out hit,4000))
            {
                if (wallToSpawn == null)
                    {
                        wallToSpawn = Instantiate(Wall1, hit.point, Quaternion.Euler(90, 180, 0));
                        wallToSpawn.transform.position += up;
                    }
                    else
                    {
                        if (hit.collider.tag == "ground")
                        {
                            wallToSpawn.transform.position = hit.point;
                            wallToSpawn.transform.position += up;
                        }

                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            wallToSpawn.transform.Rotate(0, 0, 45);
                        }
                    }
                

            }
                if (Input.GetMouseButtonDown(0))
            {
                if (wallToSpawn != null)
                {
                    if (isServer)
                    {
                        Defend(wallToSpawn.transform.position,wallToSpawn.transform.rotation);
                    }
                    else
                    {
                        CmdDefend(wallToSpawn.transform.position, wallToSpawn.transform.rotation);
                    }
                    Destroy(wallToSpawn);
                    isDefend = !isDefend;
                }
            }
        }
     

    }

    void CmdSelect(int select)
    {
        this.selected = select;
        //RpcSelect(select);
    }

    void RpcSelect(int select)
    {
        this.selected = select;
    }


    private void selectgun(int i)
    {

        if (gunsImages[i].enabled == true)
        {
            selected = taked[i];

            CmdISelected(taked[i]);
        }
        else
        {
            selected = -1;
        }

        if (selected == EdIndex)
        {
            Wall = EWall;
        }
        else if(selected==WdIndex){
            Wall = WWall;
        }else if (selected == FdIndex)
        {
            Wall = FWall;
        }

     /*   if (isServer)
        {
            RpcSelect(selected);
  
        }
        else
        {
            CmdSelect(selected);
        }*/
    }

    private void setPic(int pic)
    {
        for(int i = 0; i < gunsImages.Length; i++)
        {
            if (gunsImages[i].enabled == false)
            {
                gunsImages[i].enabled = true;
                gunsImages[i].GetComponent<Image>().sprite = images[pic];
                taked[i] = pic;
                break;
            }else if (i == gunsImages.Length - 1)
            {
                if(index== gunsImages.Length - 1)
                {
                    index = 0;
                }
                gunsImages[index].GetComponent<Image>().sprite = images[pic];
                taked[index] = pic;
                index++;
            }
        }
    }


    private void Defend(Vector3 pos, Quaternion rot)
    {
        if (Cam != null)
            myRay = Cam.ScreenPointToRay(Input.mousePosition);
        else
            print("cam is null");

        if (Physics.Raycast(myRay, out hit))
        {
            // if (Input.GetMouseButtonDown(0))
            // {// what to do if i press the left mouse button
            GameObject bullet = Instantiate(Wall.gameObject,pos,rot);
            NetworkServer.Spawn(bullet);
            //Destroy(bullet, 10);


            // }
        }
    }

    [Command]
    void CmdDefend(Vector3 pos, Quaternion rot)
    {
        Defend(pos,rot);
       // RpcDefend();
    }





    [Command]public void CmdISelected(int index)
    {
         GameObject[] players1 = GameObject.FindGameObjectsWithTag("Player");
        Player_Sync thisPS = GetComponent<Player_Sync>();
        if (!thisPS)
        {
            return;
        }
        foreach (GameObject p in players1)
        {
 

            Player_Sync otherPS = p.GetComponent<Player_Sync>();
            if( !otherPS)
            {
                continue;
            }
            if(thisPS.team == otherPS.team)
            {
                PlayerAttacks otherPA = p.GetComponent<PlayerAttacks>();
                if (!otherPA)
                {
                    return;
                }
                otherPA.otherPlayerSelected = index;
                Debug.Log("server updated selected weapon on teamamte");

                return;//finished everyting so we can stop and return
            }
            
        }
        
    }
}
