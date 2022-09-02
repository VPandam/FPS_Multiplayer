using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponShopSlot : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    Image image;

    // Start is called before the first frame update
    void Start()
    {

        image = GetComponent<Image>();
        image.sprite = weaponSO.sprite; ;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
