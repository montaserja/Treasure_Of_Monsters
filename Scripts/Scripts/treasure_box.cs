using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treasure_box : MonoBehaviour {

    public bool close;
    private Animator anim;
	// Use this for initialization
	void Start () {
        close = true;
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!close)
        {
            anim.SetTrigger("open");
            close = true;
        }
	}
}
