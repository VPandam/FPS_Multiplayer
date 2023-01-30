using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public Item itemSO;
    [SerializeField] private WeaponStats weaponSO;
    Image image;
    [SerializeField] PlayerManager playerManager;
    [SerializeField] VendingMachine vendingMachine;
    [SerializeField] private TextMeshProUGUI costText;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = itemSO.sprite;
        
        if (itemSO.itemType == ItemType.Weapon)
            if(costText && weaponSO) costText.text = weaponSO.cost.ToString();
        else
        {
            costText.enabled = false;
        }
    }

    public void SelectItem()
    {
        vendingMachine.SelectItem(this);
    }
}
