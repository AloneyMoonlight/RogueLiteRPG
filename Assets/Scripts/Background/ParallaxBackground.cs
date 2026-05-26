using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        [Range(0f, 1f)]
        [Tooltip("0 = fixed en pantalla (sigue la camara), 1 = fijo en el mundo (maximo parallax)")]
        public float parallaxFactor = 0.05f;
    }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private ParallaxLayer[] layers;

    private Vector3 lastCamPos;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        lastCamPos = mainCamera.transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = mainCamera.transform.position - lastCamPos;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform == null) continue;

            // Factor 0 = el layer sigue la camara (estatico en pantalla)
            // Factor 1 = el layer no se mueve (el mundo pasa por delante)
            // Para fondo "casi estatico" usa factor 0.05 - 0.1
            layer.layerTransform.position += new Vector3(
                delta.x * (1f - layer.parallaxFactor),
                delta.y * (1f - layer.parallaxFactor),
                0f
            );
        }

        lastCamPos = mainCamera.transform.position;
    }
}
