using UnityEngine;

public class DonutCollisionReporter : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Donut"))
        {
            SoundManager.Instance.HandleDonutCollision(gameObject);
        }
    }
}
