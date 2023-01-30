using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public enum WeaponType { pistol, rifle, machinegun, shotgun, sniper }
public interface IWeapon
{
    //Calculates the damage, depending of the weapon we are using,
    // the distance, or where do we hit the enemy.
    public float CalculateDamage(WeaponStats weaponSO, ZombieManager enemyManager, RaycastHit hit);
};
public class WeaponController : MonoBehaviour
{

    //Components
    public WeaponStats weaponSO;
    [SerializeField] GameObject cameraGO;
    [SerializeField] Camera mainCamera;
    float mainCameraFOV;
    [SerializeField] GameManager gameManager;
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem flashShot;
    MeshRenderer weaponMeshRenderer;
    AudioSource audioSource;

    //Canvas
    [SerializeField] Image aimCross;
    [SerializeField] Sprite normalCross, redCross;
    [SerializeField] CameraShake cameraShake;
    [SerializeField] TrailRenderer trail;
    [SerializeField] GameObject trailGO;
    [SerializeField] GameObject hitCross;
    [SerializeField] GameObject scopeOverlay;
    HitCross hitCrossScript;




    //Animations
    string animationAim = "isAiming";
    string animationReload = "isReloading";
    string animationShoot = "shoot";




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
    public bool isScoping;

    //Reload system
    int currentAmmo, currentReserveAmmo;

    public bool isReloading;
    [SerializeField] TextMeshProUGUI currentAmmoText, reserveAmmoText;
    //ShootRatio
    float nextShootTime;


    //True when we bought the weapon in the shop
    public bool isAvailable;

    //Index of the position in the weapon holder
    public int indexPosition;


    [SerializeField] PhotonView photonView;

    //Layer to ignore on shooting
    public LayerMask layerToIgnore;
    IWeapon IweaponInterface;

    //AudioClips


    public void SetIndexPosition()
    {
        indexPosition = ((int)weaponSO.weaponType);
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = weaponSO.maxAmmo;
        currentReserveAmmo = weaponSO.maxReserveAmmo;
        hitCrossScript = hitCross.GetComponent<HitCross>();
        weaponMeshRenderer = GetComponent<MeshRenderer>();
        mainCamera = cameraGO.GetComponentInChildren<Camera>();
        mainCameraFOV = mainCamera.fieldOfView;
        IweaponInterface = GetComponent<IWeapon>();
    }
    private void FixedUpdate()
    {
        //Raycast from the player to forward.
        hittingSomething = Physics.Raycast(cameraGO.transform.position,
         cameraGO.transform.forward, out hit, range, ~layerToIgnore);
    }


    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        SetAmmoText();
        if (gameManager.CurrentLocalGameState == GameState.inGame)
        {
            //Reset the shoot bool to false
            animator.SetBool(animationShoot, false);

            if (hittingSomething)
            {
                //If we are hitting something with the enemy tag and is alive change the aim cross sprite.
                ChangeCrossColor();
            }

            //Reload when our current ammo is 0.
            if ((currentAmmo <= 0 && !isReloading && currentReserveAmmo > 0) ||
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

                //Check if the fire rate of the weapon allows us to shoot. If the weapon is automatic we can let 
                //the button pressed to shoot over and over.
                if (Input.GetButton(fire1) && Time.time >= nextShootTime && weaponSO.isAutomatic && currentAmmo > 0)
                {
                    Shoot();
                }
                if (Input.GetButtonDown(fire1) && Time.time >= nextShootTime && !weaponSO.isAutomatic && currentAmmo > 0)
                {
                    Shoot();
                }
            }

        }
    }


    void Shoot()
    {
        ZombieManager enemyManager = null;
        currentAmmo -= 1;

        //Calculate next shoot time
        float shootTime = Time.time;
        nextShootTime = shootTime + weaponSO.fireRate;

        //Camera shake
        cameraShake.StartCoroutine(cameraShake.Shake(0.1f, 0.2f));

        //Audio shot effect
        if (weaponSO.shotClip)
            audioSource.PlayOneShot(weaponSO.shotClip, 0.5f);

        //Light effect on shoot
        flashShot.Play();

        //Trail effect simulating the bullet
        TrailRenderer trailInstance = Instantiate(trail, trailGO.transform.position, Quaternion.identity);

        //Move the trail towards the hitted object
        if (hittingSomething)
        {
            enemyManager = hit.transform.gameObject.GetComponent<ZombieManager>();
            StartCoroutine(MoveTrial(trailInstance, hit.point));
        }
        else
            //Move the trail forwards
            StartCoroutine(MoveTrial(trailInstance, cameraGO.transform.forward * 100));

        animator.SetBool(animationShoot, true);


        if (enemyManager != null)
        {
            DamageTheEnemy(hit, enemyManager, false);

            //Activates a cross that shows we hitted the enemy.
            //If it is already active, reset the time to disable it
            if (hitCross.activeSelf)
                hitCrossScript.RestartDisableCall();
            else
                hitCross.SetActive(true);

            if (weaponSO.collateral)
                Collateral();
        }
    }

    /// <summary>
    /// Make damage to the enemy passed by parameter.
    /// If call this when a collateral is made, we made half damage.
    /// </summary>
    /// <param name="enemyHitted"></param>
    /// <param name="isCollateral"></param>
    void DamageTheEnemy(RaycastHit enemyHit, ZombieManager enemyManager, bool isCollateral)
    {

        //If we hit an enemy
        if (enemyManager != null && enemyManager.isAlive)
        {
            //Calculates the damage, depending of the weapon we are using,
            // the distance, or where do we hit the enemy.
            float totalDamage = IweaponInterface.CalculateDamage(weaponSO, enemyManager, enemyHit);
            totalDamage *= isCollateral ? 0.5f : 1;

            //Update points, if we are hitting a headshot points are 50 else 10.
            //Instantiate points animation text.
            PlayerManager localPlayerInstance = PlayerManager.LocalPlayerInstance;
            if(localPlayerInstance) localPlayerInstance.UpdatePoints(enemyHit.collider.gameObject.CompareTag("Headshot")? 50 : 10);
            
            
            
            //Make damage to the enemy
            if (PhotonNetwork.InRoom && photonView.IsMine)
                enemyManager.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, totalDamage);
            else if (!PhotonNetwork.InRoom)
                enemyManager.TakeDamage(totalDamage);
        }
    }

    /// <summary>
    /// Throw a new raycast from the position of the last hit and checks if another enemy is hitted.
    /// If so, damage the enemy.
    /// </summary>
    void Collateral()
    {
        RaycastHit collateralHit;
        Physics.Raycast(hit.point, cameraGO.transform.forward, out collateralHit, range, ~layerToIgnore);
        ZombieManager enemyHitted = collateralHit.transform.gameObject.GetComponent<ZombieManager>();
        if (enemyHitted != null)
            DamageTheEnemy(collateralHit, enemyHitted, true);
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
    public void SetAimMode(bool aimMode)
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
            if (weaponSO.weaponType == WeaponType.sniper)
            {
                StopScoping();
            }
            isAiming = false;
            aimCross.enabled = true;
        }
    }
    public void StopScoping()
    {
        isScoping = false;
        scopeOverlay.SetActive(false);
        weaponMeshRenderer.enabled = true;
        mainCamera.fieldOfView = mainCameraFOV;
    }

    IEnumerator Reload()
    {
        //Unable the aim cross
        aimCross.enabled = false;

        if (weaponSO.weaponType == WeaponType.sniper)
        {
            StopScoping();
        }
        isReloading = true;
        animator.SetBool(animationReload, true);

        //If we are scoping with the sniper, stop scoping

        PlayRechargeSounds();
        //The time to wait for recharge of the shotgun depends on how many bullets
        //we have left. The EndReload call is made when we finish playing the recharge sounds
        if (weaponSO.weaponType != WeaponType.shotgun)
        {
            yield return new WaitForSeconds(weaponSO.reloadTime);
            EndReload();
        }
    }

    /// <summary>
    /// Ends the reloading animation and set the current ammo.
    /// </summary>
    public void EndReload()
    {
        //Wait for seconds equal to the reload time variable
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
    /// <summary>
    /// Stops the reload coroutine 
    /// </summary>
    public void CancelReload()
    {
        StopCoroutine("Reload");
        isReloading = false;
        //enable the aim cross once we have finished reloading.
        aimCross.enabled = true;
        //Checks if we are aiming to chose the next animation
        AimSystem();
        //Set the Reloading parameter to false
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
        animator.SetLayerWeight(weaponSO.animationLayerIndex, 1);
    }
    //On disable the weapon set the layer weight of the animation layer to 0
    private void OnDisable()
    {
        animator.SetLayerWeight(weaponSO.animationLayerIndex, 0);
    }

    public void SetAmmoToMax()
    {
        currentReserveAmmo = weaponSO.maxReserveAmmo;
    }

    /// <summary>
    /// Depending on the gun we play one or more clips to reload
    /// If we play more than one, we use a coroutine to play them one by one.
    /// </summary>
    public void PlayRechargeSounds()
    {
        AudioClip[] rechargeClips;
        rechargeClips = weaponSO.rechargeClips;
        if (rechargeClips.Length > 0)
        {
            if (rechargeClips.Length == 1)
            {
                audioSource.PlayOneShot(rechargeClips[0], 0.5f);
            }
            else
            {
                StartCoroutine(Playsounds(rechargeClips, audioSource));
            }
        }
        else
        {
            return;
        }
    }

    public IEnumerator Playsounds(AudioClip[] audioClips, AudioSource audioSource)
    {
        //For the shotgun we need to play one recharge sound for each bullet
        if (weaponSO.weaponType == WeaponType.shotgun)
        {

            audioSource.PlayOneShot(audioClips[0], 0.5f);
            yield return new WaitForSeconds(audioClips[0].length);

            for (int x = 0; x < weaponSO.maxAmmo - currentAmmo; x++)
            {
                audioSource.PlayOneShot(audioClips[1], 0.5f);
                yield return new WaitForSeconds(audioClips[1].length);
            }
            EndReload();
        }
        //If our active gun is not a shotgun
        else
        {
            //Play them one by one
            for (int i = 0; i < audioClips.Length; i++)
            {
                audioSource.PlayOneShot(audioClips[i], 0.5f);
                yield return new WaitForSeconds(audioClips[i].length);
            }
        }
    }

}
