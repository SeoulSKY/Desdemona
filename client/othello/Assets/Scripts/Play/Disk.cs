using System;
using UnityEngine;

namespace Play
{
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
        public static Disk.Color Parse(char ch)
        {
            return ch switch
            {
                DarkDiskChar => Disk.Color.Dark,
                LightDiskChar => Disk.Color.Light,
                var _ => throw new ArgumentException($"Given character cannot be parsed into Color: {ch}"),
            };
        }

        /// <summary>
        /// Convert the color to a char
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <returns>The converted char</returns>
        public static char ToChar(this Disk.Color color)
        {
            return color switch
            {
                Disk.Color.Dark => DarkDiskChar,
                var _ => LightDiskChar,
            };
        }
    }
    
    public class Disk : MonoBehaviour
    {
        public enum Color
        {
            Dark,
            Light,
        }

        public Color CurrentColor { get; private set; }

        /// <summary>
        /// Set the color of this disk instantly with no animations
        /// </summary>
        /// <param name="color">The color to set</param>
        public void SetColor(Color color)
        {
            CurrentColor = color;
            
            var rot = transform.eulerAngles;
            transform.Rotate(color == Color.Dark ? 0 : 180, rot.y, rot.z);
        }
    }
}