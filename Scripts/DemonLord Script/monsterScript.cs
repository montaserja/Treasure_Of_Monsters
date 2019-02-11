using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;



public class monsterScript : MonoBehaviour
{

    public GameObject shootingPoint;
    public Camera cam;
    public Transform effect;
    GameObject[] players;
    private Animator anim;
    private bool stop = false;
    private int health = 200;
    public bool Death = false;
    private bool hit = true;
    private bool doorOpen = false;
    private GameObject hisHouse;
    private Vector3 hishousePosition;

    // Use this for initialization
    void Start()
    {
        hishousePosition = this.transform.position;
        anim = GetComponent<Animator>();
        anim.SetBool("isIdle", true);
        GameObject[] houses = GameObject.FindGameObjectsWithTag("House");
        for (int i = 0; i < houses.Length; i++)
        {
            if (houses[i].transform.position == this.transform.position)
            {
                print("da5al");
                hisHouse = houses[i];
            }
        }
    }





    // Update is called once per frame
    void Update()
    {

        if (Death)
        {
            anim.SetBool("Death", Death);
            StartCoroutine(die());
        }
        else
        {

            anim.SetBool("isIdle", true);
            anim.SetBool("isWalk", false);
            anim.SetBool("isAttacking", false);
            players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {

                if (Vector3.Distance(players[i].transform.position, this.transform.position) <= 50 && !Death && !players[i].GetComponent<PlayerHealth>().death && hisHouse.transform.Find("Door_Prefab").GetComponent<door>().isOpen)
                {
                    Vector3 direction = players[i].transform.position - this.transform.position;
                    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), 1f);
                    cam.transform.LookAt(players[i].transform);
                    anim.SetBool("isIdle", false);
                    if (direction.magnitude <= 50 && !stop)
                    {
                        this.transform.Translate(0, 0, 0.1f);
                        anim.SetBool("isWalk", true);
                        anim.SetBool("isAttacking", false);
                    }


                    if (direction.magnitude <= 10 && !players[i].GetComponent<PlayerHealth>().death)
                    {
                        stop = true;
                        this.transform.Translate(0, 0, 0);
                        anim.SetBool("isWalk", false);
                        anim.SetBool("isAttacking", true);
                        FireAttack();
                        hit = true;
                    }
                    else
                        stop = false;

                }



            }
        }
    }
    //this method make a spawn for the fireBall attack.
    void FireAttack()
    {
        cam.transform.rotation *= Quaternion.Euler(0, 5, 0);
        GameObject bullet = (GameObject)Instantiate(effect.gameObject, shootingPoint.transform.position, cam.transform.rotation);
        Destroy(bullet, 2f);
    }

    private void OnParticleCollision(GameObject collision)
    {
        if (collision.tag == "PlayerFireBall" && hit)
        {
            if (health > 0)
            {
                health -= 50;
            }
            if (health <= 0)
            {
                Death = true;
            }
            hit = false;
        }
    }

    public void setHouse(GameObject house)
    {
        this.hisHouse = house;
    }


    IEnumerator die()
    {
        Death = false;
        yield return new WaitForSecondsRealtime(8f);
        anim.SetBool("Death", Death);
        transform.position = hishousePosition;
        health = 200;
    }

}
