using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
public class PlayerManager : MonoBehaviour
{
    public float currentHealth = 100;

    public float maximumHealth = 100;

    [SerializeField] TextMeshProUGUI healthTMP;
    public bool isAlive;
    [SerializeField] CameraShake cameraShake;

    GameManager gameManager;

    [SerializeField] CanvasGroup takeDamageCG;
    [SerializeField] float damagedBlinkTime = 0.5f;


    //Weapon change
    [SerializeField] GameObject weaponHolder;
    Weapon currentWeapon;

    List<int> weaponsAvailableIndexes = new List<int>();
    int currentWeaponIndex;


    void Start()
    {
        gameManager = GameManager.sharedInstance;
        
        currentWeaponIndex = 0;
        currentWeapon = weaponHolder.transform.GetChild(currentWeaponIndex).GetComponent<Weapon>();
        currentHealth = maximumHealth;
        healthTMP.text = $"HP: {currentHealth.ToString()}";
        isAlive = true;

        //All weapons are deactivated by default except pistol.
        //When we buy a weapon we make it available
        SetWeaponAvailable(WeaponType.pistol);
    }

    // Update is called once per frame
    void Update()
    {
        if (takeDamageCG.alpha > 0)
        {
            takeDamageCG.alpha -= Time.deltaTime / damagedBlinkTime;
        }

        if (gameManager.CurrentGameState == GameState.inGame)
        {
            CheckMouseWheelInput();
        }


    }
    void UpdateHealthText()
    {
        healthTMP.text = $"HP: {currentHealth.ToString()}";
    }
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
}
