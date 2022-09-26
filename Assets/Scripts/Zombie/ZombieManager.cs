using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class ZombieManager : MonoBehaviour
{

    public float currentHealth;
    [SerializeField] float maxHealth;
    Collider _collider;
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
        _collider = GetComponent<Collider>();
    }
    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (gameManager.CurrentLocalGameState == GameState.inGame)
                {
                    GrowlAndRotateZombie();
                }
            }
        }
        else
        {
            if (gameManager.CurrentLocalGameState == GameState.inGame)
            {
                GrowlAndRotateZombie();
            }
        }
    }
    void GrowlAndRotateZombie()
    {
        HPSlider.transform.LookAt(zombieController.playerTarget.transform);

        if (counter >= CDGrowlTime && !audioSource.isPlaying && isAlive)
        {
            Growl();
        }
        counter += Time.deltaTime;
    }

    // Start is called before the first frame update

    [PunRPC]
    public void TakeDamage(float damageAmmount)
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
        _collider.enabled = false;
        if ((PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom) || !PhotonNetwork.InRoom)
            gameManager.LookForEnemies();
        Destroy(gameObject, 3);
    }
    void Growl()
    {
        audioSource.clip = growlClips[Random.Range(0, growlClips.Length)];
        audioSource.Play();
        counter = 0;
    }


}
