using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(PlayerMotor))]

public class PlayerScript : MonoBehaviour {
    Collider col;

    private Rigidbody myBody;
    private Animator anim;

    private bool isMoving;
    private float rotationSpeed = 4f;
    private float jumpForce = 4f;
    private bool jumped;
    private bool canJump=false;
    private float JumpDir = 10f;

    private float moveVertical, moveHorizontal;
   // public LayerMask groundLayer;

    Transform mytransform;

    [SerializeField]
    GameObject Protector;
    [SerializeField]
    GameObject defendPoint;


    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 2f;
    private bool isGrounded=false;
    private bool isdefend = false;


    private PlayerMotor motor;



    // jump
    private Vector3 jumping=new Vector3(0,3,0);
    //end jumping def

    private void Awake()
    {
        myBody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
       
    }

    // Use this for initialization
    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        col = gameObject.GetComponent<Collider>();

    }
	
	// Update is called once per frame
	void Update () {
        playerMoveKeyBoard();
        
    }

    void FixedUpdate()
    {
        MoveAndRotate();
    /* if (transform.position.y > terrain.heightmapScale.y+10) { 
        Vector3 temp = transform.position;
        temp.y -= 10.0f;
        transform.position = temp;
        }*/

    }

    void OnAnimatorIK()
     {
         if (isdefend)
         {
            
           anim.SetIKPosition(AvatarIKGoal.RightHand, defendPoint.transform.position);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
          
        }
     }
 

    void playerMoveKeyBoard()
    {
        if (Input.GetKeyDown(KeyCode.Q)){
          
            isdefend = !isdefend;
            Protector.SetActive(isdefend);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveHorizontal = -1;
            anim.SetTrigger(MyTags.WALK_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.WALK_TRIGGER);
            isMoving = true;
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            moveHorizontal = 0;
            isMoving = false;
            anim.SetTrigger(MyTags.STOP_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.STOP_TRIGGER);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveHorizontal = 1;
            isMoving = true;
            anim.SetTrigger(MyTags.WALK_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.WALK_TRIGGER);
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            moveHorizontal = 0;
            isMoving = false;
            anim.SetTrigger(MyTags.STOP_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.STOP_TRIGGER);
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            moveVertical = 1;
            isMoving = true;
            anim.SetTrigger(MyTags.WALK_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.WALK_TRIGGER);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            moveVertical = 0;
            isMoving = false;
            anim.SetTrigger(MyTags.STOP_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.STOP_TRIGGER);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveVertical = -1;
            isMoving = true;
            anim.SetTrigger(MyTags.WALK_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.WALK_TRIGGER);
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            moveVertical = 0;
            isMoving = false;
            anim.SetTrigger(MyTags.STOP_TRIGGER);
            this.GetComponent<PlayerSetup>().CmdChangeAnimState(MyTags.STOP_TRIGGER);
        }
        if(Input.GetKeyDown (KeyCode.Space) && isGrounded)
        {
            myBody.AddForce(jumping * 3, ForceMode.Impulse);
            Physics.gravity = new Vector3(0, -20F, 0);
            if (isMoving)
            {
                anim.SetTrigger(MyTags.RUNJUMP_TRIGGER);
            }
            else if (!isMoving)
            {
                anim.SetTrigger(MyTags.JUMP_TRIGGER);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            isGrounded = false;
        }
    }

    void MoveAndRotate()
    {
        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");

        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;
        Vector3 _velocity = (_movHorizontal + _movVertical).normalized * speed;
        motor.Move(_velocity);



        float _yRot = Input.GetAxisRaw("Mouse X");
        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;
       
        motor.Rotate(_rotation);

        float _xRot = Input.GetAxisRaw("Mouse Y");
        if (_xRot < 1 && _xRot>-1)
        {
            Vector3 _cameraRotation = new Vector3(_xRot * lookSensitivity, 0f, 0f);
            motor.RotateCamera(_cameraRotation);
        }


    }

public GameObject getProtector()
    {
        return Protector;
    }
 
}
