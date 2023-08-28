using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gentleland.StemapunkUI.DemoAndExample
{
    public class BarsFillAnimations : MonoBehaviour
    {
        Slider[] sliders;
        float[] shifts;
        float[] fillTimeInSeconds;
        float[] durations = { 0.75f, 1.0f, 1.2f, 1.5f, 2.0f, 3.0f };
        private void Start()
        {
            sliders = FindObjectsOfType<Slider>();
            shifts = new float[sliders.Length];
            fillTimeInSeconds = new float[sliders.Length];
            for (int i = 0; i < sliders.Length; i++)
            {
                float number = durations[Mathf.RoundToInt(Random.Range(0, durations.Length))];

                fillTimeInSeconds[i] = number;
                shifts[i] = Random.Range(0, fillTimeInSeconds[i]);
            }
        }

        void Update()
        {
            for (int i = 0; i < sliders.Length; i++)
            {
                float fill;
                float t = (shifts[i] + Time.realtimeSinceStartup) % (fillTimeInSeconds[i] * 2);
                if (t > fillTimeInSeconds[i])
                {
                    fill = 1.0f - (t - fillTimeInSeconds[i]) / fillTimeInSeconds[i];
                }
                else
                {
                    fill = t / fillTimeInSeconds[i];
                }
                sliders[i].value = fill;
            }
        }
    }
}
