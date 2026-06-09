using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// Mantém o PlayerCollider alinhado com a posição da câmera no eixo X/Z.
/// O Y é controlado pelo XROrigin (e pelo FlightController).
/// </summary>
public class ColliderFollowCamera : MonoBehaviour
{
    private Transform _cameraTransform;

    void Awake()
    {
        var xrOrigin = FindFirstObjectByType<XROrigin>();
        if (xrOrigin != null)
            _cameraTransform = xrOrigin.Camera.transform;
        else
            Debug.LogError("[ColliderFollowCamera] XROrigin não encontrado!");
    }

    void Update()
    {
        if (_cameraTransform == null) return;

        // Segue X/Z da câmera, Y fica em 0 local (relativo ao XROrigin)
        transform.localPosition = new Vector3(
            _cameraTransform.localPosition.x,
            0f,
            _cameraTransform.localPosition.z
        );
    }
}
