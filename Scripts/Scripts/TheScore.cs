using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheScore : MonoBehaviour {
    public static TheScore instance = null;
    private static Dictionary<string, PlayerHealth> Allplayers = new Dictionary<string, PlayerHealth>();
    // Use this for initialization
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    // Update is called once per frame
    void Update () {
		
	}
    public void RegisterPlayer(string _PlayerID, PlayerHealth _player)
    {
        if (Allplayers.ContainsKey(_PlayerID))
        {
            Allplayers[_PlayerID] = _player;
        }
        else
        {
            Allplayers.Add(_PlayerID, _player);
        }
    }
    public PlayerHealth getPlayer(string Name)
    {
        if (Allplayers[Name])
        {
            return Allplayers[Name];
        }
        return null;
    }

    private void OnDestroy()
    {        
        Allplayers.Clear();
    }
}
