using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    float verticalInput, horizontalInput;
    public CharacterController characterController;
    Vector3 direction;
    public float speed = 12;
    public float gravity = -9.81f;
    public float jumpHeight = 3;
    Vector3 yVelocity;
    public Transform isGroundedGO;
    bool isGrounded;
    [SerializeField]
    float checkGroundRadius;
    [SerializeField]
    public LayerMask groundLayer;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(isGroundedGO.position, checkGroundRadius, groundLayer);

        if (isGrounded && yVelocity.y < 0)
        {
            yVelocity.y = -2;
        }

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        direction = transform.right * horizontalInput + transform.forward * verticalInput;

        characterController.Move(direction * Time.deltaTime * speed);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        yVelocity.y += gravity * Time.deltaTime;

        characterController.Move(yVelocity * Time.deltaTime);


    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(isGroundedGO.position, checkGroundRadius);
    }
}
