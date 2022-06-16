using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ZombieManager : MonoBehaviour
{

    public int currentHealth;
    [SerializeField]
    int maxHealth;
    public bool isAlive;
    [SerializeField] Slider HPSlider;
    ZombieController zombieController;


    public GameManager gameManager;

    //Animations
    public Animator animator;
    string dieAnimationTrigger = "isDead";
    void Start()
    {
        zombieController = GetComponent<ZombieController>();
        currentHealth = maxHealth;
        isAlive = true;
        HPSlider.value = currentHealth / maxHealth;
    }
    private void Update()
    {
        HPSlider.transform.LookAt(zombieController.playerTarget.transform);
    }

    // Start is called before the first frame update

    public void TakeDamage(int damageAmmount)
    {
        currentHealth -= damageAmmount;
        HPSlider.value = (float)currentHealth / (float)maxHealth;
        if (currentHealth <= 0 && isAlive)
            Die();

    }

    void Die()
    {
        isAlive = false;
        HPSlider.gameObject.SetActive(false);
        animator.SetTrigger(dieAnimationTrigger);
        IEnumerator lookForEnemiesCoroutine = gameManager.LookForEnemies();
        gameManager.StartCoroutine(lookForEnemiesCoroutine);
        Destroy(gameObject, 3);
    }
}
