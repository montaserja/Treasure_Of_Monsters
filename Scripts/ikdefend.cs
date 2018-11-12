using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ikdefend : MonoBehaviour {
    private Animator anim;
   /* [SerializeField]
    public GameObject point;*/
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

   /* void OnAnimatorIK(int layerIndex)
    {

        anim.SetIKPosition(AvatarIKGoal.RightHand,point.transform.position);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
    }*/
}
