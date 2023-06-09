﻿using System;
using System.Collections.Generic;

/*
 * [C] Yonka
 */
namespace SRTShareLib
{
    public static class CColorManager  // Console Color Manager
    {
        private static readonly ConsoleColor defaultBackground;
        private static readonly ConsoleColor defaultForeground;  // foreground <=> text

        private static readonly object _lock = new object();

        private static Dictionary<MessageType, CColor> colorTypes;

        static CColorManager()
        {
            defaultBackground = ConsoleColor.Black;
            defaultForeground = ConsoleColor.Gray;
            SetValues();
        }

        // support only '\n' in the END of the string [because it will be much complicated to handle with] "lala\n" - good ;; "la\nlala" - bad
        public static void Write(string str, MessageType mType)
        {
            lock (_lock)
            {
                Console.BackgroundColor = colorTypes[mType].Background;
                Console.ForegroundColor = colorTypes[mType].Foreground;

                if (str.EndsWith("\n"))  // given string ends with new line char
                {
                    int newLines = CountNewLines(str);

                    str = str.Replace("\n", "");  // remove them to prevent colored new line down
                    Console.Write(str);  // print without them
                    Console.ResetColor();  // remove colors

                    for (int i = 0; i < newLines; i++)
                    {
                        Console.WriteLine();  // print each of them (\n)
                    }
                }
                else
                {
                    Console.Write(str);
                    Console.ResetColor();
                }
            }
        }

        // support only '\n' in the END of the string [because it will be much complicated to handle with] "lala\n" - good ;; "la\nlala" - bad
        public static void WriteLine(string str, MessageType mType)
        {
            lock (_lock)
            {
                Console.BackgroundColor = colorTypes[mType].Background;
                Console.ForegroundColor = colorTypes[mType].Foreground;

                if (str.EndsWith("\n"))  // given string ends with new line char
                {
                    int newLines = CountNewLines(str);

                    str = str.Replace("\n", "");  // remove them to prevent colored new line down
                    Console.Write(str);  // print without them
                    Console.ResetColor();  // remove colors

                    for (int i = 0; i < newLines; i++)
                    {
                        Console.WriteLine();  // print each of them (\n)
                    }
                }
                else
                {
                    Console.Write(str);
                    Console.ResetColor();
                }

                Console.WriteLine();  // '\n' because method is WriteLine
            }
        }

        /// <summary>
        /// Counts the \n in the given string to remove them later, and restore them with WriteLINE function
        /// </summary>
        /// <param name="str">String to search in</param>
        /// <returns>number of \n in the given string</returns>
        private static int CountNewLines(string str)
        {
            int count = 0;

            for (int i = str.Length - 1; i >= 0; i--)
            {
                if (str[i] == '\n')
                    count++;
            }
            return count;
        }

        private static void SetValues()
        {
            colorTypes = new Dictionary<MessageType, CColor>
            {
                { MessageType.txtDefault, new CColor(defaultBackground, defaultForeground) },
                { MessageType.txtMuted, new CColor(defaultBackground, ConsoleColor.DarkGray) },
                { MessageType.txtWarning, new CColor(defaultBackground, ConsoleColor.Yellow) },
                { MessageType.txtError, new CColor(defaultBackground, ConsoleColor.Red) },
                { MessageType.txtSuccess, new CColor(defaultBackground, ConsoleColor.Green) },
                { MessageType.txtInfo, new CColor(defaultBackground, ConsoleColor.DarkCyan) },
                { MessageType.bgDefault, new CColor(defaultBackground, defaultForeground) },
                { MessageType.bgMuted, new CColor(ConsoleColor.DarkGray, ConsoleColor.Black) },
                { MessageType.bgPrimary, new CColor(ConsoleColor.Gray, ConsoleColor.White) },
                { MessageType.bgWarning, new CColor(ConsoleColor.Yellow, ConsoleColor.Black) },
                { MessageType.bgError, new CColor(ConsoleColor.Red, ConsoleColor.White) },
                { MessageType.bgSuccess, new CColor(ConsoleColor.DarkGreen, ConsoleColor.White) },
                { MessageType.bgInfo, new CColor(ConsoleColor.DarkCyan, ConsoleColor.White) }
            };
        }

    }
    public struct CColor
    {
        public ConsoleColor Background { get; private set; }  // console background color
        public ConsoleColor Foreground { get; private set; }  // console text color

        public CColor(ConsoleColor background, ConsoleColor foreground)
        {
            Background = background;
            Foreground = foreground;
        }
    }

    public enum MessageType
    {
        // foreground (text) color
        txtDefault,  // 7
        txtMuted,  // 8
        txtSuccess,  // A
        txtInfo,  // 3
        txtWarning,  // E
        txtError,  // C

        // background color
        bgDefault,  // 0 7
        bgMuted,  // 8 0
        bgPrimary,  // 7 F
        bgSuccess,  // 2 F
        bgInfo,  // 3 F
        bgWarning,  // E 0
        bgError  // C F
    }
}
