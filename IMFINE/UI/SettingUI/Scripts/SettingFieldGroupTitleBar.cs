namespace IMFINE.UI.SettingUI
{
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class SettingFieldGroupTitleBar : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public delegate void SettingFieldGroupTitleBarEvent(bool isOpen);
        public event SettingFieldGroupTitleBarEvent Changed;

        [SerializeField]
        private Image _arrow;
        [SerializeField]
        private Text _titleText;
        [SerializeField]
        private Image _lockIcon;
        [SerializeField]
        private Image _bg;
        [HideInInspector]
        private bool _isOpen = false;

        public bool isOpen
        {
            get
            {
                return _isOpen;
            }
            set
            {
                _isOpen = value;

                if (_isOpen)
                {
                    _lockIcon.enabled = false;
                    _arrow.rectTransform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.3f).SetId("arrow.rotate" + GetInstanceID()).SetEase(Ease.InOutCubic);
                    _arrow.DOFade(0.5f, 0.3f).SetId("arrow.fade" + GetInstanceID()).SetEase(Ease.InOutCubic);
                }
                else
                {
                    _lockIcon.enabled = true;
                    _arrow.rectTransform.DOLocalRotate(new Vector3(0f, 0f, 90f), 0.3f).SetId("arrow.rotate" + GetInstanceID()).SetEase(Ease.InOutCubic);
                    _arrow.DOFade(1f, 0.3f).SetId("arrow.fade" + GetInstanceID()).SetEase(Ease.InOutCubic);
                }

                Changed?.Invoke(_isOpen);
            }
        }

        void Start()
        {
            UpdateLockIconPos();
            UpdateBgWidth();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            DOTween.Kill("bg.fade" + GetInstanceID());
            _bg.DOFade(0.1f, 0f).SetId("bg.fade" + GetInstanceID()).SetEase(Ease.OutCubic);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DOTween.Kill("bg.fade" + GetInstanceID());
            _bg.DOFade(0f, 0.4f).SetId("bg.fade" + GetInstanceID()).SetEase(Ease.OutCubic);

            DOTween.Kill("arrow.rotate" + GetInstanceID());
            if (_isOpen)
            {
                _isOpen = false;
                _lockIcon.enabled = true;
                _arrow.rectTransform.DOLocalRotate(new Vector3(0f, 0f, 90f), 0.3f).SetId("arrow.rotate" + GetInstanceID()).SetEase(Ease.InOutCubic);
                _arrow.DOFade(1f, 0.3f).SetId("arrow.fade" + GetInstanceID()).SetEase(Ease.InOutCubic);
            }
            else
            {
                _isOpen = true;
                _lockIcon.enabled = false;
                _arrow.rectTransform.DOLocalRotate(new Vector3(0f, 0f, 0f), 0.3f).SetId("arrow.rotate" + GetInstanceID()).SetEase(Ease.InOutCubic);
                _arrow.DOFade(0.5f, 0.3f).SetId("arrow.fade" + GetInstanceID()).SetEase(Ease.InOutCubic);
            }

            Changed?.Invoke(_isOpen);
        }




        private void UpdateLockIconPos()
        {
            float lockIconLeftSpace = 30f;
            float lockIconPosX = _titleText.rectTransform.localPosition.x + _titleText.preferredWidth + lockIconLeftSpace;
            Vector3 lockIconPos = _lockIcon.rectTransform.localPosition;
            lockIconPos.x = lockIconPosX;
            _lockIcon.rectTransform.localPosition = lockIconPos;
        }

        private void UpdateBgWidth()
        {
            Vector2 bgSize = _bg.rectTransform.sizeDelta;
            bgSize.x = _titleText.rectTransform.localPosition.x + _titleText.preferredWidth + 100f;
            _bg.rectTransform.sizeDelta = bgSize;
        }




        public void SetTitle(string title)
        {
            _titleText.text = title.ToUpper();

            UpdateBgWidth();
            UpdateLockIconPos();
        }
    }
}
