using UnityEngine;

public class GrabDetector : MonoBehaviour
{
    public bool isHoldingFlag;
    public GameObject flagHolding;
    GameObject flashLight;
    [SerializeField] GameManager gameManager;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("flag"))
        {
            print(other.name);
            isHoldingFlag = true;
            flagHolding = other.gameObject;
        }
        if (other.gameObject.CompareTag("flashlight"))
        {
            gameManager.RefreshFlashBattery();
            flashLight = other.gameObject;
            Invoke(nameof(DeystroyFlashLight), 0.5f);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("flag"))
        {
            isHoldingFlag = false;
            flagHolding = null;
        }
    }

    void DeystroyFlashLight()
    {
        Destroy(flashLight);
        flashLight = null;
    }
}
