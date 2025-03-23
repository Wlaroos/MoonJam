using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FinalCutscene : MonoBehaviour
{
    private int _state = 0;
    private float _holdTime = 0f;
    private bool _isHolding = false;
    private bool _childAnimated = false; // Flag to track if the child animation has been triggered.

    [SerializeField] private Image[] _cutsceneImages; // Assign cutscene images in the Inspector.
    [SerializeField] private Image _holdBar; // Assign a UI Image in the Inspector to represent the hold bar.
    private Vector2 _holdBarSize;

    void Awake()
    {
        if (_holdBar != null)
        {
            _holdBarSize = _holdBar.rectTransform.sizeDelta;
            _holdBar.rectTransform.sizeDelta = Vector2.zero; // Start with an empty hold bar.
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isHolding = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_holdTime >= 1f)
            {
                QuitGame();
            }
            else
            {
                AdvanceState();
            }

            _holdTime = 0f;
            _isHolding = false;
            UpdateHoldBar(0f); // Reset the hold bar.
        }

        if (_isHolding)
        {
            _holdTime += Time.unscaledDeltaTime;

            if (_holdBar != null)
            {
                UpdateHoldBar(_holdTime / 1f); // Update the hold bar fill amount.
            }

            if (_holdTime >= 1f)
            {
                QuitGame();
            }
        }
    }

    private void AdvanceState()
    {
        if (_state < _cutsceneImages.Length)
        {
            if (!_childAnimated && _state == _cutsceneImages.Length - 1)
            {
                StartCoroutine(AnimateChild(_cutsceneImages[_state-1].transform.GetChild(0)));
                _childAnimated = true; // Mark the child animation as triggered.
            }
            else
            {
                StartCoroutine(XSlide(_cutsceneImages[_state]));
                _state++;
                _childAnimated = false; // Reset the flag for future states.
            }
        }
    }

    private IEnumerator XSlide(Image image)
    {
        if (image != null)
        {
            image.gameObject.SetActive(true);

            float duration = 1f;
            float elapsedTime = 0f;

            // Add random offset and rotation
            Vector3 initialPosition = image.rectTransform.anchoredPosition;
            Vector3 targetPosition = new Vector3(Random.Range(-2f,2f), Random.Range(-2f,2f),0);
            Quaternion initialRotation = Quaternion.identity;
            Quaternion targetRotation = Quaternion.Euler(0, 0, Random.Range(-5, 5f));

            image.rectTransform.anchoredPosition = initialPosition;
            image.rectTransform.rotation = initialRotation;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                image.rectTransform.anchoredPosition = Vector3.Lerp(initialPosition, targetPosition, t);
                image.rectTransform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t);

                yield return null;
            }

            image.rectTransform.anchoredPosition = targetPosition;
            image.rectTransform.rotation = targetRotation;
        }
    }

    private IEnumerator AnimateChild(Transform child)
    {
        if (child != null)
        {
            float duration = 5f;
            float elapsedTime = 0f;

            Vector3 initialPosition = child.localPosition;
            Vector3 targetPosition = initialPosition + new Vector3(120, 0f, 0f); // Move to the right
            Quaternion initialRotation = child.localRotation;
            Quaternion targetRotation = Quaternion.Euler(0, 0, 60f); // Slight rotation

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                child.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);
                child.localRotation = Quaternion.Lerp(initialRotation, targetRotation, t);

                yield return null;
            }

            child.localPosition = targetPosition;
            child.localRotation = targetRotation;
        }
    }

    private void UpdateHoldBar(float fillAmount)
    {
        if (_holdBar != null)
        {
            _holdBar.rectTransform.sizeDelta = new Vector2(fillAmount * _holdBarSize.x, _holdBarSize.y);
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}