using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Camera seekFleeCamera;
    [SerializeField] private Camera flockingCamera;

    private void Start()
    {
        if (seekFleeCamera.gameObject.activeSelf && flockingCamera.gameObject.activeSelf) 
            flockingCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (!seekFleeCamera.gameObject.activeSelf)
                seekFleeCamera.gameObject.SetActive(true);

            if (flockingCamera.gameObject.activeSelf)
                flockingCamera.gameObject.SetActive(false);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (!flockingCamera.gameObject.activeSelf)
                flockingCamera.gameObject.SetActive(true);

            if (seekFleeCamera.gameObject.activeSelf)
                seekFleeCamera.gameObject.SetActive(false);
        }
    }
}
