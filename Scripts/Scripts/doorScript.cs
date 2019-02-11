using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorScript : MonoBehaviour {

    public bool open,canOpen;

	// Use this for initialization
	void Start () {
        open = false;
        canOpen = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (!open)
        {
            if (canOpen)
            {
                this.gameObject.SetActive(false);
                open = true;
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            canOpen = true;
        }
    }
}
