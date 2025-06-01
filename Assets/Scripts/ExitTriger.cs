using UnityEngine;

public class ExitTriger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name == "XR Origin (XR Rig)")
        {
            print("exit");
            FindFirstObjectByType<GameManager>().PassBackRoom();
        }
        else
        {
            print(other.name);
        }
    }
}
