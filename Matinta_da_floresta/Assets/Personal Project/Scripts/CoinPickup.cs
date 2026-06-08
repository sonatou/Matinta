using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Usamos GetComponentInParent porque o colisor vai estar na Câmera (filha)
            // e o script de voo está no XR Origin (pai)
            FlightController flightController = other.GetComponentInParent<FlightController>();

            if (flightController != null)
            {
                flightController.EnableFlight();
            }

            Destroy(gameObject);
        }
    }
}