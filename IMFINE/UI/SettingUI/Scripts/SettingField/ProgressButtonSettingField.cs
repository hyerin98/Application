using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IMFINE.UI.SettingUI
{
    public class ProgressButtonSettingField : SettingField, ISettingField
    {
        [SerializeField] private float progressTime;
        [SerializeField] private Button button;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text progressText;
        Tweener progressTweener;

        public void SetName(string name)
        {
        }

        public void AddListener(UnityAction action)
        {
            if (button != null)
                button.onClick.AddListener(action);
        }

        public void RemoveListener()
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }

        private void OnEnable()
        {
            progressTweener = DOVirtual.Float(0, 1, progressTime, (x) =>
            {
                progressFill.fillAmount = x;
                progressText.text = Mathf.CeilToInt((1 - x) * progressTime).ToString();
            }
            ).OnComplete(FinishButtonProgress).SetEase(Ease.Linear).SetAutoKill(false).Pause();
        }

        private void OnDisable()
        {
            progressTweener.Kill();
        }

        public void OnButtonPointerDown()
        {
            progressTweener.Restart();
        }

        public void OnButtonPointerUp()
        {
            progressTweener.Pause();
            progressFill.fillAmount = 0;
            progressText.text = "";
        }

        private void FinishButtonProgress()
        {
            button.onClick.Invoke();
            OnButtonPointerUp();
        }

        public void Show()
        {
            throw new System.NotImplementedException();
        }

        public void Hide()
        {
            throw new System.NotImplementedException();
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}