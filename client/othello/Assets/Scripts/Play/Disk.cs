using System;
using UnityEngine;

namespace Play
{
    public enum DiskColor
    {
        Dark,
        Light,
    }
    
    public static class DiskColorMethods
    {
        private const char DarkDiskChar = 'D';
        private const char LightDiskChar = 'L';

        /// <summary>
        /// Check if the given character can be parsed into disk color
        /// </summary>
        /// <param name="ch">The character to check</param>
        /// <returns>true if it can, false otherwise</returns>
        public static bool CanParse(char ch)
        {
            return ch is DarkDiskChar or LightDiskChar;
        }
        
        /// <summary>
        /// Parse the given character into a Disk.Color
        /// </summary>
        /// <param name="ch">The character to parse</param>
        /// <returns>The parsed Color</returns>
        /// <exception cref="ArgumentException">If the given character cannot be parsed</exception>
        public static DiskColor Parse(char ch)
        {
            return ch switch
            {
                DarkDiskChar => DiskColor.Dark,
                LightDiskChar => DiskColor.Light,
                var _ => throw new ArgumentException($"Given character cannot be parsed into Color: {ch}"),
            };
        }

        /// <summary>
        /// Convert the color to a char
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <returns>The converted char</returns>
        public static char ToChar(this DiskColor color)
        {
            return color switch
            {
                DiskColor.Dark => DarkDiskChar,
                var _ => LightDiskChar,
            };
        }

        /// <summary>
        /// Returns the opposite color of this color
        /// </summary>
        /// <param name="color">The original color</param>
        /// <returns>The opposite color</returns>
        public static DiskColor Opposite(this DiskColor color)
        {
            return color switch
            {
                DiskColor.Dark => DiskColor.Light,
                var _ => DiskColor.Dark,
            };
        }
    }
    
    public class Disk : MonoBehaviour
    {
        private Animator _animator;
        private int _darkHash;
        private int _lightHash;
        private int _darkNoTransitionHash;
        private int _lightNoTransitionHash;

        private bool _isReady;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _darkHash = Animator.StringToHash("dark");
            _lightHash = Animator.StringToHash("light");
            _darkNoTransitionHash = Animator.StringToHash("darkNoTransition");
            _lightNoTransitionHash = Animator.StringToHash("lightNoTransition");
            
            _isReady = true;
        }

        private DiskColor _color = DiskColor.Dark;
        
        public DiskColor Color
        {
            get
            {
                return _color;
            }

            set
            {
                if (!gameObject.activeSelf)
                {
                    throw new InvalidOperationException("This disk must be active to set its color");
                }

                if (_color == value)
                {
                    return;
                }
            
                _color = value;

                if (!_isReady)
                {
                    Awake();
                }

                _animator.SetTrigger(value == DiskColor.Dark ? _darkNoTransitionHash : _lightNoTransitionHash);
            }
        }
        
        /// <summary>
        /// Flip this disk so that it shows the opposite color
        /// </summary>
        public void Flip()
        {
            _animator.SetTrigger(Color == DiskColor.Dark ? _lightHash : _darkHash);
            _color = Color.Opposite();
        }
    }
}