using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    // OnTriggerEnter é chamado automaticamente quando algo atravessa o colisor da moeda
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem encostou na moeda foi o Player
        if (other.CompareTag("Player"))
        {
            // Tenta achar o seu script FlightController no Player que colidiu
            FlightController flightController = other.GetComponent<FlightController>();

            // Se achou o script de voo...
            if (flightController != null)
            {
                flightController.EnableFlight(); // Chama a função que vamos criar no próximo passo
            }

            // Destrói a moeda da cena
            Destroy(gameObject);
        }
    }
}