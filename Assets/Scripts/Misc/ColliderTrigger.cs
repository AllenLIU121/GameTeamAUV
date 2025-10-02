using UnityEngine;

public class StoreTrigger : MonoBehaviour
{
    private bool isTriggered = false;
    private void OnTriggerEnter2D()
    {
        if (!isTriggered)
        {
            StoreManager.Instance.OpenStore();
            AudioManager.Instance.PlaySFX("SFX_Shop");
            isTriggered = true;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.GetComponent<CharacterMove>().canMove = false;
                player.GetComponent<Rigidbody2D>().simulated = false;
            }
        }
    }
}
