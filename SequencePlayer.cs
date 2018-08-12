using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;

public class SequencePlayer : SerializedMonoBehaviour, IGenericValue<Tween>
{
    private Sequence sequence;

    [BoxGroup("Options")]
    public UpdateType UpdateType = UpdateType.Normal;

    [BoxGroup("Options")]
    public bool PlayOnEnable;

    [BoxGroup("Options")]
    public bool AutoKill = false;

    [BoxGroup("Tweens"),OnValueChanged("CheckIfNotSelf")]
    public List<IGenericValue<Tween>> TweensBeforeParallel = new List<IGenericValue<Tween>>();
    [BoxGroup("Tweens"), OnValueChanged("CheckIfNotSelf")]
    public List<IGenericValue<Tween>> ParallelTweens = new List<IGenericValue<Tween>>();
    [BoxGroup("Tweens"), OnValueChanged("CheckIfNotSelf")]
    public List<IGenericValue<Tween>> TweensAfterParallel = new List<IGenericValue<Tween>>();

    [ShowInInspector, FoldoutGroup("Status", false)]
    public bool IsInitialized => sequence?.IsInitialized() ?? false;
    [ShowInInspector, FoldoutGroup("Status", false)]
    public bool IsNull => sequence?.IsNull() ?? true;
    [ShowInInspector, FoldoutGroup("Status", false)]
    public bool IsPlaying => sequence?.IsPlaying() ?? false;
    [ShowInInspector, FoldoutGroup("Status", false)]
    public bool IsComplete => sequence?.IsComplete() ?? false;

    public void OnEnable()
    {
        GenerateSequence();
        if (PlayOnEnable)
            Play();
    }

    private void GenerateSequence()
    {
        if (sequence != null)
            return;

        RegenerateSequence();
    }

    public void RegenerateSequence()
    {
        DOTween.Init();

        sequence = DOTween.Sequence();
        sequence.SetUpdate(UpdateType);
        sequence.SetAutoKill(AutoKill);

        float insertTime = 0;

        foreach (var tween in TweensBeforeParallel)
        {
            sequence.Append(tween.Value);
            insertTime += tween.Value.Duration();
        }

        foreach (var parallelTween in ParallelTweens)
        {
            sequence.Insert(insertTime, parallelTween.Value);
        }

        foreach (var tween in TweensAfterParallel)
        {
            sequence.Append(tween.Value);
        }
    }

    public Tween Value
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                RegenerateSequence();
            else
#endif
                GenerateSequence();
            return sequence;
        }
    }

    [Button(ButtonSizes.Medium), BoxGroup("Utils"), HideIf("IsEditMode")]
    public void Play()
    {
        sequence.Play();
    }

    [Button(ButtonSizes.Medium), BoxGroup("Utils"), HideIf("IsEditMode")]
    public void Restart()
    {
        sequence.Restart();
    }

    [Button(ButtonSizes.Medium), BoxGroup("Utils"), HideIf("IsEditMode")]
    public void Complete()
    {
        sequence.Complete();
    }

    [Button(ButtonSizes.Medium), BoxGroup("Utils"), HideIf("IsEditMode")]
    public void Rewind()
    {
        sequence.Rewind();
    }

#if UNITY_EDITOR

    private bool IsEditMode => !Application.isPlaying;

    public void CheckIfNotSelf()
    {
        if (TweensBeforeParallel.Any(_ => _ as SequencePlayer == this))
            TweensBeforeParallel.RemoveAll(_ => _ as SequencePlayer == this);
        if (ParallelTweens.Any(_ => _ as SequencePlayer == this))
            ParallelTweens.RemoveAll(_ => _ as SequencePlayer == this);
        if (TweensAfterParallel.Any(_ => _ as SequencePlayer == this))
            TweensAfterParallel.RemoveAll(_ => _ as SequencePlayer == this);
    }


    [Button(ButtonSizes.Medium), BoxGroup("Utils"), ShowIf("IsEditMode")]
    public void PlayPreview()
    {
        RegenerateSequence();
        sequence.SetUpdate(UpdateType.Manual);

        sequence.ForceInit();
        sequence.SetAutoKill(false);
        sequence.Restart(false);
        EditorUpdate.EditorUpdatePlug(EditorPlay);
    }

    [Button(ButtonSizes.Medium), BoxGroup("Utils"), ShowIf("IsEditMode")]
    public void StopPreview()
    {
        try
        {
            sequence?.Complete();
            sequence.Rewind();
        }
        catch (Exception)
        {
            // ignored
        }

        EditorUpdate.RemoveEditorUpdatePlug(EditorPlay);
    }

    public void EditorPlay(double deltaTime)
    {
        DOTween.ManualUpdate((float)deltaTime, (float)deltaTime);

        if (sequence.IsComplete())
        {

            sequence.Rewind(false);
            EditorUpdate.RemoveEditorUpdatePlug(EditorPlay);
        }
    }
#endif
}