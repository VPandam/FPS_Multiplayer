using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    GameObject cameraGO;

    //Stats
    float range = 1000;

    public int weaponDamage = 10;

    RaycastHit hit;
    Vector3 targetDirection;
    string enemyTag = "Enemy";

    [SerializeField]
    Image aimCross;

    [SerializeField]
    Sprite normalCross, redCross;
    bool hittingSomething;
    [SerializeField]
    Animator animator;
    [SerializeField] CameraShake cameraShake;
    [SerializeField] TrailRenderer trail;

    [SerializeField] float bulletSpeed = 100;
    [SerializeField] ParticleSystem flashShot;

    bool shoot;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("shoot", false);

        //Raycast from the player to forward.
        hittingSomething = Physics.Raycast(cameraGO.transform.position,
         transform.forward, out hit, range);

        if (hittingSomething)
        {

            //If we are hitting something with the enemy tag and is alive change the aim cross sprite.
            aimCross.sprite = hit.transform.CompareTag(enemyTag)
            && hit.transform.gameObject.GetComponent<ZombieManager>().isAlive
            ? redCross : normalCross;

            aimCross.color = hit.transform.CompareTag(enemyTag)
            && hit.transform.gameObject.GetComponent<ZombieManager>().isAlive
            ? Color.white : Color.black;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        cameraShake.StartCoroutine(cameraShake.Shake(0.1f, 0.2f));

        flashShot.Play();
        TrailRenderer trailInstance = Instantiate(trail, transform.position, Quaternion.identity);
        if (hittingSomething)
            StartCoroutine(MoveTrial(trailInstance, hit.point));
        else
            StartCoroutine(MoveTrial(trailInstance, cameraGO.transform.forward * 100));


        animator.SetBool("shoot", true);
        if (hittingSomething)
        {
            GameObject gameObjectHitted = hit.transform.gameObject;
            //If we hit an enemy
            if (gameObjectHitted.CompareTag(enemyTag))
            {
                Debug.Log(hit.collider.gameObject.name);
                //Make damage to the enemy
                gameObjectHitted.GetComponent<ZombieManager>().TakeDamage(
                hit.collider.gameObject.name == "HeadCollider" ? weaponDamage * 2 : weaponDamage);
            }
        }
    }
    IEnumerator MoveTrial(TrailRenderer trailToMove, Vector3 destiny)
    {
        float remainingDistance = 0;
        float distanceToHit = Vector3.Distance(trailToMove.transform.position, destiny);
        remainingDistance = distanceToHit;
        while (remainingDistance > 0)
        {
            trailToMove.transform.position = Vector3.Lerp(trailToMove.transform.position,
            destiny, 1 - (remainingDistance / distanceToHit));

            remainingDistance -= bulletSpeed * Time.deltaTime;

            yield return null;
        }

        trailToMove.transform.position = destiny;

        Destroy(trailToMove.gameObject, trailToMove.time);

    }
}
