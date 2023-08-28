using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gentleland.StemapunkUI.DemoAndExample
{
    public class ClockSlider : MonoBehaviour
    {
        [SerializeField]
        float minZRotation;

        [SerializeField]
        float maxZRotation;

        [SerializeField]
        float m_value;

        public float Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                UpdateRotation();

            }
        }

        void OnValidate()
        {
            UpdateRotation();
        }
        void UpdateRotation()
        {
            float Zrotation = Mathf.Lerp(minZRotation, maxZRotation, m_value);
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, Zrotation);
        }
    }
}
