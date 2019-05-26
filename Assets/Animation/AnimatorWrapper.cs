using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimatorWrapper : MonoBehaviour, ISerializationCallbackReceiver {

    public float speed {
        get { return anim.speed; }
        set { anim.speed = value; }
    }

    [HideInInspector][SerializeField] Animator anim;

    [SerializeField] List<ClipEventList> anim_clip_events;

    private void Awake() {
        if (anim  == null) anim = GetComponent<Animator>();
    }

    public void SetBool(string parameter, bool set) {
        anim.SetBool(parameter, set);
    }
    public void SetTrigger(string parameter) {
        anim.SetTrigger(parameter);
    }

    public void SetSpeed(float speed) {
        anim.speed = speed;
    }

    public bool IsAnimInState(string state) {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(state);
    }

    public bool RegisterAnimClipEvent(string anim_clip_name, int event_index, UnityAction action) {
        ClipEventList list = GetClipEventList(anim_clip_name);
        if (GetClipEventList(anim_clip_name) == null
            || list.events.Count <= event_index)
            return false;

        list.events[event_index].AddListener(action);
        return true;
    }

    public void TriggerAnimEvent(int event_index) {
        string anim_clip_name = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        ClipEventList list = GetClipEventList(anim_clip_name);
        if (GetClipEventList(anim_clip_name) == null
            || list.events.Count <= event_index)
            return;

        list.events[event_index].Invoke();
    }

    public void ProccessAnimParameterEvent(AnimParameterEvent ape) {
        if (ape.type == AnimParameterEvent.Type.Bool) {
            anim.SetBool(ape.parameter_name, ape.bool_value);
        } else if (ape.type == AnimParameterEvent.Type.Float) {
            anim.SetFloat(ape.parameter_name, ape.float_value);
        } else if (ape.type == AnimParameterEvent.Type.Trigger) {
            anim.SetTrigger(ape.parameter_name);
        }
    }

    public void Rebind() {
        anim.Rebind();
    }

    public void OnBeforeSerialize() {
        if (anim == null) anim = GetComponent<Animator>();
        if (anim_clip_events == null) {
            anim_clip_events = new List<ClipEventList>();
        }
        if (anim.runtimeAnimatorController) {
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips) {
                AnimationEvent[] events = new AnimationEvent[clip.events.Length];
                for (int i = 0; i < clip.events.Length; i++) {
                    events[i] = new AnimationEvent();
                    events[i].time = clip.events[i].time;
                    events[i].functionName = "TriggerAnimEvent";
                    events[i].intParameter = i;
                }

                #if UNITY_EDITOR
                    UnityEditor.AnimationUtility.SetAnimationEvents(clip, events);
                #endif

                if (clip.events.Length != 0)
                    EnsureAnimEventListExists(clip.name, clip.events.Length);
            }
            TrimZeroCountLists();
        }
    }

    public void OnAfterDeserialize() {
    }

    void EnsureAnimEventListExists(string clip_name, int count) {
        foreach (ClipEventList ae in anim_clip_events) {
            if (ae.clip_name == clip_name) {
                if (ae.events.Count != count) {
                    for (int i = ae.events.Count - 1; i > count - 1; i--) {
                        ae.events.RemoveAt(i);
                    }
                    for (int i = ae.events.Count - 1; i < count - 1; i++) {
                        ae.events.Insert(i, new UnityEvent());
                    }
                }
                return;
            }
        }
        anim_clip_events.Add(new ClipEventList(clip_name, count));
        anim_clip_events.Sort((a, b) => string.Compare(a.clip_name, b.clip_name));
    }

    void TrimZeroCountLists() {
        for (int i = anim_clip_events.Count - 1; i >= 0; i--) {
            if (anim_clip_events[0].events.Count == 0) {
                anim_clip_events.RemoveAt(i);
            }
        }
    }

    ClipEventList GetClipEventList(string clip_name) {
        foreach (ClipEventList list in anim_clip_events) {
            if (list.clip_name == clip_name) {
                return list;
            }
        }
        return null;
    }

    [System.Serializable]
    class ClipEventList {
        [HideInInspector]public string clip_name;
        public List<UnityEvent> events;

        public ClipEventList(string name, int count) {
            clip_name = name;
            events = new List<UnityEvent>(count);
            for (int i = 0; i < count; i++) {
                events.Add(new UnityEvent());
            }
        }
    }
}

[System.Serializable]
public class AnimParameterEvent {
    public enum Type { Bool, Trigger, Float }

    public string parameter_name;

    public Type type;
    public bool bool_value;
    public float float_value;

    public AnimParameterEvent Reverse() {
        AnimParameterEvent e = new AnimParameterEvent();
        e.parameter_name = parameter_name;
        e.type = type;
        e.float_value = -float_value;
        e.bool_value = !bool_value;
        return e;
    }
}
