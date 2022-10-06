using UnityEngine;

public class Pistol : MonoBehaviour, IWeapon
{
    float totalDamage;


    public float CalculateDamage(WeaponSO weaponSO, ZombieManager enemyManager, RaycastHit hit)
    {
        float baseDamage = weaponSO.weaponDamage;
        totalDamage = baseDamage;


        //If we make a headshot we double the total damage
        if (hit.collider.gameObject.name == "HeadCollider")
            totalDamage *= 2;

        return totalDamage;
    }


}