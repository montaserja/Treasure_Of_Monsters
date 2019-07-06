using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class healthbar : NetworkBehaviour {
/*set the health bar as image*/
    public Image image;
    public Canvas can;
    private PlayerHealth health;

	// Use this for initialization
	void Start () {
        if (isLocalPlayer) { 
        health = GetComponent<PlayerHealth>();
        }
        else
        {
            can.gameObject.SetActive(false);
            return;
        }

    }
	
	// Update is called once per frame
	void Update () {
        if (isLocalPlayer)
        {
            float healthbar = health.health / 100;
            image.gameObject.transform.localScale = new Vector3(healthbar, 1f, 1f);
        }
	}
}
