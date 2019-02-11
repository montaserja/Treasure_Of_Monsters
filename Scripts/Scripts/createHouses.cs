﻿
using UnityEngine;
using UnityEngine.Networking;

public class createHouses : NetworkBehaviour
{

    public GameObject house;
    public LayerMask mask = -1;
    public Terrain terrain;
    public GameObject monster;
    public bool mapIsOk = false;
    public int numOfPlayers = 0;
    private GameObject[] players1;
    private GameObject[] players2;
    // Use this for initialization
    void Start()
    {
    
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
                mapIsOk = true;
            }
        }
    }

    private void CreateHouses()
    {
        for (int i = 0; i < 11; i++)
        {
            transform.position = new Vector3(Random.Range(-790, 700), Random.Range(400f, 500f), Random.Range(-500, 700));
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
          
            GameObject m = (GameObject)Instantiate(monster,transform.position,transform.rotation);
            NetworkServer.Spawn(m);
            House.transform.Find("Door_Prefab").GetComponent<door>().setMonster(m);
            m.GetComponent<monsterScript>().setHouse(House);
            //m.gameObject.transform.SetParent(House.transform);
            m.transform.position = new Vector3(House.transform.position.x, House.transform.position.y+3, House.transform.position.z);

            //m.localScale = new Vector3(1, 1, 1);
        }
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
