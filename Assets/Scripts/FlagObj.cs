using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FlagObj : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private bool isBeingGrabbed = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isBeingGrabbed = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isBeingGrabbed = false;
        StartMoveToYZeroAndResetRotation();
    }

    public bool IsBeingGrabbed()
    {
        return isBeingGrabbed;
    }

    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool shouldMove = false;

    void Update()
    {
        if (!shouldMove) return;
        if (isBeingGrabbed) return;

        // 平滑移動
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 平滑旋轉
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

        // 當到達目標位置與旋轉時，停止移動
        if (transform.position == targetPosition && transform.rotation == targetRotation)
        {
            shouldMove = false;
        }
    }

    public void StartMoveToYZeroAndResetRotation()
    {
        Vector3 currentPos = transform.position;
        targetPosition = new Vector3(currentPos.x, 0f, currentPos.z);
        targetRotation = Quaternion.Euler(0f, 0f, 0f);
        shouldMove = true;
    }
}
