using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    float verticalInput, horizontalInput;
    public CharacterController characterController;
    PlayerManager playerManager;
    Vector3 direction;
    float speed;
    [SerializeField] float normalSpeed = 8;
    [SerializeField] float runSpeed = 12;
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
        playerManager = GetComponent<PlayerManager>();
        speed = normalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(isGroundedGO.position, checkGroundRadius, groundLayer);

        if (playerManager.isAlive)
        {
            SimulateGravity();

            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            direction = transform.right * horizontalInput + transform.forward * verticalInput;

            if (Input.GetButtonDown("Run"))
            {
                speed = runSpeed;
            }
            if (Input.GetButtonUp("Run"))
            {
                speed = normalSpeed;
            }

            //Move the player to the direction.
            characterController.Move(direction * Time.deltaTime * speed);

            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            }

            //Move the player on the y axis.
            //Y velocity depends on gravity and changes if we are jumping
            characterController.Move(yVelocity * Time.deltaTime);
        }


    }

    void SimulateGravity()
    {
        //Reset gravity if we are on the ground
        if (isGrounded && yVelocity.y < 0)
        {
            //We use -2 in order to avoid bugs related to 0 speed.
            yVelocity.y = -2;
        }

        //Increase yvelocity by the force of gravity every second
        yVelocity.y += gravity * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(isGroundedGO.position, checkGroundRadius);
    }
}
