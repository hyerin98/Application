namespace IMFINE.UI.SettingUI
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class ToggleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public delegate void ToggleButtonEvent(bool isOn);
        public event ToggleButtonEvent ValueChanged;

        [SerializeField]
        private Image _bg;
        [SerializeField]
        private Image _button;
        [SerializeField]
        private Color _onColor;
        [SerializeField]
        private Color _offColor;
        [HideInInspector]
        public bool isOn = false;


        public void OnPointerDown(PointerEventData eventData)
        {
            if (isOn)
            {
                _button.rectTransform.localPosition = new Vector3(9f, 0f, 0f);
            }
            else
            {
                _button.rectTransform.localPosition = new Vector3(-9f, 0f, 0f);
            }

            _button.rectTransform.sizeDelta = new Vector2(50f, 40f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isOn)
            {
                isOn = false;
                _button.rectTransform.DOLocalMove(new Vector3(-14f, 0f, 0f), 0.3f).SetEase(Ease.OutCubic);
                _bg.DOColor(_offColor, 0.3f).SetEase(Ease.OutCubic);
            }
            else
            {
                isOn = true;
                _button.rectTransform.DOLocalMove(new Vector3(14f, 0f, 0f), 0.3f).SetEase(Ease.OutCubic);
                _bg.DOColor(_onColor, 0.3f).SetEase(Ease.OutCubic);
            }

            ValueChanged?.Invoke(isOn);
            _button.rectTransform.DOSizeDelta(new Vector2(40f, 40f), 0.3f).SetEase(Ease.OutCubic);
        }


        public void SetDefaultValue(bool value, bool ignoreSameValue = false)
        {
            if (isOn != value || ignoreSameValue)
            {
                isOn = value;
                if (isOn)
                {
                    _button.rectTransform.localPosition = new Vector3(14f, 0f, 0f);
                    _button.rectTransform.sizeDelta = new Vector2(40f, 40f);

                    _bg.color = _onColor;
                }
                else
                {
                    _button.rectTransform.localPosition = new Vector3(-14f, 0f, 0f);
                    _button.rectTransform.sizeDelta = new Vector2(40f, 40f);

                    _bg.color = _offColor;
                }
            }
            ValueChanged?.Invoke(isOn);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Reset()
        {
            gameObject.SetActive(false);
        }
    }
}