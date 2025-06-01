using UnityEngine;

public class EntryTriger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name == "XR Origin (XR Rig)")
        {
            FindFirstObjectByType<GameManager>().StartGame();
        }
        else
        {
            print(other.name);
        }
    }
}
