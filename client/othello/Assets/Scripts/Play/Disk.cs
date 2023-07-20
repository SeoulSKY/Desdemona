using UnityEngine;

namespace Play
{
    public class Disk : MonoBehaviour
    {
        /// <summary>
        /// Representative character for dark disk
        /// </summary>
        public const char Dark = 'D';
        
        /// <summary>
        /// Representative character for light disk
        /// </summary>
        public const char Light = 'L';
        
        [Tooltip("Whether the color of this disk is dark or not")]
        [SerializeField] private bool isDark = true;

        public void FlipColor()
        {
            isDark = !isDark;

            var rot = transform.rotation;
            transform.rotation = new Quaternion(isDark ? 0 : 180, rot.y, rot.z, rot.w);
        }
    }
}