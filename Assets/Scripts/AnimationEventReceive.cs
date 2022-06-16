using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceive : MonoBehaviour
{
    public ZombieController zombieController;
    public void MakeDamage()
    {
        zombieController.MakeDamage();
    }
}
