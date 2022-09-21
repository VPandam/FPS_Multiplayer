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
    public bool isShopOpen = false;
    public PlayerManager _playerManager;
    public ShopSlot selectedShopSlot;
    EventSystem eventSystem;
    private void Start()
    {
        gameManager = GameManager.sharedInstance;
        eventSystem = EventSystem.current;
    }

    private void OnTriggerEnter(Collider other)
    {
        doText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        ExitShop();
        doText.SetActive(false);
    }

    public void OpenShop(PlayerManager playerManager)
    {
        _playerManager = playerManager;
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
        Debug.Log(_playerManager + " player manager vending machine");
        WeaponSO selectedWeaponSO = (WeaponSO)selectedShopSlot.itemSO;
        _playerManager.SetWeaponAvailable(selectedWeaponSO.weaponType);
        ExitShop();
    }
    public void BuyHeal()
    {
        _playerManager.Heal(true);
    }
    public void BuyAmmo()
    {
        _playerManager.BuyAmmo();
    }
}

