using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Heal");
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager playerManager = other.gameObject.GetComponent<PlayerManager>();
            if (playerManager.currentHealth < playerManager.maximumHealth)
            {
                playerManager.Heal(20);
                Destroy(gameObject);
            }
        }
    }
}
