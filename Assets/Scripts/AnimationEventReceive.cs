using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceive : MonoBehaviour
{

    public ZombieController zombieController;
    
    //Called when the animation of the zombie makes him stretch his arm to attack
    public void MakeDamage()
    {
        zombieController.MakeDamage();
    }
}
