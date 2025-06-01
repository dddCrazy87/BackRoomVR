using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public FadeCanvas fadeCanvas;
    public BackRoomGenerator backRoomGenerator;
    public GameObject sciFiRoom;
    public Transform player;
    public GameObject entry, entryTrigger, exit;

    [Header("Player Initial UI")]
    [SerializeField] private GameObject initialUI;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Slider roomSizeSlider;
    public GameObject passCanvas, failCanvas;
    void Start()
    {
        fadeCanvas.StartFadeOut();
        Renderer targetRenderer;
        targetRenderer = entry.GetComponent<Renderer>();
        entryGlassMaterial = targetRenderer.materials[1];
        entryMaterial = targetRenderer.materials[0];
        entryTrigger.SetActive(false);
        brightnessSlider.minValue = 0;
        brightnessSlider.maxValue = 1;
        roomSizeSlider.minValue = 20;
        roomSizeSlider.maxValue = 150;

        InvokeRepeating(nameof(UpdateFlashBattery), 0f, 2f);
        flashLight.SetActive(false);
        menuCanvas.SetActive(false);
    }
    private float brightness, roomSize;
    public void SetBackRoomValue()
    {
        brightness = brightnessSlider.value;
        roomSize = roomSizeSlider.value;
        entryTrigger.SetActive(true);
        if (isFading) return;
        StartCoroutine(FadeToAlpha(targetAlpha));
        StartCoroutine(IncreaseEmission());
    }
    [SerializeField] private AudioSource backRoomBgm;
    public void StartGame()
    {
        fadeCanvas.StartFadeIn(() =>
        {
            fadeCanvas.StartFadeOut();
            backRoomBgm.Play();
            player.position = new Vector3(0, player.position.y, 0);
            player.rotation = Quaternion.identity;
            sciFiRoom.SetActive(false);
            initialUI.SetActive(false);
            backRoomGenerator.StartGenerate(brightness, roomSize);
        });

    }
    public void PassBackRoom()
    {
        fadeCanvas.StartFadeIn(() =>
        {
            fadeCanvas.StartFadeOut();
            passCanvas.SetActive(true);
            CloseBackRoom();
            ResetData();
        });
    }
    public void FailBackRoom()
    {
        fadeCanvas.StartFadeIn(() =>
        {
            fadeCanvas.StartFadeOut();
            failCanvas.SetActive(true);
            CloseBackRoom();
            ResetData();
        });
    }

    private void CloseBackRoom()
    {
        backRoomBgm.Pause();
        backRoomGenerator.InitializeRoomData();
        sciFiRoom.SetActive(true);
        player.position = new Vector3(0, player.position.y, 0);
        player.rotation = Quaternion.identity;
    }
    void ResetData()
    {
        flagCnt = 2;
        flashBattery = 100;
        isUsingFlashLight = false;
        isFading = false;
        fadeSpeed = 1f;
        targetAlpha = 45;
        targetEmissionIntensity = 5f;
        duration = 0.5f;
        entryGlassMaterial.SetColor("_BaseColor", new Color(1f, 1f, 1f, 228 / 255f));
        Color color = new Color32(77, 191, 92, 255);
        entryMaterial.SetColor("_EmissionColor", color * targetEmissionIntensity);
    }

    [Header("Menu Setting")]
    [SerializeField] GameObject menuCanvas;
    [SerializeField] TextMeshProUGUI flashStatusTxt;
    [SerializeField] TextMeshProUGUI flagStatusTxt;
    [SerializeField] GameObject flagPrefab;
    [SerializeField] GameObject flashLight;
    int flashBattery = 100, flagCnt = 2;
    public bool isUsingFlashLight = false;
    public void ToggleMenuCanvas()
    {
        if (menuCanvas.activeSelf) menuCanvas.SetActive(false);
        else menuCanvas.SetActive(true);
    }
    public void UpdateFlagAmount(int n)
    {
        flagCnt += n;
        flagStatusTxt.text = flagCnt.ToString();
        if (flagCnt <= 0) flagStatusTxt.color = Color.red;
        else flagStatusTxt.color = new Color32(176, 176, 176, 255);
    }

    [SerializeField] GrabDetector leftDetector, rightDetector;
    public void PlaceFlag()
    {
        if (leftDetector.isHoldingFlag)
        {
            Destroy(leftDetector.flagHolding);
            leftDetector.flagHolding = null;
            leftDetector.isHoldingFlag = false;
            rightDetector.flagHolding = null;
            rightDetector.isHoldingFlag = false;
            UpdateFlagAmount(1);
        }
        else if (rightDetector.isHoldingFlag)
        {
            Destroy(rightDetector.flagHolding);
            rightDetector.flagHolding = null;
            rightDetector.isHoldingFlag = false;
            leftDetector.flagHolding = null;
            leftDetector.isHoldingFlag = false;
            UpdateFlagAmount(1);
        }
        else
        {
            if (flagCnt <= 0) return;
            Vector3 pos = player.position + player.forward * 1.5f;
            pos.y = 0;
            Instantiate(flagPrefab, pos, Quaternion.identity);
            UpdateFlagAmount(-1);
        }
    }
    public void UpdateFlashBattery()
    {
        if (!isUsingFlashLight) return;
        if (flashBattery <= 0) return;
        flashBattery -= 1;
        flashStatusTxt.text = flashBattery.ToString();
        if (flashBattery <= 0) flashStatusTxt.color = Color.red;
        else flashStatusTxt.color = new Color32(176, 176, 176, 255); ;
    }
    public void RefreshFlashBattery()
    {
        flashBattery = 100;
        flashStatusTxt.text = flashBattery.ToString();
        flashStatusTxt.color = new Color32(176, 176, 176, 255); ;
    }
    public void ToggleFlashLight()
    {
        if (flashBattery <= 0) return;
        isUsingFlashLight = !isUsingFlashLight;
        if (isUsingFlashLight) flashLight.SetActive(true);
        else flashLight.SetActive(false);
    }

    [Header("Entry Glass Setting")]
    public float fadeSpeed = 1f;
    public float targetAlpha = 45;
    private Material entryGlassMaterial;
    private bool isFading = false;
    private Color baseColor;
    IEnumerator FadeToAlpha(float targetAlpha)
    {
        isFading = true;
        float currentAlpha = 228f; // 使用 float，範圍是0~255

        float step = fadeSpeed * Time.deltaTime * 255f;
        // 把 fadeSpeed 改成「每秒透明度改變的百分比(0~1)」，乘以255讓步進合理

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, step);

            // 轉換成0~1範圍，Color參數也都用0~1
            Color newColor = new Color(1f, 1f, 1f, currentAlpha / 255f);

            entryGlassMaterial.SetColor("_BaseColor", newColor);

            yield return null;
        }

        // 最後確保正確設置
        baseColor = new Color(1f, 1f, 1f, targetAlpha / 255f);
        entryGlassMaterial.SetColor("_BaseColor", baseColor);
        isFading = false;
    }

    private Material entryMaterial;
    public float targetEmissionIntensity = 5f; // 最終亮度
    public float duration = 0.5f; // 漸變所需時間（秒）
    IEnumerator IncreaseEmission()
    {
        if (!entryMaterial.IsKeywordEnabled("_EMISSION"))
        {
            entryMaterial.EnableKeyword("_EMISSION"); // 確保開啟 Emission
        }

        Color baseColor = Color.white; // 最終想要的 Emission 顏色（白色）
        Color currentColor = entryMaterial.GetColor("_EmissionColor"); // 當前 emission 顏色
        float currentIntensity = currentColor.maxColorComponent; // 取出目前亮度

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float intensity = Mathf.Lerp(currentIntensity, targetEmissionIntensity, t);
            entryMaterial.SetColor("_EmissionColor", baseColor * intensity);
            yield return null;
        }

        entryMaterial.SetColor("_EmissionColor", baseColor * targetEmissionIntensity);
    }

}
