
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour {

    [SerializeField]
    private Camera cam;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 CameraRotation = Vector3.zero;
 

    private Rigidbody rb;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
       
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;

    }

    void FixedUpdate()
    {
        PerformMovment();
        PerformRotation();
     
    }

    void PerformMovment()
    {
        if(velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }

      
    }

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    void PerformRotation()
    {
      
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            cam.transform.Rotate(-CameraRotation);
        }
    }


    public void RotateCamera(Vector3 _camerarotation)
    {
        CameraRotation = _camerarotation;
    }


   



}
