using UnityEngine;
using UnityEngine.XR;

public class VRButtonTest : MonoBehaviour
{
    private bool aBtnPressed = false;
    private bool xBtnPressed = false;
    private bool yBtnPressed = false;
    private bool menuBtnPressed = false;
    [SerializeField] GameManager gameManager;
    void Update()
    {

        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool aBtnIsPressing))
        {
            if (aBtnIsPressing && !aBtnPressed)
            {
                gameManager.ToggleFlashLight();
            }

            aBtnPressed = aBtnIsPressing;
        }

        if (leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool xBtnIsPressing))
        {
            if (xBtnIsPressing && !xBtnPressed)
            {
                gameManager.PlaceFlag();
            }

            xBtnPressed = xBtnIsPressing;
        }

        if (leftHand.TryGetFeatureValue(CommonUsages.menuButton, out bool mennuBtnIsPressing))
        {
            if (mennuBtnIsPressing && !menuBtnPressed)
            {
                gameManager.ToggleMenuCanvas();
            }

            menuBtnPressed = mennuBtnIsPressing;
        }

        if (leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yBtnIsPressing))
        {
            if (yBtnIsPressing && !yBtnPressed)
            {
                gameManager.FailBackRoom();
            }

            yBtnPressed = yBtnIsPressing;
        }

        // // A 或 X 鍵（Primary Button）
        // if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonPressed) && primaryButtonPressed)
        // {
        //     Debug.Log("Primary button (A/X) pressed");
        // }

        // // Trigger Button
        // if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        // {
        //     Debug.Log("Trigger is pressed");
        // }

        // // Grip Button
        // if (rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed) && gripPressed)
        // {
        //     Debug.Log("Grip is pressed");
        // }

    }
}
