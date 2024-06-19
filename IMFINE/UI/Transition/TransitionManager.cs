using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;

public class TransitionManager : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject RestartTrigger;
    public CanvasGroup[] TransitionItemList;

    [Header("Transition")]
    public float _transitionTime = 3;
    public int _loopCount = -1;
    public bool ConnectEndToStart = true;

    [Header("Fade")]
    public float _fadeDuration = 1;
    public Ease _fadeEaseType = Ease.Linear;
    public bool useBlackFade = false;
    public float _blackFadeDelay = 1;
    public CanvasGroup BlackOverlay;

    Sequence TransitionSequence;

    public Action TransitionFinished;

    void Start()
    {
        TransitionSequence = DOTween.Sequence().SetAutoKill(false).SetLoops(_loopCount, LoopType.Restart)
                                 .SetLink(RestartTrigger, LinkBehaviour.PauseOnDisableRestartOnEnable)
                                 .OnComplete(()=> TransitionFinished?.Invoke());

        foreach (CanvasGroup item in TransitionItemList)
        {
            if (item.Equals(TransitionItemList[0]))
            {
                TransitionSequence.AppendCallback(() =>
                {
                    //트윈 초기설정은 이곳에서
                    if (useBlackFade) BlackOverlay.alpha = 0;
                    item.alpha = 1;
                    item.transform.SetAsLastSibling();
                });
            }
            else
            {
                NextTransition(item);
                if (!ConnectEndToStart)
                    TransitionSequence.AppendInterval(_transitionTime);
                else
                {
                    if (item.Equals(TransitionItemList[TransitionItemList.Length - 1]))
                        NextTransition(TransitionItemList[0]);
                }
            }
        }
    }

    void NextTransition(CanvasGroup _itemCanvas)
    {
        //특정 모션을 따로 추가하고싶다면 여기서
        TransitionSequence.AppendInterval(_transitionTime);
        if (useBlackFade)
        {
            TransitionSequence.Append(BlackOverlay.DOFade(1, _fadeDuration).SetEase(_fadeEaseType))
            .AppendCallback(() => _itemCanvas.transform.SetAsLastSibling())
            .AppendInterval(_blackFadeDelay)
            .Append(BlackOverlay.DOFade(0, _fadeDuration).SetEase(_fadeEaseType));
        }
        else
        {
            TransitionSequence.AppendCallback(() => _itemCanvas.transform.SetAsLastSibling())
            .Join(_itemCanvas.DOFade(1, _fadeDuration).From(0, false).SetEase(_fadeEaseType));
        }
    }



}
