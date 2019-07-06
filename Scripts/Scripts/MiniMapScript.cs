using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapScript : MonoBehaviour {

    public Transform player;

    private void LateUpdate()
    {
		//Camera runs after the player
        Vector3 newPosition = player.position;
        newPosition.y = newPosition.y + 15;
        transform.position = newPosition;
    }
}
