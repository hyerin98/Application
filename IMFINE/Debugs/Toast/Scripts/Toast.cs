namespace IMFINE.Debugs
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using IMFINE.Utils;

    public class Toast : MonoSingleton<Toast>
    {
        [SerializeField]
        private float _displayTimeSec = 3f;
        [SerializeField]
        private bool _isTestMode = false;
        [SerializeField]
        private Text _text;
        [SerializeField]
        private Image _bg;
        [SerializeField]
        private CanvasGroup _canvasGroup;

        void Awake()
        {
            _canvasGroup.DOFade(0f, 0f).SetId("toast.fade" + GetInstanceID());
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        void Update()
        {
            if (_isTestMode)
            {
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    Bake("Toast - Jun-hee's birthday is December 21st. " + Random.Range(100000, 1000000000).ToString());
                }
            }
        }
#endif

        private void OnDestroy()
        {
            DOTween.Kill("toast.fade" + GetInstanceID());
            DOTween.Kill("toast.bake.delay" + GetInstanceID());
        }




        public void Bake(string value)
        {
            _text.text = value;
            _bg.rectTransform.sizeDelta = new Vector2(_text.preferredWidth + 100f, 100f);

            DOTween.Kill("toast.fade" + GetInstanceID());
            DOTween.Kill("toast.bake.delay" + GetInstanceID());
            _canvasGroup.DOFade(1f, 0f).SetId("toast.fade" + GetInstanceID());
            DOVirtual.DelayedCall(_displayTimeSec, OnBaked).SetId("toast.bake.delay" + GetInstanceID());
        }

        private void OnBaked()
        {
            DOTween.Kill("toast.fade" + GetInstanceID());
            DOTween.Kill("toast.bake.delay" + GetInstanceID());
            _canvasGroup.DOFade(0f, 0.5f).SetId("toast.fade" + GetInstanceID());
        }
    }
}