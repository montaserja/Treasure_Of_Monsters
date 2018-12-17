 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 using UnityEngine.Networking;
 
 public class ikdefend : NetworkBehaviour
{
    //The Gameobject holding the weapons
    [SerializeField]
    GameObject ProtectorHolder;



    //Holds all of the players weapons 
    [SerializeField]
    GameObject[] Defender;

    [SerializeField]
    GameObject defendPoint;
    [SerializeField]
    GameObject upPoint;

    [SerializeField]
    GameObject bottomPoint;

    private float lookSensitivity = 1f;

    private Vector3 standartPointPosition;

    private bool isdefend = false;
    public Camera mainCam;
    public Camera defendCam;

    int DefenderNumber = 0; //Used to scroll through weapons in inventory
    private Animator anim;

    //Gives the player Their start Weapon
    private void Start()
    {
      
        anim   = GetComponent<Animator>();
        DefenderNumber = 0;
      

        standartPointPosition = defendPoint.transform.position;
        bottomPoint.transform.position = new Vector3(defendPoint.transform.position.x, defendPoint.transform.position.y - 0.3f, defendPoint.transform.position.z);
        //Apply weapon through out the network
      
    }
    private void FixedUpdate()
    {

        if (defendCam != null)
        {
            if (isdefend) { 
            var mousePos = Input.mousePosition;
            if (mousePos.x < 0 || mousePos.x >= Screen.width || mousePos.y < 0 || mousePos.y >= Screen.height)
                return;

            var ray = defendCam.ScreenPointToRay(mousePos);
            RaycastHit hit;
            }
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;
        onCamDefend();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isdefend = !isdefend;
            mainCam.gameObject.SetActive(!isdefend);
             defendCam.gameObject.SetActive(isdefend);
            
           
            onPlayerDefend();
            if (isServer)
            {
                RpcPlayerDefend();
            }
            else
            {
                CmdPlayerDefend();
            }
        }

    }
    void OnAnimatorIK()
    {
        if (isdefend)
        {

            anim.SetIKPosition(AvatarIKGoal.RightHand, defendPoint.transform.position);
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);

        }
    }
    private void onCamDefend()
    {

        if (isdefend)
        {

            float _yRot = Input.GetAxisRaw("Mouse X");
            Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;

            float _xRot = Input.GetAxisRaw("Mouse Y");


            if (_xRot < 1 && _xRot > -1)
            {
                Vector3 CameraRotation = Vector3.zero;
                if ((defendPoint.transform.position.y <= upPoint.transform.position.y) && (defendPoint.transform.position.y >= bottomPoint.transform.position.y))
                {


                    CameraRotation = new Vector3(_xRot * lookSensitivity, 0f, 0f);
                    defendCam.transform.Rotate(-CameraRotation);

                }
                else
                {
                    if (_xRot > 0)
                    {
                        defendPoint.transform.position = upPoint.transform.position;
                    }
                    else
                    {
                        defendPoint.transform.position = bottomPoint.transform.position;

                    }

                }


            }



        }
    }

        void onPlayerDefend()
    {
        DefenderNumber++;
        //Disable previous weapon
        if (DefenderNumber > 0)
            Defender[DefenderNumber - 1].SetActive(false);

        //If we are greater than all weapons in inventory, set it to 0
        if (DefenderNumber >= Defender.Length)
        {
            DefenderNumber = 0;
        }
        //Set current weapon active
        Defender[DefenderNumber].SetActive(true);
    }

    [Command]
    void CmdPlayerDefend()
    {
        //Apply it to all other clients
        onPlayerDefend();
        RpcPlayerDefend();
    }

    [ClientRpc]
    void RpcPlayerDefend()
    {
        if (isLocalPlayer)
            return;
        onPlayerDefend();
    }
}