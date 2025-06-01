using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Fades a canvas over time using a coroutine and a canvas group
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvas : MonoBehaviour
{
    [Tooltip("The speed at which the canvas fades")]
    public float defaultDuration = 1.0f;

    public Coroutine CurrentRoutine { private set; get; } = null;

    // 事件方式通知
    public event Action OnFadeInComplete;
    public event Action OnFadeOutComplete;

    private CanvasGroup canvasGroup = null;
    private float alpha = 0.0f;

    private float quickFadeDuration = 0.25f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // 初始化 alpha 值
        alpha = canvasGroup.alpha;
    }

    // 使用 Action 回調的方法
    public void StartFadeIn(Action onComplete = null)
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeIn(defaultDuration, onComplete));
    }

    public void StartFadeOut(Action onComplete = null)
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeOut(defaultDuration, onComplete));
    }

    public void QuickFadeIn(Action onComplete = null)
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeIn(quickFadeDuration, onComplete));
    }

    public void QuickFadeOut(Action onComplete = null)
    {
        StopAllCoroutines();
        CurrentRoutine = StartCoroutine(FadeOut(quickFadeDuration, onComplete));
    }

    private IEnumerator FadeIn(float duration, Action onComplete = null)
    {
        float startAlpha = alpha;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            SetAlpha(Mathf.Lerp(startAlpha, 1f, progress));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetAlpha(1f); // 確保最終值
        CurrentRoutine = null;

        // 通知完成 - 兩種方式
        onComplete?.Invoke();
        OnFadeInComplete?.Invoke();
    }

    private IEnumerator FadeOut(float duration, Action onComplete = null)
    {
        float startAlpha = alpha;
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            SetAlpha(Mathf.Lerp(startAlpha, 0f, progress));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f); // 確保最終值
        CurrentRoutine = null;

        // 通知完成 - 兩種方式
        onComplete?.Invoke();
        OnFadeOutComplete?.Invoke();
    }

    private void SetAlpha(float value)
    {
        alpha = Mathf.Clamp01(value);
        canvasGroup.alpha = alpha;
    }

    // 立即設定透明度的輔助方法
    public void SetImmediateAlpha(float targetAlpha)
    {
        StopAllCoroutines();
        SetAlpha(targetAlpha);
        CurrentRoutine = null;
    }
}