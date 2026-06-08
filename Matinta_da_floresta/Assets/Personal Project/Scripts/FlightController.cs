using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

/// <summary>
/// Mecânica de voo.
/// </summary>
public class FlightController : MonoBehaviour
{
    public enum FlightState { Grounded, Ascending, Flying, Landing }

    [Header("Referências")]
    [Tooltip("XROrigin da cena. Se vazio, busca automaticamente.")]
    public XROrigin xrOrigin;

    [Tooltip("Input Action para iniciar o voo.")]
    public InputActionReference flightAction; // Mudei o nome para ser mais genérico

    [Header("Parâmetros de Voo")]
    public float flightHeight = 6f;
    public float ascentSpeed = 3f;
    public float flightSpeed = 4f;
    public float flightDuration = 10f;
    public float descentSpeed = 2f;
    public float groundY = 0f;

    [Header("Estado (read-only)")]
    [SerializeField] private FlightState currentState = FlightState.Grounded;
    [SerializeField] private float flightTimer = 0f;

    [Header("Controle de Permissão")]
    [Tooltip("Se verdadeiro, o jogador tem permissão para voar.")]
    public bool canFly = false;

    private Transform _originTransform;
    private Transform _cameraTransform;
    private float _targetFlightY;
    private bool _flightActionPressed = false;

    void Awake()
    {
        if (xrOrigin == null)
            xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin != null)
        {
            _originTransform = xrOrigin.transform;
            _cameraTransform = xrOrigin.Camera.transform;
        }
        else
        {
            Debug.LogError("[FlightController] XROrigin não encontrado!");
        }
    }

    void OnEnable()
    {
        if (flightAction != null)
        {
            flightAction.action.Enable();
            // Lemos quando o botão é pressionado (started) ou solto (canceled)
            flightAction.action.started += OnFlightActionStarted;
            flightAction.action.canceled += OnFlightActionCanceled;
        }
    }

    void OnDisable()
    {
        if (flightAction != null)
        {
            flightAction.action.started -= OnFlightActionStarted;
            flightAction.action.canceled -= OnFlightActionCanceled;
        }
    }

    private void OnFlightActionStarted(InputAction.CallbackContext ctx)
    {
        _flightActionPressed = true;
        // Tenta ativar o voo apenas quando o botão for pressionado
        ActivateFlight();
    }

    private void OnFlightActionCanceled(InputAction.CallbackContext ctx)
    {
        _flightActionPressed = false;
    }


    void Update()
    {
        if (_originTransform == null || _cameraTransform == null) return;

        switch (currentState)
        {
            case FlightState.Ascending: HandleAscent(); break;
            case FlightState.Flying: HandleFlight(); break;
            case FlightState.Landing: HandleLanding(); break;
            case FlightState.Grounded:
                // Se estiver no chão, com permissão e segurando o botão, tenta subir
                if (canFly && _flightActionPressed)
                {
                    ActivateFlight();
                }
                break;
        }
    }

    public void EnableFlight()
    {
        canFly = true;
        Debug.Log("[FlightController] Voo liberado pela moeda!");
    }

    public void ActivateFlight()
    {
        if (!canFly || currentState != FlightState.Grounded) return;

        _targetFlightY = _originTransform.position.y + flightHeight; // Sobe em relação à altura atual, não groundY absoluto
        flightTimer = flightDuration;
        currentState = FlightState.Ascending;
    }

    private void HandleAscent()
    {
        float currentY = _originTransform.position.y;
        float newY = Mathf.MoveTowards(currentY, _targetFlightY, ascentSpeed * Time.deltaTime);

        _originTransform.position = new Vector3(_originTransform.position.x, newY, _originTransform.position.z);

        if (Mathf.Abs(newY - _targetFlightY) < 0.01f)
            currentState = FlightState.Flying;
    }

    private void HandleFlight()
    {
        // Se o jogador soltar o botão, começamos a descer
        if (!_flightActionPressed)
        {
            currentState = FlightState.Landing;
            return;
        }

        // Move na direção da câmera
        Vector3 flightDirection = _cameraTransform.forward;
        // Ignora a rotação em Y para não voar para cima/baixo com a câmera, mantém a altura constante
        flightDirection.y = 0;

        _originTransform.position += flightDirection.normalized * flightSpeed * Time.deltaTime;

        flightTimer -= Time.deltaTime;

        if (flightTimer <= 0f)
            currentState = FlightState.Landing;
    }

    private void HandleLanding()
    {
        float currentY = _originTransform.position.y;
        float newY = Mathf.MoveTowards(currentY, groundY, descentSpeed * Time.deltaTime);

        _originTransform.position = new Vector3(_originTransform.position.x, newY, _originTransform.position.z);

        if (Mathf.Abs(newY - groundY) < 0.01f)
        {
            currentState = FlightState.Grounded;
            canFly = false; // Gasta a "permissão" ao aterrissar
            _flightActionPressed = false;
        }
    }

    public float GetRemainingFlightTime() => flightTimer;
    public FlightState GetFlightState() => currentState;
}