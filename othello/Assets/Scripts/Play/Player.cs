using System;

namespace Play
{
    public static class PlayerMethods
    {
        private const char BotChar = 'B';
        private const char HumanChar = 'H';
        
        /// <summary>
        /// Check if the given character can be parsed into Player
        /// </summary>
        /// <param name="ch">The character to check</param>
        /// <returns>true if it can, false otherwise</returns>
        public static bool CanParse(char ch)
        {
            return ch is BotChar or HumanChar;
        }
        
        /// <summary>
        /// Parse the given character into a Player
        /// </summary>
        /// <param name="ch">The character to parse</param>
        /// <returns>The parsed Player</returns>
        /// <exception cref="ArgumentException">If the given character cannot be parsed</exception>
        public static Player Parse(char ch)
        {
            return ch switch
            {
                BotChar => Player.Bot,
                HumanChar => Player.Human,
                var _ => throw new ArgumentException($"Given character cannot be parsed into Player: {ch}"),
            };
        }

        /// <summary>
        /// Convert the player to a char
        /// </summary>
        /// <param name="player">The player to convert</param>
        /// <returns>The converted char</returns>
        public static char ToChar(this Player player)
        {
            return player == Player.Bot ? BotChar : HumanChar;
        }

        /// <summary>
        /// Returns the corresponding disk color for this player
        /// </summary>
        /// <param name="player">The player to convert</param>
        /// <returns></returns>
        public static DiskColor Disk(this Player player)
        {
            return player == Player.Bot ? DiskColor.Light : DiskColor.Dark;
        }
    }
    
    public enum Player
    {
        Bot,
        Human,
    }
}