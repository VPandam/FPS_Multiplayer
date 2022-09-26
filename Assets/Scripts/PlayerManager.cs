using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Photon.Pun;
public class PlayerManager : MonoBehaviour
{
    public static GameObject LocalPlayerInstance;
    public float currentHealth = 100;

    public float maximumHealth = 100;

    [SerializeField] TextMeshProUGUI healthTMP;
    public bool isAlive;
    [SerializeField] CameraShake cameraShake;

    [SerializeField] GameManager gameManager;

    [SerializeField] CanvasGroup takeDamageCG;
    [SerializeField] float damagedBlinkTime = 0.5f;


    //Weapon change
    [SerializeField] GameObject weaponHolder;
    Weapon currentWeapon;

    List<int> weaponsAvailableIndexes = new List<int>();
    int currentWeaponIndex;
    VendingMachine vendingMachine;

    public PhotonView photonView;

    private void Awake()
    {
        if (photonView.IsMine)
            LocalPlayerInstance = this.gameObject;
    }
    void Start()
    {
        //All weapons are deactivated by default except pistol.
        //When we buy a weapon we make it available
        currentWeaponIndex = 0;
        currentWeapon = weaponHolder.transform.GetChild(currentWeaponIndex).GetComponent<Weapon>();
        Debug.Log(currentWeapon + " Current weapon " + weaponHolder.transform.GetChild(currentWeaponIndex).name);
        SetWeaponAvailable(WeaponType.pistol);
        currentHealth = maximumHealth;
        healthTMP.text = $"HP: {currentHealth.ToString()}";
        isAlive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (takeDamageCG.alpha > 0)
        {
            takeDamageCG.alpha -= Time.deltaTime / damagedBlinkTime;
        }

        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        if (gameManager.CurrentLocalGameState == GameState.inGame)
        {
            CheckMouseWheelInput();
        }

        //If we are in range of a vending machine check input to open or close it
        if (vendingMachine != null)
        {
            if (Input.GetKeyDown(KeyCode.E) && !vendingMachine.isShopOpen)
            {
                Debug.Log(vendingMachine.isShopOpen);
                vendingMachine.OpenShop(this);
            }

        }


    }
    void UpdateHealthText()
    {
        healthTMP.text = $"HP: {currentHealth.ToString()}";
    }
[PunRPC]
    public void TakeDamage(float damage)
    {

        cameraShake.StartCoroutine(cameraShake.Shake(0.3f, 0.4f));
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maximumHealth);
        takeDamageCG.alpha = 1;
        UpdateHealthText();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        if (isAlive)
        {

            Debug.Log("Muelto");
            isAlive = false;
            gameManager.GameOver();
        }
    }
    public void Heal(float healAmmount)
    {
        currentHealth += healAmmount;
        UpdateHealthText();
    }
    public void Heal(bool max)
    {
        if (max)
            currentHealth = maximumHealth;
        UpdateHealthText();
    }

    void CheckMouseWheelInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (currentWeaponIndex + 1 < weaponsAvailableIndexes.Count)
            {
                ChangeWeapon(weaponsAvailableIndexes[currentWeaponIndex + 1]);
            }
            else
                ChangeWeapon(weaponsAvailableIndexes.First());
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            if (currentWeaponIndex - 1 >= 0)
                ChangeWeapon(weaponsAvailableIndexes[currentWeaponIndex - 1]);
            else
                ChangeWeapon(weaponsAvailableIndexes.Last());

    }

    public void ChangeWeapon(int weaponsAvailableIndex)
    {
        // CheckWeaponsAvailable();
        Debug.Log(currentWeapon + " Current weapon on change weapon");
        if (currentWeapon.isReloading)
            currentWeapon.CancelReload();


        //Activate the weapon with the right index and deactivate the others.
        foreach (Weapon weapon in weaponHolder.GetComponentsInChildren<Weapon>(true))
        {
            if (weaponsAvailableIndex == weapon.indexPosition && weapon.isAvailable)
            {
                weapon.gameObject.SetActive(true);
                currentWeapon = weapon;
                currentWeaponIndex = weaponsAvailableIndexes.IndexOf(weaponsAvailableIndex);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }
        }

    }

    void AddWeaponIndexToAvailable(int indexPosition)
    {
        if (!weaponsAvailableIndexes.Contains(indexPosition))
            weaponsAvailableIndexes.Add(indexPosition);
    }

    /// <summary>
    //Called through the shop interface when a weapon is bought
    //Loop through all the weapons and make available the one that matches
    //the weaponType passed by parameter
    //Add it to available weapons
    //Change to that weapon 
    /// </summary>
    /// <param name="weaponTypeToSetAvailable"></param>
    public void SetWeaponAvailable(WeaponType weaponTypeToSetAvailable)
    {

        foreach (Weapon weapon in weaponHolder.GetComponentsInChildren<Weapon>(true))
        {
            if (weapon.weaponSO.weaponType == weaponTypeToSetAvailable)
            {
                weapon.isAvailable = true;
                AddWeaponIndexToAvailable(weapon.indexPosition);
                ChangeWeapon(weapon.indexPosition);
            }
        }
    }
    public void BuyAmmo()
    {
        foreach (Weapon weapon in weaponHolder.GetComponentsInChildren<Weapon>(true))
        {
            if (weapon.isAvailable)
            {
                weapon.SetAmmoToMax();
            }
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "VendingMachine")
        {
            vendingMachine = other.gameObject.GetComponent<VendingMachine>();
            gameManager.vendingMachine = vendingMachine;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "VendingMachine")
        {
            vendingMachine = null;
            gameManager.vendingMachine = null;
        }
    }
}
