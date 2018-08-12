using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(DOTweenAnimation))]
public class AdvancedDOTweenAnimation : MonoBehaviour, IGenericValue<Tween>
{
    public DOTweenAnimation Animation;
    public Tween Value
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return Animation.CreateEditorPreview();
#endif
            return Animation.tween;
        }
    }

    void Reset()
    {
        Animation = GetComponent<DOTweenAnimation>();
    }
}