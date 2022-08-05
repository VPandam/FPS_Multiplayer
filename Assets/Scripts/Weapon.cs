using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Weapon : MonoBehaviour
{
    //Components
    [SerializeField] GameObject cameraGO;
    GameManager gameManager;
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem flashShot;
    AudioSource audioSource;
    [SerializeField] AudioClip shotClip;

    //Canvas
    [SerializeField] Image aimCross;
    [SerializeField] Sprite normalCross, redCross;
    [SerializeField] CameraShake cameraShake;
    [SerializeField] TrailRenderer trail;
    [SerializeField] GameObject trailGO;

    //Stats
    float range = 1000;
    public int weaponDamage = 10;
    [SerializeField] float bulletSpeed = 100;
    [SerializeField] bool isAutomatic;

    RaycastHit hit;
    Vector3 targetDirection;
    string enemyTag = "Enemy";

    //Animations
    string animationAim = "isAiming";
    string animationReload = "isReloading";
    string animationShoot = "shoot";

    //Buttons
    string reloadButton = "Reload";
    string fire1 = "Fire1";
    string aimButton = "Aim";

    bool hittingSomething;
    bool isAiming;

    //Reload system
    int currentAmmo, currentReserveAmmo;
    [SerializeField] int maxAmmo = 30, maxReserveAmmo = 99;
    [SerializeField] float reloadTime = 2;
    bool isReloading;
    [SerializeField] TextMeshProUGUI currentAmmoText, reserveAmmoText;

    //ShootRatio
    [SerializeField]
    float fireRate = 0.3f;
    float nextShootTime;




    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        currentAmmo = maxAmmo;
        currentReserveAmmo = maxReserveAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        SetAmmoText();
        if (gameManager.CurrentGameState == GameState.inGame)
        {
            //Reset the shoot bool to false
            animator.SetBool(animationShoot, false);

            //Raycast from the player to forward.
            hittingSomething = Physics.Raycast(cameraGO.transform.position,
             cameraGO.transform.forward, out hit, range);

            if (hittingSomething)
            {
                //If we are hitting something with the enemy tag and is alive change the aim cross sprite.
                ChangeCrossColor();
            }

            //Reload when our current ammo is 0.
            if ((currentAmmo <= 0 && !isReloading) ||
            (currentAmmo < maxAmmo && !isReloading && Input.GetButtonDown(reloadButton)))
            {
                StartCoroutine(Reload());
                return;
            }

            //If we are not reloading we can shoot or aim
            if (!isReloading)
            {
                //Called on update to check if we are hitting the Aim button, if so, change to aim mode.
                AimSystem();

                if (Input.GetButton(fire1) && Time.time >= nextShootTime && isAutomatic)
                {
                    Shoot();
                }
                if (Input.GetButtonDown(fire1) && Time.time >= nextShootTime && !isAutomatic)
                {
                    Shoot();
                }
            }
        }
    }

    void Shoot()
    {
        currentAmmo -= 1;

        //Calculate next shoot time
        float shootTime = Time.time;
        nextShootTime = shootTime + fireRate;

        //Camera shake
        cameraShake.StartCoroutine(cameraShake.Shake(0.1f, 0.2f));

        //Audio shot effect
        audioSource.PlayOneShot(shotClip, 0.5f);

        //Light effect on shoot
        flashShot.Play();

        //Trail effect simulating the bullet
        TrailRenderer trailInstance = Instantiate(trail, trailGO.transform.position, Quaternion.identity);

        //Move the trail towards the hitted object
        if (hittingSomething)
            StartCoroutine(MoveTrial(trailInstance, hit.point));
        else
            //Move the trail forwards
            StartCoroutine(MoveTrial(trailInstance, cameraGO.transform.forward * 100));

        animator.SetBool(animationShoot, true);

        if (hittingSomething)
        {
            GameObject gameObjectHitted = hit.transform.gameObject;
            //If we hit an enemy
            if (gameObjectHitted.CompareTag(enemyTag))
            {
                //Make damage to the enemy
                gameObjectHitted.GetComponent<ZombieManager>().TakeDamage(
                hit.collider.gameObject.name == "HeadCollider" ? weaponDamage * 2 : weaponDamage);
            }
        }
    }

    IEnumerator MoveTrial(TrailRenderer trailToMove, Vector3 destiny)
    {
        //The remaining distance until reach destiny
        float remainingDistance = 0;

        //The total distance between trail and destiny
        float distanceToHit = Vector3.Distance(trailToMove.transform.position, destiny);
        //At the start, the remaining distance is equal to the total distance.
        remainingDistance = distanceToHit;

        //While the remaining distance is not 0
        while (remainingDistance > 0)
        {
            //Move the trial until we reach the destiny
            trailToMove.transform.position = Vector3.Lerp(trailToMove.transform.position,
            destiny, 1 - (remainingDistance / distanceToHit));

            //Every frame we substract some distance depending of the bullet speed.
            remainingDistance -= bulletSpeed * Time.deltaTime;

            yield return null;
        }

        trailToMove.transform.position = destiny;

        Destroy(trailToMove.gameObject, trailToMove.time);

    }

    /// <summary>
    /// Called on update to change the aim cross color if we are pointing to an enemy
    /// </summary>
    void ChangeCrossColor()
    {
        if (!isAiming)
        {
            //If we are pointing an enemy, show the red cross either the black one
            aimCross.sprite = hit.transform.CompareTag(enemyTag) && hit.transform.gameObject.GetComponent<ZombieManager>().isAlive
            ? redCross : normalCross;

            //If we are hitting an enemy change the color to white
            aimCross.color = hit.transform.CompareTag(enemyTag) && hit.transform.gameObject.GetComponent<ZombieManager>().isAlive
            ? Color.white : Color.black;
        }
    }

    /// <summary>
    /// Called on update to check if we are hitting the Aim button, if so, change to aim mode.
    /// </summary>
    void AimSystem()
    {
        if (Input.GetButton(aimButton))
            SetAimMode(true);
        else
            SetAimMode(false);

    }

    /// <summary>
    /// Set needed parameters to consider when changing the aim mode
    /// </summary>
    /// <param name="aimMode">True if aiming, false if not</param>
    void SetAimMode(bool aimMode)
    {
        if (aimMode == true)
        {
            animator.SetBool(animationAim, true);
            isAiming = true;
            aimCross.enabled = false;
        }
        else
        {
            animator.SetBool(animationAim, false);
            isAiming = false;
            aimCross.enabled = true;
        }
    }

    IEnumerator Reload()
    {
        // //If we were aiming when reload starts we need to reset it.
        // SetAimMode(false);
        //unable the aim cross
        aimCross.enabled = false;


        isReloading = true;
        animator.SetBool(animationReload, true);
        //Wait for seconds equal to the reload time variable
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;
        //enable the aim cross once we have finished reloading.
        aimCross.enabled = true;
        //Checks if we are aiming to chose the next animation
        AimSystem();

        animator.SetBool(animationReload, false);



        //Ammount of bullets to substra to reserve ammo in order to have the max current ammo
        int ammountToReload = maxAmmo - currentAmmo;

        //If there is enough reserve ammo
        //set the current ammo to the maximum and substract it from reserve.
        if (currentReserveAmmo - ammountToReload >= 0)
        {
            currentReserveAmmo -= maxAmmo - currentAmmo;
            currentAmmo = maxAmmo;
        }
        //Set the reserve to 0 and get all is left.
        else
        {
            currentAmmo = currentReserveAmmo;
            currentReserveAmmo = 0;
        }

    }

    void SetAmmoText()
    {
        //The current ammo text needs to have a slide at the end.
        currentAmmoText.text = $"{currentAmmo} /";
        reserveAmmoText.text = currentReserveAmmo.ToString();
    }



}
