using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ItemType { Ammo, Weapon, Heal };

[CreateAssetMenu(menuName = "Item"),]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public ItemType itemType;
}


