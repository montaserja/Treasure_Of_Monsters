using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5_5_OR_NEWER

public class RFX4_EffectSettingsTimeScale : MonoBehaviour
{

    public float Time = 1;

    float prevTime;
    ParticleSystem[] particles;

    
    void Start()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        if (Math.Abs(prevTime - Time) > 0.0001f)
        {
            prevTime = Time;
            UpdateTime();

        }
    }

    void UpdateTime()
    {
        foreach (var ps in particles)
        {

            var main = ps.main;
            main.simulationSpeed = Time;
        }
        var tm = GetComponentInChildren<RFX4_TransformMotion>();
        if (tm != null)
        {
            tm.Speed *= Time;
            foreach (var go in tm.EffectsOnCollision)
            {
                var particlesCollided = go.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particlesCollided)
                {
                    var main = ps.main;
                    main.simulationSpeed = Time;
                }
            }
        }

var floatCurves = GetComponentsInChildren<RFX4_ShaderFloatCurve>();
        foreach (var rfx4ShaderFloatCurve in floatCurves)
        {
            rfx4ShaderFloatCurve.GraphTimeMultiplier /= Time;
        }

        var colorGradients = GetComponentsInChildren<RFX4_ShaderColorGradient>();
        foreach (var rfx4_ShaderColorGradient in colorGradients)
        {
            rfx4_ShaderColorGradient.TimeMultiplier /= Time;
        }

        var lightCurves = GetComponentsInChildren<RFX4_LightCurves>();
        foreach (var rfx4LightCurve in lightCurves)
        {
            rfx4LightCurve.GraphTimeMultiplier /= Time;
        }
    }
}
#endif
