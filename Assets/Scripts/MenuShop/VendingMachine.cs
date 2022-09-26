using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class VendingMachine : MonoBehaviourPunCallbacks
{
    ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
    [SerializeField] GameObject shopCanvas;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject doText;
    [SerializeField] GameObject firstSelectedButton;
    public bool isShopOpen = false;
    public PlayerManager _playerManager;
    public ShopSlot selectedShopSlot;
    EventSystem eventSystem;
    string isShopOpenKey = "isShopOpen";
    private void Start()
    {
        eventSystem = EventSystem.current;
    }


    private void OnTriggerEnter(Collider other)
    {
        doText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        doText.SetActive(false);
    }

    public void OpenShop(PlayerManager playerManager)
    {
        if (hash.ContainsKey(isShopOpenKey))
        {
            hash[isShopOpenKey] = true;
        }
        else
            hash.Add(isShopOpenKey, true);

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        gameManager = playerManager.gameObject.GetComponentInChildren<GameManager>();
        _playerManager = playerManager;
        Debug.Log("OpeningShop");
        shopCanvas.SetActive(true);
        eventSystem.SetSelectedGameObject(firstSelectedButton);
        gameManager.Shop();
    }

    public void ExitShop()
    {
        if (hash.ContainsKey(isShopOpenKey))
        {
            hash[isShopOpenKey] = false;
        }
        else
            hash.Add(isShopOpenKey, false);

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        Debug.Log("Close shop");
        shopCanvas.SetActive(false);
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
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps[isShopOpenKey] != null)
        {
            isShopOpen = (bool)changedProps[isShopOpenKey];
            doText.GetComponent<Text>().text = isShopOpen ? "Shop in use" : "Press E";

            Debug.Log(" isShopOpen = " + isShopOpen);
        }
    }
}

