using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum WeaponType { pistol, rifle, machinegun, shotgun }
public class Weapon : MonoBehaviour
{

    //Components
    [SerializeField] WeaponSO weaponSO;
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
    [SerializeField] GameObject hitCross;
    HitCross hitCrossScript;




    //Animations
    string animationAim = "isAiming";
    string animationReload = "isReloading";
    string animationShoot = "shoot";

    //The index of the animation layer we use when enabling this weapon.
    [SerializeField] int animationLayerIndex;


    //Buttons
    string reloadButton = "Reload";
    string fire1 = "Fire1";
    string aimButton = "Aim";

    //Shooting
    float range = 1000;
    bool hittingSomething;
    bool isAiming;
    RaycastHit hit;
    Vector3 targetDirection;
    string enemyTag = "Enemy";

    //Reload system
    int currentAmmo, currentReserveAmmo;

    public bool isReloading;
    [SerializeField] TextMeshProUGUI currentAmmoText, reserveAmmoText;

    //ShootRatio
    float nextShootTime;






    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        currentAmmo = weaponSO.maxAmmo;
        currentReserveAmmo = weaponSO.maxReserveAmmo;
        hitCrossScript = hitCross.GetComponent<HitCross>();
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
            (currentAmmo < weaponSO.maxAmmo && !isReloading && Input.GetButtonDown(reloadButton)))
            {
                StartCoroutine(Reload());
                return;
            }

            //If we are not reloading we can shoot or aim
            if (!isReloading)
            {
                //Called on update to check if we are hitting the Aim button, if so, change to aim mode.
                AimSystem();

                if (Input.GetButton(fire1) && Time.time >= nextShootTime && weaponSO.isAutomatic)
                {
                    Shoot();
                }
                if (Input.GetButtonDown(fire1) && Time.time >= nextShootTime && !weaponSO.isAutomatic)
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
        nextShootTime = shootTime + weaponSO.fireRate;

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
            ZombieManager enemyManager = gameObjectHitted.GetComponent<ZombieManager>();
            //If we hit an enemy
            if (enemyManager != null && enemyManager.isAlive)
            {
                float totalDamage = weaponSO.weaponDamage;

                //If we are using a shotgun we make double damage if the enemy is close
                if (weaponSO.weaponType == WeaponType.shotgun)
                {
                    if (Vector3.Distance(transform.position, enemyManager.transform.position) < 3)
                    {
                        totalDamage *= 2;
                    }
                }

                //If we make a headshot we double the total damage
                //Machinegun is so powerfull, so i made headshots half strong
                if (hit.collider.gameObject.name == "HeadCollider")
                    totalDamage *= weaponSO.weaponType == WeaponType.machinegun ? 1.5f : 2;

                //Make damage to the enemy
                enemyManager.TakeDamage(totalDamage);
                Debug.Log(totalDamage);


                //Activates a cross that shows we hitted the enemy.
                //If it is already active, reset the time to disable it
                if (hitCross.activeSelf)
                    hitCrossScript.RestartDisableCall();
                else
                    hitCross.SetActive(true);
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
            remainingDistance -= weaponSO.bulletSpeed * Time.deltaTime;

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

        //Unable the aim cross
        aimCross.enabled = false;

        isReloading = true;
        animator.SetBool(animationReload, true);
        //Wait for seconds equal to the reload time variable
        yield return new WaitForSeconds(weaponSO.reloadTime);
        isReloading = false;
        //enable the aim cross once we have finished reloading.
        aimCross.enabled = true;
        //Checks if we are aiming to chose the next animation
        AimSystem();

        animator.SetBool(animationReload, false);



        //Ammount of bullets to substra to reserve ammo in order to have the max current ammo
        int ammountToReload = weaponSO.maxAmmo - currentAmmo;

        //If there is enough reserve ammo
        //set the current ammo to the maximum and substract it from reserve.
        if (currentReserveAmmo - ammountToReload >= 0)
        {
            currentReserveAmmo -= weaponSO.maxAmmo - currentAmmo;
            currentAmmo = weaponSO.maxAmmo;
        }
        //Set the reserve to 0 and get all is left.
        else
        {
            currentAmmo = currentReserveAmmo;
            currentReserveAmmo = 0;
        }

    }
    public void CancelReload()
    {
        StopCoroutine("Reload");
        isReloading = false;
        //enable the aim cross once we have finished reloading.
        aimCross.enabled = true;
        //Checks if we are aiming to chose the next animation
        AimSystem();
        //Set the irReloading parameter to false
        animator.SetBool(animationReload, false);

    }

    void SetAmmoText()
    {
        //The current ammo text needs to have a slide at the end.
        currentAmmoText.text = $"{currentAmmo} /";
        reserveAmmoText.text = currentReserveAmmo.ToString();
    }

    //On enable the weapon set the layer weight of the animation layer to 1
    private void OnEnable()
    {
        animator.SetLayerWeight(animationLayerIndex, 1);
    }
    //On disable the weapon set the layer weight of the animation layer to 0
    private void OnDisable()
    {
        animator.SetLayerWeight(animationLayerIndex, 0);
    }



}
