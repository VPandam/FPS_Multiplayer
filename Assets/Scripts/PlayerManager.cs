using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public float currentHealth = 100;

    public float maximumHealth = 100;

    [SerializeField]
    TextMeshProUGUI healthTMP;
    public bool isAlive;
    [SerializeField] CameraShake cameraShake;

    GameManager gameManager;

    [SerializeField] CanvasGroup takeDamageCG;
    [SerializeField] float damagedBlinkTime = 0.5f;


    //Weapon change
    [SerializeField] GameObject weaponHolder;
    int currentWeaponIndex;
    Weapon currentWeapon;

    void Start()
    {
        currentWeaponIndex = 0;
        currentWeapon = weaponHolder.transform.GetChild(currentWeaponIndex).GetComponent<Weapon>();
        currentHealth = maximumHealth;
        healthTMP.text = $"HP: {currentHealth.ToString()}";
        isAlive = true;
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
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
    public void Heal(int healAmmount)
    {
        currentHealth += healAmmount;
        UpdateHealthText();
    }

    void CheckMouseWheelInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            ChangeWeapon(currentWeaponIndex + 1);
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            ChangeWeapon(currentWeaponIndex - 1);

    }
    public void ChangeWeapon(int weaponIndex)
    {
        int index = 0;
        int ammountOfWeapons = weaponHolder.transform.childCount;

        if (currentWeapon.isReloading)
            currentWeapon.CancelReload();

        //If we try to reach an unreachable index restart the index to the minimum or maximum
        if (weaponIndex > ammountOfWeapons - 1)
            weaponIndex = 0;
        else if (weaponIndex < 0)
            weaponIndex = ammountOfWeapons - 1;

        Debug.Log(weaponIndex);
        foreach (Transform weapon in weaponHolder.transform)
        {
            //Activate the weapon with the right index and deactivate the others.
            weapon.gameObject.SetActive(index == weaponIndex);

            index++;
        }

        currentWeaponIndex = weaponIndex;
        currentWeapon = weaponHolder.transform.GetChild(currentWeaponIndex).GetComponent<Weapon>();
    }


}
