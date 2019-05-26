using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat {
    [SerializeField] float base_value;
    List<IStatBuff> buffs = new List<IStatBuff>();

    float last_calculated;
    bool changed = true;
    protected float value {
        get {
            if (changed) {
                last_calculated = GetBuffedValue();
                changed = false;
                return last_calculated;
            } else {
                return last_calculated;
            }
        }
    }

    float GetBuffedValue() {
        float to_ret = base_value;
        foreach (IStatBuff buff in buffs) {
            to_ret += buff.flat;
        }
        foreach (IStatBuff buff in buffs) {
            if (buff.multi != 0)
                to_ret *= buff.multi;
        }
        return to_ret;
    }

    public virtual void AddBuff(IStatBuff buff) {
        buffs.Add(buff);
        changed = true;
    }

    public virtual void RemoveBuff(IStatBuff buff) {
        buffs.Remove(buff);
        changed = true;
    }

    public virtual void SetBaseValue(float f) {
        base_value = f;
        changed = true;
    }

    public static implicit operator float(Stat s) {
        return s.value;
    }
}

[System.Serializable]
public class CapStat : Stat {

    float _current = 0;
    public float current {
        get { return _current; }
        set {
            _current = value;
            if (current < 0) {
                current = 0;
            } else if (current > this.value) {
                current = this.value;
            }
        }
    }

    public float max {
        get { return this; }
    }

    public override void AddBuff(IStatBuff buff) {
        float value_before = value;
        base.AddBuff(buff);
        float value_after = value;
        if (value_before < value_after) {
            current += value_after - value_before;
        }
        if (current > value) {
            current = value;
        }
    }
    public override void RemoveBuff(IStatBuff buff) {
        base.RemoveBuff(buff);
        if (current > value) {
            current = value;
        }
    }
    public override void SetBaseValue(float f) {
        float value_before = value;
        base.SetBaseValue(f);
        float value_after = value;
        if (value_before < value_after) {
            current += value_after - value_before;
        }
        if (current > value) {
            current = value;
        }
    }
}
