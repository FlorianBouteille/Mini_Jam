using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class DeathZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float posX = 0f;
    public float posY = 0f;
    public float posZ = 0f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = new Vector3(posX, posY, posZ);
            other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
    }
    // Update is called once per frame
}
