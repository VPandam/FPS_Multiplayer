using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VendingMachine : MonoBehaviour
{
    [SerializeField] GameObject shopCanvas;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject doText;
    bool isInRangeToOpenShop = false;
    bool isShopOpen = false;

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
        isShopOpen = true;
        gameManager.Shop();
    }

    public void ExitShop()
    {
        Debug.Log("Close door");
        shopCanvas.SetActive(false);
        isShopOpen = false;
        gameManager.Resume();
    }
}
