using UnityEngine;
using Siccity.GLTFUtility;
public class BackRoomCell : MonoBehaviour
{
    [SerializeField] private GameObject lighting;
    GameObject monsterImg;
    bool hasMonster = false;
    public void Initial(bool isBright, bool toSpawn)
    {
        if (isBright) lighting.SetActive(true);
        else lighting.SetActive(false);
        if (toSpawn) SpawnMonster();
    }
    void SpawnMonster()
    {
        int childCount = transform.childCount;

        // 確保至少有4個子物件
        if (childCount < 4) return;

        // 從前4個子物件中隨機選擇一個
        int randomIndex = Random.Range(0, 4);
        Transform selectedChild = transform.GetChild(randomIndex);

        // 確保選中的子物件有至少2個子物件
        if (selectedChild.childCount < 3) return;
        hasMonster = true;
        // 啟用前兩個子物件
        if (Random.value < 0.5f)
        {
            monsterImg = selectedChild.GetChild(1).gameObject;
            monsterImg.SetActive(true);
        }
        else 
        {
            monsterImg = selectedChild.GetChild(2).gameObject;
            monsterImg.SetActive(true);
        }
        print("覽老at: " + gameObject.name);

    }


    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject frontWall;
    [SerializeField] private GameObject backWall;
    [SerializeField] private GameObject unvisitedWall;
    public bool isVisited { get; private set; }
    public void Visit()
    {
        isVisited = true;
        unvisitedWall.SetActive(false);
    }
    public void ClearLeftWall()
    {
        leftWall.SetActive(false);
    }
    public void ClearRightWall()
    {
        rightWall.SetActive(false);
    }
    public void ClearFrontWall()
    {
        frontWall.SetActive(false);
    }
    public void ClearBackWall()
    {
        backWall.SetActive(false);
    }

    [SerializeField] AudioSource[] audioSources;
    bool isPlayed = false;
    void OnTriggerEnter(Collider other)
    {
        if (!hasMonster || !monsterImg.activeSelf || isPlayed) return;
        if (other.transform.root.name == "XR Origin (XR Rig)")
        {
            int randomIndex = Random.Range(0, audioSources.Length);
            foreach (var audio in audioSources)
            {
                audio.Stop();
            }
            audioSources[randomIndex].Play();
            isPlayed = true;
        }
    }
}
