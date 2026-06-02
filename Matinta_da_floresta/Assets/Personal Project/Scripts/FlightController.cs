using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

/// <summary>
/// Mecânica de voo do Matinta.
/// Trigger esquerdo ativa o voo: o XR Origin sobe até flightHeight e então
/// voa continuamente na direção que o headset está apontando.
/// Timer regressivo encerra o voo e pousa suavemente.
/// </summary>
public class FlightController : MonoBehaviour
{
    public enum FlightState { Grounded, Ascending, Flying, Landing }

    [Header("Referências")]
    [Tooltip("XROrigin da cena. Se vazio, busca automaticamente.")]
    public XROrigin xrOrigin;

    [Tooltip("Input Action do trigger esquerdo.")]
    public InputActionReference leftTriggerAction;

    [Header("Parâmetros de Voo")]
    public float flightHeight   = 6f;
    public float ascentSpeed    = 3f;
    public float flightSpeed    = 4f;
    public float flightDuration = 10f;
    public float descentSpeed   = 2f;
    public float groundY        = 0f;

    [Header("Estado (read-only)")]
    [SerializeField] private FlightState currentState = FlightState.Grounded;
    [SerializeField] private float flightTimer = 0f;

    private Transform _originTransform;
    private Transform _cameraTransform;
    private float _targetFlightY;

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
        if (leftTriggerAction != null)
        {
            leftTriggerAction.action.Enable();
            leftTriggerAction.action.performed += OnTriggerPressed;
        }
    }

    void OnDisable()
    {
        if (leftTriggerAction != null)
            leftTriggerAction.action.performed -= OnTriggerPressed;
    }

    private void OnTriggerPressed(InputAction.CallbackContext ctx) => ActivateFlight();

    void Update()
    {
        if (_originTransform == null || _cameraTransform == null) return;

        switch (currentState)
        {
            case FlightState.Ascending: HandleAscent();  break;
            case FlightState.Flying:    HandleFlight();  break;
            case FlightState.Landing:   HandleLanding(); break;
        }
    }

    public void ActivateFlight()
    {
        if (currentState != FlightState.Grounded) return;
        _targetFlightY = groundY + flightHeight;
        flightTimer    = flightDuration;
        currentState   = FlightState.Ascending;
    }

    private void HandleAscent()
    {
        float currentY = _originTransform.position.y;
        float newY     = Mathf.MoveTowards(currentY, _targetFlightY, ascentSpeed * Time.deltaTime);

        _originTransform.position = new Vector3(
            _originTransform.position.x,
            newY,
            _originTransform.position.z
        );

        if (Mathf.Abs(newY - _targetFlightY) < 0.01f)
            currentState = FlightState.Flying;
    }

    private void HandleFlight()
    {
        _originTransform.position += _cameraTransform.forward * flightSpeed * Time.deltaTime;

        flightTimer -= Time.deltaTime;
        if (flightTimer <= 0f)
            currentState = FlightState.Landing;
    }

    private void HandleLanding()
    {
        float currentY = _originTransform.position.y;
        float newY     = Mathf.MoveTowards(currentY, groundY, descentSpeed * Time.deltaTime);

        _originTransform.position = new Vector3(
            _originTransform.position.x,
            newY,
            _originTransform.position.z
        );

        if (Mathf.Abs(newY - groundY) < 0.01f)
            currentState = FlightState.Grounded;
    }

    public float GetRemainingFlightTime() => flightTimer;
    public FlightState GetFlightState()   => currentState;
}
