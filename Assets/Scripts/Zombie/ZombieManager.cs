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

    [SerializeField] AudioClip[] growlClips;
    AudioSource audioSource;
    float CDGrowlTime = 2;
    float counter = 0;


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
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (gameManager.CurrentGameState == GameState.inGame)
        {

            HPSlider.transform.LookAt(zombieController.playerTarget.transform);

            if (counter >= CDGrowlTime && !audioSource.isPlaying && isAlive)
            {
                Growl();
            }
            counter += Time.deltaTime;
        }


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
        gameManager.StartCoroutine(gameManager.LookForEnemies());
        Destroy(gameObject, 3);
    }
    void Growl()
    {
        audioSource.clip = growlClips[Random.Range(0, growlClips.Length)];
        audioSource.Play();
        counter = 0;
    }


}
