using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RFX4_SimpleDecal : MonoBehaviour
{
    public float Offset = 0.05f;
    public bool UseNormalRotation ;
    private Transform t;
    private RaycastHit hit;

    // Use this for initialization
    void OnEnable ()
	{
	    t = transform;
        if (Physics.Raycast(t.position + Vector3.up / 2, Vector3.down, out hit))
        {
            transform.position = hit.point + Vector3.up * Offset;
            if (UseNormalRotation) transform.rotation = Quaternion.LookRotation(-hit.normal);
        }
    }
}
