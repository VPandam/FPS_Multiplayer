using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public GameObject playerTarget;
    Vector3 target;
    float distanceToPlayer;
    NavMeshAgent navMeshAgent;

    ZombieManager zombieManager;

    //Animations
    [SerializeField]
    Animator animator;
    string isRunningBool = "isRunning", runningSpeed = "velocity",
    isAttackingTrigger = "isAttacking";



    [SerializeField]
    float rangeToMove = 16;

    //Stats
    [SerializeField]
    float attackDamage = 20;

    [SerializeField]
    float minMovementSpeed = 2, maxMovementSpeed = 4;

    [SerializeField]
    float minAttackSpeed = 2, maxAttackSpeed = 4;
    float attackSpeed;

    //Equals to the stopping distance of the nav mesh agent
    float rangeToAttack;

    float counterAttack;
    bool isInRangeToMove, isInRangeToAttack, isAlive;


    float maxSpeed;
    NavMeshPath path;
    [SerializeField] float armLength = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        zombieManager = gameObject.GetComponent<ZombieManager>();

        playerTarget = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = Random.Range(minMovementSpeed, maxMovementSpeed);
        maxSpeed = navMeshAgent.speed;
        path = new NavMeshPath();

        rangeToAttack = navMeshAgent.stoppingDistance + 0.1f;
        attackSpeed = Random.Range(minAttackSpeed, maxAttackSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        //In order to move the enemy even when the player is jumping, we set the target vector y to 0.
        target = new Vector3(playerTarget.transform.position.x, 0, playerTarget.transform.position.z);
        distanceToPlayer = Vector3.Distance(transform.position, target);
        isInRangeToMove = (distanceToPlayer < rangeToMove);
        isInRangeToAttack = distanceToPlayer <= rangeToAttack;
        isAlive = zombieManager.isAlive;

        Movement();
        Attack();

        counterAttack += Time.deltaTime;
    }

    void Movement()
    {

        //Sets the speed of the animation to the zombie speed
        animator.SetFloat(runningSpeed, navMeshAgent.velocity.magnitude / maxSpeed);

        //Checks if the player is in a reachable place
        navMeshAgent.CalculatePath(target, path);

        //move to the player direction if the player is in a reachable place,
        //the distance to the player is bigger than the attack range and lower than the moveRange,
        //and the zombie is not dead.
        if (isInRangeToMove && path.status == NavMeshPathStatus.PathComplete &&
        !isInRangeToAttack && isAlive)
        {
            animator.SetBool(isRunningBool, true);
            navMeshAgent.SetDestination(target);
        }
        else
        {
            animator.SetBool(isRunningBool, false);
            navMeshAgent.SetDestination(transform.position);
        }
    }
    void Attack()
    {
        if (isInRangeToAttack && isAlive)
        {
            //If is in range to attack we need to manually rotate our enemy to face the player
            FaceTarget();

            if (counterAttack >= attackSpeed)
            {
                animator.SetTrigger(isAttackingTrigger);
                counterAttack = 0;
                //The damage is made by an event in the animation which calls MakeDamage
            }
        }
    }
    void FaceTarget()
    {
        Vector3 lookDirection = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));
        transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    //It's called when the zombie stretches his arm to attack
    public void MakeDamage()
    {
        if (distanceToPlayer <= rangeToAttack + armLength)
            playerTarget.GetComponent<PlayerManager>().TakeDamage(attackDamage);
    }
}
