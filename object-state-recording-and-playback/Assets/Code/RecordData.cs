using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordData {
    protected int object_count = 1;

    public struct RecordObjectState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public bool active;

        public Transform transform;
    }

    // key = timestamp
    SortedList<float, RecordObjectState[]> states = new SortedList<float, RecordObjectState[]>();

    public void Initialize(int _object_count)
    {
        object_count = _object_count;
    }

    public RecordObjectState[] GetInterpolatedStates(float time)
    {
        int next_key = FindFirstIndexGreaterThanOrEqualTo(states, time);
        if (next_key >= states.Keys.Count)
            next_key = states.Keys.Count - 1;
        int previous_key = next_key - 1;
        if (previous_key < 0)
            previous_key = 0;

        float bias = states.Keys[next_key] - states.Keys[previous_key];
        bias = (time - states.Keys[previous_key])/bias;
        if (bias > 1.0f)
            bias = 1.0f;
        if (float.IsNaN(bias))
            bias = 0.0f;

        RecordObjectState[] interpolated_states = new RecordObjectState[object_count];

        for (int i = 0; i < object_count; i++)
        {
            RecordObjectState next_state = states[states.Keys[next_key]][i];
            RecordObjectState prev_state = states[states.Keys[previous_key]][i];

            interpolated_states[i].position = prev_state.position + ((next_state.position - prev_state.position) * bias);
            interpolated_states[i].rotation = Quaternion.Lerp(prev_state.rotation, next_state.rotation, bias);
            //interpolated_states[i].rotation = Quaternion.Euler(prev_state.rotation.eulerAngles + ((next_state.rotation.eulerAngles - prev_state.rotation.eulerAngles) * bias));
        }

        return interpolated_states;
    }

    public RecordObjectState[] GetClosestStates(float time)
    {
        int key_index = FindFirstIndexGreaterThanOrEqualTo(states, time);
        if (key_index >= states.Keys.Count)
            key_index = states.Keys.Count - 1;
        return states[states.Keys[key_index]];
    }

    public float GetClosestKey(float time)
    {
        int key_index = FindFirstIndexGreaterThanOrEqualTo(states, time);
        return states.Keys[key_index];
    }

    public void SaveStates(RecordObjectState[] _states, float time)
    {
        states.Add(time, _states);
    }

    protected int FindFirstIndexGreaterThanOrEqualTo(SortedList<float, RecordObjectState[]> sortedList, float key)
    {
        return BinarySearch(sortedList.Keys, key);
    }

    protected int BinarySearch(IList<float> list, float value)
    {
        if (list == null)
            throw new ArgumentNullException("list");
        var comp = Comparer<float>.Default;
        int lo = 0, hi = list.Count - 1;
        while (lo < hi)
        {
            int m = (hi + lo) / 2;  // this might overflow; be careful.
            if (comp.Compare(list[m], value) < 0) lo = m + 1;
            else hi = m - 1;
        }
        if (comp.Compare(list[lo], value) < 0) lo++;
        return lo;
    }
}
