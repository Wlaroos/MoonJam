using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneCanvas : MonoBehaviour
{
    private int _state = 0;
    private float _holdTime = 0f;
    private bool _isHolding = false;

    [SerializeField] private Image _holdBar; // Assign a UI Image in the Inspector to represent the hold bar.
    [SerializeField] private Image _holdBar2; // Assign a UI Image in the Inspector to represent the hold bar.
    [SerializeField] private Image _gaz1;
    [SerializeField] private Image _gaz2;
    [SerializeField] private Image _phone;
    [SerializeField] private Image _map;
    private Vector2 _holdBarSize;
    private float _holdDuration = 1f;
    private CanvasGroup _canvasGroup; // Assign the CanvasGroup in the Inspector.
    private Sprite[] _phoneSprites;
    private float[] _maskX = {5.5f,49f,78.96f,98f};
    private float[] _maskY = {10f,22f,24.3f,55f};

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;

        _phoneSprites = Resources.LoadAll<Sprite>("Cutscene");
        _phone.sprite = _phoneSprites[2];

        _holdBarSize = _holdBar.rectTransform.sizeDelta;
        _holdBar.rectTransform.sizeDelta = Vector2.zero;
        _holdBar2.rectTransform.sizeDelta = Vector2.zero;
    }

    void Update()
    {
        if(_state >= 10) return;

        if (Input.GetMouseButtonDown(0))
        {
            _isHolding = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_holdTime < _holdDuration)
            {
                AdvanceState();
            }
            else
            {
                _state = 9;
                AdvanceState();
            }

            _holdTime = 0f;
            _isHolding = false;
            UpdateHoldBar(0f);
        }

        if (_isHolding)
        {
            _holdTime += Time.unscaledDeltaTime;
            UpdateHoldBar(_holdTime / _holdDuration);

            if (_holdTime >= _holdDuration)
            {
                _state = 9;
                AdvanceState();
                _isHolding = false;
                _holdTime = 0f;
                UpdateHoldBar(0f);
            }
        }
    }

    private void UpdateHoldBar(float fillAmount)
    {
        if (_holdBar != null)
        {
            _holdBar.rectTransform.sizeDelta = new Vector2(fillAmount * _holdBarSize.x, _holdBarSize.y);
        }

               if (_holdBar2 != null)
        {
            _holdBar2.rectTransform.sizeDelta = new Vector2(fillAmount * _holdBarSize.x, _holdBarSize.y);
        }
    }

    private void AdvanceState()
    {
        switch (_state)
        {
            case 0:
                StartCoroutine(XSlide(_gaz1));
                break;
            case 1:
                StartCoroutine(XSlide(_gaz2));
                break;
            case 2:
                StartCoroutine(YSlide(_phone));
                break;
            case 3:
                _phone.sprite = _phoneSprites[_state];
                break;
            case 4:
                _phone.sprite = _phoneSprites[_state];
                break;
            case 5:
                _phone.sprite = _phoneSprites[_state];
                break;
            case 6:
                _phone.sprite = _phoneSprites[_state];
                break;
            case 7:
                _phone.sprite = _phoneSprites[_state];
                break;
            case 8:
                StartCoroutine(YSlide(_map));
                break;
            case 9:
                StartCoroutine(FadeOutCanvas());
                break;
            default:
                Time.timeScale = 1;
                break;
        }

        _state++;
        Debug.Log("State: " + _state);
    }

    private IEnumerator XSlide(Image image)
    {
        if (image != null)
        {
            float duration = 1f; // Duration of the lerp in seconds
            float elapsedTime = 0f;

            // Store the initial position and rotation
            Vector3 initialPosition = image.rectTransform.anchoredPosition;
            Quaternion initialRotation = image.rectTransform.rotation;

            // Target position and rotation
            Vector3 targetPosition = new Vector3(0, initialPosition.y, initialPosition.z);
            Quaternion targetRotation = Quaternion.Euler(
                initialRotation.eulerAngles.x,
                initialRotation.eulerAngles.y,
                0
            );

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                // Calculate the easing factor using Mathf.SmoothStep
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                // Lerp position with easing
                image.rectTransform.anchoredPosition = Vector3.Lerp(
                    initialPosition,
                    targetPosition,
                    t
                );

                // Lerp rotation with easing
                image.rectTransform.rotation = Quaternion.Lerp(
                    initialRotation,
                    targetRotation,
                    t
                );

                yield return null;
            }

            // Ensure final position and rotation are set
            image.rectTransform.anchoredPosition = targetPosition;
            image.rectTransform.rotation = targetRotation;
        }
    }

    private IEnumerator YSlide(Image image)
    {
        if (image != null)
        {
            float duration = 1f; // Duration of the lerp in seconds
            float elapsedTime = 0f;

            // Store the initial position and rotation
            Vector3 initialPosition = image.rectTransform.anchoredPosition;
            Quaternion initialRotation = image.rectTransform.rotation;

            // Target position and rotation
            Vector3 targetPosition = new Vector3(initialPosition.x, -initialPosition.y, initialPosition.z);
            Quaternion targetRotation = Quaternion.Euler(
                initialRotation.eulerAngles.x,
                initialRotation.eulerAngles.y,
                -initialRotation.eulerAngles.z
            );

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                // Calculate the easing factor using Mathf.SmoothStep
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                // Lerp position with easing
                image.rectTransform.anchoredPosition = Vector3.Lerp(
                    initialPosition,
                    targetPosition,
                    t
                );

                // Lerp rotation with easing
                image.rectTransform.rotation = Quaternion.Lerp(
                    initialRotation,
                    targetRotation,
                    t
                );

                yield return null;
            }

            // Ensure final position and rotation are set
            image.rectTransform.anchoredPosition = targetPosition;
            image.rectTransform.rotation = targetRotation;
        }
    }

    private IEnumerator FadeOutCanvas()
    {
        if (_canvasGroup != null)
        {
            float duration = 1f; // Duration of the fade in seconds
            float elapsedTime = 0f;

            // Create a black overlay image
            GameObject blackOverlay = new GameObject("BlackOverlay");
            blackOverlay.transform.SetParent(transform, false);
            RectTransform rectTransform = blackOverlay.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image overlayImage = blackOverlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0f); // Start with transparent black

            // Gradually reduce the alpha of the CanvasGroup and fade in the black overlay
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                overlayImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 1f, t));

                yield return null;
            }

            // Ensure the alpha is set to 0 and the overlay is fully black at the end
            _canvasGroup.alpha = 0f;
            overlayImage.color = new Color(0f, 0f, 0f, 1f);

            // Advance to the next state
            AdvanceState();
        }
    }
}