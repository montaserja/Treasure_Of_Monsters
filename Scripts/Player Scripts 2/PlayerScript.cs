using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]

public class PlayerScript : MonoBehaviour {

    private Rigidbody myBody;
    private Animator anim;

    private bool isMoving;
    private float rotationSpeed = 4f;
    private float jumpForce = 3f;
    private bool canJump;

    private float moveVertical, moveHorizontal;
    private float rotY = 0f;
    public Transform groundCheck;
    public LayerMask groundLayer;

 
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 3f;
   

  

    private PlayerMotor motor;
 


    private void Awake()
    {
       // myBody = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start () {
        motor = GetComponent<PlayerMotor>();
        rotY = transform.localRotation.eulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () {
        playerMoveKeyBoard();
        AnimatePlayer();

    }

    void FixedUpdate()
    {
        MoveAndRotate();

    }

    void playerMoveKeyBoard()
    {
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveHorizontal = -1;
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            moveHorizontal = 0;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveHorizontal = 1;
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            moveHorizontal = 0;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            moveVertical = 1;
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            moveVertical = 0;
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveVertical = -1;
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            moveVertical = 0;
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
        Vector3 _cameraRotation = new Vector3(_xRot, 0f, 0f) * lookSensitivity;

        motor.RotateCamera(_cameraRotation);
        /*  if(moveVertical != 0)
          {
              myBody.MovePosition(transform.position + transform.forward * (moveVertical * speed));
          }

          rotY += moveHorizontal * rotationSpeed;
          myBody.rotation = Quaternion.Euler(0f, rotY, 0f);*/
    }

    void AnimatePlayer()
    {
        if (moveVertical != 0 || moveHorizontal!=0)
        {
            if (!isMoving)
            {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName(MyTags.WALK_ANIMATION))
                {
                    isMoving = true;
                    anim.SetTrigger(MyTags.WALK_TRIGGER);
                }
            }
        }
        else
        {

            if (isMoving)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName(MyTags.WALK_ANIMATION))
                {
                    isMoving = false;
                    anim.SetTrigger(MyTags.STOP_TRIGGER);
                }
            }
        }
    }
}
