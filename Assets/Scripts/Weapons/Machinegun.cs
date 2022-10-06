using UnityEngine;

public class Machinegun : MonoBehaviour, IWeapon
{
    float totalDamage;

    public float CalculateDamage(WeaponStats weaponSO, ZombieManager enemyManager, RaycastHit hit)
    {
        float baseDamage = weaponSO.weaponDamage;
        totalDamage = baseDamage;

        //If we make a headshot we normally double the total damage,
        //Since machinegun is so strong, we do 1.5x
        if (hit.collider.gameObject.name == "HeadCollider")
            totalDamage *= 1.5f;

        return totalDamage;
    }
}
