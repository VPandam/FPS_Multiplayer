using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public float currentHealth = 100;

    public float maximumHealth = 100;

    [SerializeField]
    TextMeshProUGUI healthTMP;
    public bool isAlive;
    [SerializeField] CameraShake cameraShake;

    GameManager gameManager;

    [SerializeField] CanvasGroup TakeDamageCG;
    [SerializeField] float blinkTime = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maximumHealth;
        healthTMP.text = $"HP: {currentHealth.ToString()}";
        isAlive = true;
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (TakeDamageCG.alpha > 0)
        {
            TakeDamageCG.alpha -= Time.deltaTime / blinkTime;
        }
    }
    void UpdateHealthText()
    {
        healthTMP.text = $"HP: {currentHealth.ToString()}";
    }
    public void TakeDamage(float damage)
    {

        cameraShake.StartCoroutine(cameraShake.Shake(0.3f, 0.4f));
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maximumHealth);
        TakeDamageCG.alpha = 1;
        UpdateHealthText();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        if (isAlive)
        {

            Debug.Log("Muelto");
            isAlive = false;
            gameManager.GameOver();
        }
    }


}
