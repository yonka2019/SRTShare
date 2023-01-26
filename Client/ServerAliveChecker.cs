﻿using System.Threading;
using System.Timers;

namespace Client
{
    internal static class ServerAliveChecker
    {
        private const int TIMEOUT_SECONDS = 5;
        private static bool firstCheck = true;

        private static int timeoutSeconds;

        private static System.Timers.Timer timer;

        internal delegate void Notify();
        internal static event Notify LostConnection;

        static ServerAliveChecker()
        {
            timeoutSeconds = 0;

            timer = new System.Timers.Timer(1000);  // 1 second timeout
            timer.Elapsed += Timer_Elapsed;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeoutSeconds++;

            if (timeoutSeconds == TIMEOUT_SECONDS)  // ALIVE TIMED-OUT
            {
                LostConnection.Invoke();

                timer.Stop();
                timer.Dispose();
            }
        }

        /// <summary>
        /// The server-alive check begans when the server start to send data packets, if the check is first, so it's runs the thread, 
        /// if it's not the first check, so the program confirms that he received data packet, and the server still alive
        /// </summary>
        internal static void Check()
        {
            if (firstCheck)
            {
                firstCheck = false;
                Thread saChecker = new Thread(new ThreadStart(AliveChecker));  // create thread of keep-alive checker
                saChecker.Start();
            }
            else
            {
                ConfirmStatus();
            }
        }

        internal static void ConfirmStatus()  // reset timeout seconds
        {
            timeoutSeconds = 0;
        }

        private static void AliveChecker()
        {
            timer.Start();
        }
    }
}
