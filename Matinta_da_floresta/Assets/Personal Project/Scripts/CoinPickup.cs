using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // FlightController está no pai (XROrigin), não no PlayerCollider
        FlightController flightController = other.GetComponentInParent<FlightController>();

        if (flightController != null)
            flightController.ActivateFlight();

        Destroy(gameObject);
    }
}
