using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ItemWeapon"),]
public class WeaponSO : Item
{

    public WeaponType weaponType;

    [Header("Stats")]
    public int weaponDamage = 10;
    public float bulletSpeed = 100;
    public bool isAutomatic;
    public float fireRate = 0.3f;
    public int maxAmmo = 30, maxReserveAmmo = 99;
    public float reloadTime = 2;
    //The index of the animation layer we use when enabling this weapon.
    public int animationLayerIndex;


}
