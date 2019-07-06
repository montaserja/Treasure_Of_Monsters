
using UnityEngine;
using UnityEngine.Networking;

public class createHouses : NetworkBehaviour
{

    public GameObject house;
    public LayerMask mask = -1;
    public Terrain terrain;
    public GameObject monster;
    public GameObject golem;
    public GameObject spider;
    public bool mapIsOk = false;
    public int numOfPlayers = 0;
    private GameObject[] players1;
    private GameObject[] players2;
    public GameObject treasure;
    public GameObject orc;
    public GameObject goblin;
    public GameObject wolf;
    [SerializeField]
    GameObject[] guns;
      
    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {

        if (isServer && !mapIsOk)
        {
            players1 = GameObject.FindGameObjectsWithTag("Player");
             players2 = GameObject.FindGameObjectsWithTag("Player2");
        

            if (numOfPlayers-1 == (players1.Length + players2.Length )) { 
                CreateHouses();
                createGuns();
                initSmallMonstes();
                mapIsOk = true;
            }
        }
    }

    private void CreateHouses()
    {
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;

        //int treasure_house = Random.Range(0, 10);
        int treasure_house = 1;
        for (int i = 0; i < 11; i++)
        {
            transform.position = new Vector3(Random.Range(-790, 700), Random.Range(400f, 500f), Random.Range(-500, 700));
            if(i == 1)
            {
                transform.position = new Vector3(-336f, 126.167f, -210f);
            }

            int xRes = terrain.terrainData.heightmapWidth;
            int yRes = terrain.terrainData.heightmapHeight;

            float[,] heights = terrain.terrainData.GetHeights(0, 0, xRes, yRes);
            int x = 0, y = 0;
            y = (int)(transform.position.x / 2) + 512;
            x = (int)(transform.position.z / 2) + 512;
            for (int k = y - 15; k < y + 15; k++)
            {
                for (int j = x - 18; j < x + 18; j++)
                {
                    heights[j, k] = heights[x, y];
                }
            }
            //SetHeights to change the terrain height.
            terrain.terrainData.SetHeights(0, 0, heights);
            RpcCreatHouses(transform.position);


            RaycastHit hit;
            // note that the ray starts at 100 units
            Ray ray = new Ray(transform.position + Vector3.up * 100, Vector3.down);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                if (hit.collider != null)
                {
                    // this is where the gameobject is actually put on the ground
                    transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                }
            }
            Quaternion Temo = new Quaternion(0f, 90f, 90f, 0f);
            GameObject House = (GameObject)Instantiate(house, transform.position, Temo);
            NetworkServer.Spawn(House);
            if (count1 == 4)
            {
                count1++;
            }
            if (count2 == 3)
            {
                count2++;
            }
            if (count1 <= 4)
            {
                GameObject m = (GameObject)Instantiate(monster, transform.position, transform.rotation);
                NetworkServer.Spawn(m);
                House.transform.Find("Door_Prefab").GetComponent<door>().setMonster(m);
                m.GetComponent<monsterScript>().setHouse(House);
                //m.gameObject.transform.SetParent(House.transform);
                m.transform.position = new Vector3(House.transform.position.x, House.transform.position.y + 3, House.transform.position.z);
                count1++;
            }

            if (count2 <= 3 && count1==5)
            {
                GameObject m = (GameObject)Instantiate(spider, transform.position, transform.rotation);
                NetworkServer.Spawn(m);
                House.transform.Find("Door_Prefab").GetComponent<door>().setMonster(m);
                m.GetComponent<monsterScript>().setHouse(House);
                //m.gameObject.transform.SetParent(House.transform);
                m.transform.position = new Vector3(House.transform.position.x, House.transform.position.y + 3, House.transform.position.z);
                count2++;
            }
            if (count3 <= 3 && count2 == 4)
            {
                GameObject m = (GameObject)Instantiate(golem, transform.position, transform.rotation);
                NetworkServer.Spawn(m);
                House.transform.Find("Door_Prefab").GetComponent<door>().setMonster(m);
                m.GetComponent<monsterScript>().setHouse(House);
                //m.gameObject.transform.SetParent(House.transform);
                m.transform.position = new Vector3(House.transform.position.x, House.transform.position.y + 3, House.transform.position.z);
                count3++;
            }
            if (i == treasure_house) {
                GameObject t = (GameObject)Instantiate(treasure);
                NetworkServer.Spawn(t);
                t.transform.position = new Vector3(House.transform.position.x,House.transform.position.y+7,House.transform.position.z+11);
                RpcTresurePosition(t, new Vector3(House.transform.position.x, House.transform.position.y + 7, House.transform.position.z + 11));
                //t.transform.position = House.transform.position;
            }

            //m.localScale = new Vector3(1, 1, 1);
        }
    }

    [ClientRpc] void RpcTresurePosition(GameObject t, Vector3 position)
    {
        t.transform.position = position;
    }

    private void createGuns()
    {
        for(int i = 0; i < guns.Length; i++)
        {
            for(int j = 0; j < 50; j++)
            {
                GameObject gun= Instantiate(guns[i]);
               
                gun.transform.position = new Vector3(Random.Range(-790, 700), Random.Range(400f, 500f), Random.Range(-500, 700));
                RaycastHit hit;
                // note that the ray starts at 100 units
                Ray ray = new Ray(gun.transform.position + Vector3.up * 50, Vector3.down);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                    if (hit.collider != null)
                    {
                   
                        gun.transform.position = new Vector3(gun.transform.position.x, hit.point.y - 3, gun.transform.position.z);
                        NetworkServer.Spawn(gun);
                    }
                }                       
            }
        }
    }

    private void creatSmallMonsters(GameObject monstertype)
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject monster = Instantiate(monstertype);
            monster.transform.position = new Vector3(Random.Range(-790, 700), Random.Range(400f, 500f), Random.Range(-500, 700));
            float radius;

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
                    NetworkServer.Spawn(monster);

                }


            }
        }
    }

    private void initSmallMonstes()
    {
        creatSmallMonsters(wolf);
        creatSmallMonsters(goblin);
        creatSmallMonsters(orc);


    }


    [ClientRpc]
    void RpcCreatHouses(Vector3 position)
    {

        int xRes = terrain.terrainData.heightmapWidth;
        int yRes = terrain.terrainData.heightmapHeight;

        float[,] heights = terrain.terrainData.GetHeights(0, 0, xRes, yRes);


        int x = 0, y = 0;
        y = (int)(position.x / 2) + 512;
        x = (int)(position.z / 2) + 512;
        //heights[x,y]=0.5f;
        for (int k = y - 15; k < y + 15; k++)
        {
            for (int j = x - 18; j < x + 18; j++)
            {
                heights[j, k] = heights[x, y];
            }
        }
        //SetHeights to change the terrain height.
        terrain.terrainData.SetHeights(0, 0, heights);

    }
}
