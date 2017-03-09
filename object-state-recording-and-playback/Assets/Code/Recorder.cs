using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour {
    public int record_frequency = 20;

    public float record_start_time;
    public float record_length;
    public float play_start_time;

    public bool reverse = false;

    public bool disable_collisions_playback = false;

    protected float last_recorded_at = -1.0f;

    protected RecordData record_data;

    protected bool recording = false;
    protected bool playing = false;

    protected Transform[] transforms;

    protected struct RigidbodyValues
    {
        public bool kinematic;
        public bool detect_collisions;
    }
    protected List<RigidbodyValues> cached_rigidbody_values;

    public void StartRecording()
    {
        transforms = GetComponentsInChildren<Transform>();

        record_data = new RecordData();
        record_data.Initialize(transforms.Length);

        last_recorded_at = -1.0f;

        record_start_time = Time.time;

        recording = true;
    }

    public void StopRecording()
    {
        recording = false;
        record_length = Time.time - record_start_time;
    }

    public void StartPlaying()
    {
        play_start_time = Time.time;
        playing = true;

        if (disable_collisions_playback)
        {
            cached_rigidbody_values = new List<RigidbodyValues>(transforms.Length);

            foreach (var trans in transforms)
            {
                RigidbodyValues cached_values;

                if (trans.GetComponent<Rigidbody>())
                {
                    cached_values.kinematic = trans.GetComponent<Rigidbody>().isKinematic;
                    cached_values.detect_collisions = trans.GetComponent<Rigidbody>().detectCollisions;

                    trans.GetComponent<Rigidbody>().isKinematic = true;
                    trans.GetComponent<Rigidbody>().detectCollisions = false;
                }
                else if (trans.GetComponent<Rigidbody2D>())
                {
                    cached_values.kinematic = trans.GetComponent<Rigidbody2D>().isKinematic;
                    cached_values.detect_collisions = false;

                    trans.GetComponent<Rigidbody2D>().isKinematic = true;
                    //trans.GetComponent<Rigidbody2D>().detectCollisions = false;
                }
                else
                    continue;

                cached_rigidbody_values.Add(cached_values);
            }
        }
    }

    public void StopPlaying()
    {
        playing = false;

        if (disable_collisions_playback)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].GetComponent<Rigidbody>())
                {
                    transforms[i].GetComponent<Rigidbody>().isKinematic = cached_rigidbody_values[i].kinematic;
                    transforms[i].GetComponent<Rigidbody>().detectCollisions = cached_rigidbody_values[i].detect_collisions;
                }
                else if (transforms[i].GetComponent<Rigidbody2D>())
                {
                    transforms[i].GetComponent<Rigidbody2D>().isKinematic = cached_rigidbody_values[i].kinematic;
                    //transforms[i].GetComponent<Rigidbody2D>().detectCollisions = cached_rigidbody_values[i].detect_collisions;
                }
            }
        }
    }

    public void SetTo(float normalized_time)
    {
        
    }

    protected void Record()
    {
        if (last_recorded_at >= 0.0f && last_recorded_at > (Time.time - 1.0f/record_frequency))
        {
            return;
        }
        // Gather states
        RecordData.RecordObjectState[] states = new RecordData.RecordObjectState[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            RecordData.RecordObjectState state = new RecordData.RecordObjectState();
            state.position = transforms[i].localPosition;
            state.rotation = transforms[i].localRotation;
            state.scale = transforms[i].localScale;
            state.active = transforms[i].gameObject.activeSelf;
            state.transform = transforms[i];

            states[i] = state;
        }

        record_data.SaveStates(states, Time.time - record_start_time);

        last_recorded_at = Time.time;
    }

    protected void Play()
    {
        float time = Time.time - play_start_time;

        if (reverse)
        {
            time = record_length - time;
            if (time < 0.0f)
                time = 0.0f;
        }

        RecordData.RecordObjectState[] states = record_data.GetInterpolatedStates(time);
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].localPosition = states[i].position;
            transforms[i].localRotation = states[i].rotation;
            //transforms[i]scale thing
        }

        
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!recording)
                StartRecording();
            else
                StopRecording();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!playing)
                StartPlaying();
            else
                StopPlaying();
        }

        if (recording)
            Record();
        if (playing)
            Play();
    }
}
