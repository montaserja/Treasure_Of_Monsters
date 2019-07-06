using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class score : MonoBehaviour {
    public LayerMask mask = -1;
    public int BigMonsterCount, PlayersCount, SmallMonsterCount;
    private bool BigMonstersIsLive=true;
    private bool SmallMonstersIsLive=true;
    private bool PlayerIsLive=true;
    public bool hasTresureKey, hasDoorsKey;
    private GameObject theHit;
    public Text bigMonsters;
    public Text smallMonsters;
    public Text Playerskilled;



    // Use this for initialization
    void Start () {
        BigMonsterCount = 0;
        PlayersCount = 0;
        SmallMonsterCount = 0;
        hasDoorsKey = false;
        hasTresureKey = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Mouse0) && GetComponent<PlayerAttacks>().canShoot && GetComponent<AimBehaviourBasic>().Aiming() 
            && !GetComponent<PlayerHealth>().death)
        {
            RaycastHit hit;
            Ray ray = new Ray(GetComponent<PlayerAttacks>().ShootingPoint.transform.position, transform.forward);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask) )
            {
                if (hit.collider.gameObject.tag == "Demon_Lord" || hit.collider.gameObject.tag == "Spider" || 
                    hit.collider.gameObject.tag == "Golem" )
                {                         
                    if (hit.collider.gameObject.GetComponent<monsterScript>().isDeath() && BigMonstersIsLive)
                    {
                        BigMonsterCount++;
                        bigMonsters.text = "Big Monsters" +BigMonsterCount;
                        BigMonstersIsLive = false;
                    }
                    if (hit.collider.gameObject.GetComponent<monsterScript>().isDeath() == false)
                        BigMonstersIsLive = true;
                }
                if (hit.collider.gameObject.tag == "Wolf" || hit.collider.gameObject.tag == "Goblin" ||
                    hit.collider.gameObject.tag == "ORC")
                {
                    theHit = hit.collider.gameObject;
                    Invoke("countsmall",2);
                }
                if (hit.collider.gameObject.tag == "Player")
                {
                    if (hit.collider.gameObject.GetComponent<PlayerHealth>().isDeath() && PlayerIsLive)
                    {
                        PlayersCount++;
                        Playerskilled.text = "kill"+PlayersCount ;
                        PlayerIsLive = false;
                    }
                    if (hit.collider.gameObject.GetComponent<PlayerHealth>().isDeath() == false)
                        PlayerIsLive = true;
                }

            }
        }

        if (PlayersCount >= 2 && BigMonsterCount >=3)
        {
            hasTresureKey = true;
        }

        if (PlayersCount >= 1 && SmallMonsterCount >= 5)
        {
            hasDoorsKey = true;
        }


    }

    private void countsmall()
    {
        if (theHit.GetComponent<SmallMonsters>().isDeath() && theHit.GetComponent<SmallMonsters>() != null)
        {
            SmallMonsterCount++;
            smallMonsters.text = "Small Monsters"+SmallMonsterCount;
        }
    }
}
