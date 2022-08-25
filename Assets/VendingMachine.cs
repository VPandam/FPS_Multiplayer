using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] GameObject ShopCanvas;
    [SerializeField] GameManager gameManager;


    private void OnTriggerEnter(Collider other)
    {
        OpenShop();
    }

    private void OnTriggerExit(Collider other)
    {
        ExitShop();
    }

    void OpenShop()
    {
        ShopCanvas.SetActive(true);
        gameManager.Shop();
    }

    public void ExitShop()
    {
        ShopCanvas.SetActive(false);
        gameManager.Resume();
    }
}
