using UnityEngine;

public class Shotgun : MonoBehaviour, IWeapon
{
    float totalDamage;

    public float CalculateDamage(WeaponStats weaponSO, ZombieManager enemyManager, RaycastHit hit)
    {
        float baseDamage = weaponSO.weaponDamage;
        totalDamage = baseDamage;

        //If we are using a shotgun we make double damage if the enemy is close
        if (weaponSO.weaponType == WeaponType.shotgun &&
        (Vector3.Distance(transform.position, enemyManager.transform.position) < 12))
        {
            totalDamage = baseDamage * 5;
            Debug.Log("*5!!");
        }

        //If we make a headshot we double the total damage

        if (hit.collider.gameObject.name == "HeadCollider")
            totalDamage *= 2;

        return totalDamage;
    }
}
