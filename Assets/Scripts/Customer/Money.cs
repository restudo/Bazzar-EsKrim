using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class Money : MonoBehaviour
{
    private ObjectPool<Money> _moneyPool;

    private void OnEnable()
    {
        transform.DOMoveY(3.8f, 1f).SetEase(Ease.OutExpo).OnComplete(BackToThePool);
    }

    private void BackToThePool()
    {
        _moneyPool.Release(this);
    }

    public void SetPool(ObjectPool<Money> moneyPool)
    {
        _moneyPool = moneyPool;
    }
}
