using System.Collections;
using TMPro;
using UnityEngine;

public sealed class DamageNumberPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text popupTemplate;
    [SerializeField] private Color damageColor = new Color(0.96f, 0.16f, 0.13f, 1f);
    [SerializeField, Min(0.1f)] private float lifetime = 0.8f;
    [SerializeField, Min(0f)] private float riseDistance = 72f;

    private static DamageNumberPopup instance;
    private RectTransform canvasRect;

    private void Awake()
    {
        instance = this;
        canvasRect = transform as RectTransform;
        if (popupTemplate != null)
            popupTemplate.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void Show(float damage, Vector3 worldPosition)
    {
        if (instance == null || instance.popupTemplate == null || damage <= 0f)
            return;

        instance.CreatePopup(damage, worldPosition);
    }

    private void CreatePopup(float damage, Vector3 worldPosition)
    {
        Camera worldCamera = Camera.main;
        if (worldCamera == null || canvasRect == null)
            return;

        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, null, out Vector2 localPosition))
            return;

        TMP_Text popup = Instantiate(popupTemplate, canvasRect, false);
        popup.gameObject.SetActive(true);
        popup.text = Mathf.RoundToInt(damage).ToString();
        popup.color = damageColor;
        popup.alignment = TextAlignmentOptions.Center;
        popup.enableAutoSizing = false;
        popup.fontSize = 42f;
        popup.raycastTarget = false;

        RectTransform popupRect = popup.rectTransform;
        popupRect.anchorMin = popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.sizeDelta = new Vector2(260f, 76f);
        popupRect.anchoredPosition = localPosition + new Vector2(Random.Range(-18f, 19f), 0f);

        CanvasGroup group = popup.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;
        StartCoroutine(AnimatePopup(popup, group, popupRect.anchoredPosition));
    }

    private IEnumerator AnimatePopup(TMP_Text popup, CanvasGroup group, Vector2 startPosition)
    {
        float elapsed = 0f;
        while (elapsed < lifetime && popup != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / lifetime);
            popup.rectTransform.anchoredPosition = startPosition + Vector2.up * (riseDistance * progress);
            group.alpha = 1f - progress;
            yield return null;
        }

        if (popup != null)
            Destroy(popup.gameObject);
    }
}