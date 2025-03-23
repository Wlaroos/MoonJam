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
    [SerializeField] private Image _mask;
    [SerializeField] private Image _mePage;
    private Vector2[] _mePagePos = 
    {
        new Vector2(-1.6f, -5.1f),
        new Vector2(-23.1f, 2.3f),
        new Vector2(-34.8f, 5.5f),
        new Vector2(-41.8f, 16.3f)
    };    
    private Vector2[] _maskSizes = 
    {
        new Vector2(5.5f, 10f),
        new Vector2(49f, 22f),
        new Vector2(78.96f, 24.3f),
        new Vector2(98f, 55f)
    };
    private Vector2 _holdBarSize;
    private float _holdDuration = 1f;
    private CanvasGroup _canvasGroup; // Assign the CanvasGroup in the Inspector.
    private Sprite[] _phoneSprites;
    private GameStateManager _gsm;
    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;

        _phoneSprites = Resources.LoadAll<Sprite>("Cutscene");
        _phone.sprite = _phoneSprites[2];

        _holdBarSize = _holdBar.rectTransform.sizeDelta;
        _holdBar.rectTransform.sizeDelta = Vector2.zero;
        _holdBar2.rectTransform.sizeDelta = Vector2.zero;

        _gsm = FindObjectOfType<GameStateManager>();

        Time.timeScale = 0;
    }

    void Update()
    {
        if(Time.timeScale == 1) return;

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
                StartCoroutine(YSlideIn(_phone));
                break;
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                _phone.sprite = _phoneSprites[_state];
                break;
            case 8:
                StartCoroutine(YSlideIn(_map));
                break;
            case 9:
                _gaz1.transform.parent.GetComponent<CanvasGroup>().alpha = 0;
                StartCoroutine(FadeOutCanvas());
                StartCoroutine(YSlideOut(_map));
                break;
            case 10:
                Time.timeScale = 1;
                FindObjectOfType<TutorialMessages>().ActivateGunMessage();
                break;
            case 11:
                StartCoroutine(MapAnimation());
                break;
            case 12:
                _gaz1.transform.parent.GetComponent<CanvasGroup>().alpha = 0;
                StartCoroutine(FadeOutCanvas());
                StartCoroutine(YSlideOut(_map));
                _gsm.NextLevel();
                break;
            default:
                break;
        }

        _state++;
        //Debug.Log("State: " + _state);
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
                elapsedTime += Time.unscaledDeltaTime;;

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

    private IEnumerator YSlideIn(Image image)
    {
        if (image != null)
        {
            float duration = 1f; // Duration of the lerp in seconds
            float elapsedTime = 0f;

            // Store the initial position and rotation
            Vector3 initialPosition = image.rectTransform.anchoredPosition;
            Quaternion initialRotation = image.rectTransform.rotation;

            // Calculate the target position (half the height of the image)
            float halfHeight = image.rectTransform.rect.height / 2f;

            // Apply additional offset only for the phone
            float additionalOffset = (image == _phone) ? 20f : 0f; // Adjust 50f as needed
            Vector3 targetPosition = new Vector3(initialPosition.x, halfHeight + additionalOffset, initialPosition.z);

            Quaternion targetRotation = Quaternion.Euler(
                initialRotation.eulerAngles.x,
                initialRotation.eulerAngles.y,
                0
            );

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;

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

    private IEnumerator YSlideOut(Image image)
    {
        if (image != null)
        {
            float duration = 1f; // Duration of the lerp in seconds
            float elapsedTime = 0f;

            // Store the initial position and rotation
            Vector3 initialPosition = image.rectTransform.anchoredPosition;
            Quaternion initialRotation = image.rectTransform.rotation;

            // Calculate the target position (negative half the height of the image)
            float halfHeight = image.rectTransform.rect.height / 2f;
            Vector3 targetPosition = new Vector3(initialPosition.x, -halfHeight, initialPosition.z);
            Quaternion targetRotation = Quaternion.Euler(
                initialRotation.eulerAngles.x,
                initialRotation.eulerAngles.y,
                -initialRotation.eulerAngles.z
            );

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;

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

            // Gradually reduce the alpha of the CanvasGroup and fade in the black overlay
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;;
                float t = elapsedTime / duration;

                yield return null;
            }

            _canvasGroup.alpha = 1f;

            // Advance to the next state
            Time.timeScale = 1;
            AdvanceState();
        }
    }

    public void FadeIn()
    {
        Time.timeScale = 0;
        _state = 11;
        _gaz1.transform.parent.GetComponent<CanvasGroup>().alpha = 0;
        StartCoroutine(YSlideIn(_map));
        StartCoroutine(FadeInCanvas());
    }

    private IEnumerator FadeInCanvas()
    {
        if (_canvasGroup != null)
        {
            float duration = 1f; // Duration of the fade in seconds
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;;
                float t = elapsedTime / duration;

                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }
    }

    private IEnumerator MapAnimation()
    {
        // Get the current level from the GameStateManager
        int level = _gsm.CurrentLevel;

        // Ensure the level index is within bounds
        if (level < 0 || level >= _maskSizes.Length || level >= _mePagePos.Length)
        {
            Debug.LogError("Level index out of bounds for _maskSizes or _mePagePos.");
            yield break;
        }

        // Target size for the mask and position for the mePage
        Vector2 targetMaskSize = _maskSizes[level];
        Vector2 targetMePagePos = _mePagePos[level];

        // Initial size of the mask and position of the mePage
        Vector2 initialMaskSize = _mask.rectTransform.sizeDelta;
        Vector2 initialMePagePos = _mePage.rectTransform.anchoredPosition;

        float duration = 2f; // Duration of the animation in seconds
        float elapsedTime = 0f;

        // Animate the size delta of the mask and the position of the mePage
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / duration;

            // Lerp the size delta of the mask
            _mask.rectTransform.sizeDelta = Vector2.Lerp(initialMaskSize, targetMaskSize, t);

            // Lerp the position of the mePage
            _mePage.rectTransform.anchoredPosition = Vector2.Lerp(initialMePagePos, targetMePagePos, t);

            yield return null;
        }

        // Ensure the final size and position are set
        _mask.rectTransform.sizeDelta = targetMaskSize;
        _mePage.rectTransform.anchoredPosition = targetMePagePos;
    }
}