using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class VendingMachine : MonoBehaviour
{
    [SerializeField] GameObject shopCanvas;
    GameManager gameManager;
    [SerializeField] GameObject doText;
    [SerializeField] GameObject firstSelectedButton;
    bool isInRangeToOpenShop = false;
    bool isShopOpen = false;
    public PlayerManager playerManager;
    public ShopSlot selectedShopSlot;
    EventSystem eventSystem;
    private void Start()
    {
        gameManager = GameManager.sharedInstance;
        eventSystem = EventSystem.current;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isInRangeToOpenShop && !isShopOpen)
            OpenShop();
        else
        if (Input.GetKeyDown(KeyCode.E) && isInRangeToOpenShop && isShopOpen)
            ExitShop();
    }
    private void OnTriggerEnter(Collider other)
    {
        isInRangeToOpenShop = true;
        doText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        ExitShop();
        doText.SetActive(false);
        isInRangeToOpenShop = false;
    }

    void OpenShop()
    {
        Debug.Log("OpeningShop");
        shopCanvas.SetActive(true);
        eventSystem.SetSelectedGameObject(firstSelectedButton);
        isShopOpen = true;
        gameManager.Shop();
    }

    public void ExitShop()
    {
        Debug.Log("Close shop");
        shopCanvas.SetActive(false);
        isShopOpen = false;
        gameManager.Resume();
    }

    public void SelectItem(ShopSlot shopSlot)
    {
        selectedShopSlot = shopSlot;
    }
    public void BuyItem()
    {
        if (selectedShopSlot != null)
        {
            switch (selectedShopSlot.itemSO.itemType)
            {
                case ItemType.Weapon:
                    BuyWeapon();
                    break;
                case ItemType.Ammo:
                    BuyAmmo();
                    break;
                case ItemType.Heal:
                    BuyHeal();
                    break;

            }
        }

    }
    public void BuyWeapon()
    {
        WeaponSO selectedWeaponSO = (WeaponSO)selectedShopSlot.itemSO;
        playerManager.SetWeaponAvailable(selectedWeaponSO.weaponType);
    }
    public void BuyHeal()
    {
        playerManager.Heal(true);
    }
    public void BuyAmmo()
    {
        playerManager.BuyAmmo();
    }
}

