using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public Item itemSO;
    Image image;
    [SerializeField] PlayerManager playerManager;
    [SerializeField] VendingMachine vendingMachine;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = itemSO.sprite; ;
    }

    public void SelectItem()
    {
        vendingMachine.SelectItem(this);
    }
}
