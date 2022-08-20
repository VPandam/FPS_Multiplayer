using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementMobile : MonoBehaviour
{

    float verticalInput, horizontalInput;
    public CharacterController characterController;
    PlayerManager playerManager;
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
    float screenWidth = Screen.width;
    int leftFingerID, rightFingerID;
    Vector3 direction;

    Vector2 moveTouchDirection;
    Vector2 moveTouchStart;



    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        leftFingerID = -1;
        rightFingerID = -1;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(isGroundedGO.position, checkGroundRadius, groundLayer);

        if (isGrounded && yVelocity.y < 0)
        {
            yVelocity.y = -2;
        }

        GetTouchInput();

        direction = transform.right * moveTouchDirection.normalized.x + transform.forward * moveTouchDirection.normalized.y;

        if (playerManager.isAlive && leftFingerID != -1)
            characterController.Move(direction * Time.deltaTime * speed);

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        yVelocity.y += gravity * Time.deltaTime;

        characterController.Move(yVelocity * Time.deltaTime);


    }


    void GetTouchInput()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                CheckTouchPhases(touch);

            }
        }
    }
    void CheckTouchPhases(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:

                //We move the player using the left finger.
                //We get if we are touching the left side of the screen
                bool touchedLeftSideScreen = touch.position.x < screenWidth / 2;
                bool touchedrightSideScreen = touch.position.x > screenWidth / 2;
                if (touchedLeftSideScreen && leftFingerID == -1)
                {
                    leftFingerID = touch.fingerId;
                    moveTouchStart = touch.position;

                }
                else if (touchedrightSideScreen && rightFingerID == -1)
                {
                    rightFingerID = touch.fingerId;
                }
                break;

            case TouchPhase.Moved:
                if (leftFingerID == touch.fingerId)
                {
                    moveTouchDirection = touch.position - moveTouchStart;
                }
                else if (rightFingerID == touch.fingerId)
                {

                }
                break;
            case TouchPhase.Canceled:
                break;
            case TouchPhase.Ended:
                if (leftFingerID == touch.fingerId)
                {
                    leftFingerID = -1;
                }
                break;
            case TouchPhase.Stationary:
                break;
            default:
                break;
        }
    }
}
