using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Framework;

namespace HumbleGuns
{
    public delegate void OnGuiEventHandler(object sender, EventArgs args, string name);

    public class WebCoreHelper : Behavior
    {
        int tickDelay = 2; //in miliseconds

        /// <summary>
        /// Instantiates Webcore Updater Class with given tick delay in miliseconds (20ms is default)
        /// </summary>
        public WebCoreHelper() : base("WebCoreHelper") { }
        /// <summary>
        /// Instantiates Webcore Updater Class with given tick delay in miliseconds (20ms is default)
        /// </summary>
        /// <param name="TickDelay">Tick delay in miliseconds (20ms is default)</param>
        public WebCoreHelper(int TickDelay) : base("WebCoreHelper") { tickDelay = TickDelay; }

        protected override void Update(TimeSpan gameTime)
        {
            if ((gameTime.Milliseconds % tickDelay) == 0)
            { tick(); }
        }

        public void tick() { WebCore.Update(); }
    }

    public static class UIEventListener
    {
        static int tickDelay = 10; //in miliseconds
        static DateTime hitTime;

        private static event OnGuiEventHandler OnGUI;
        public static EventArgs e = null;
        static bool  stopped = true;

        public static void UpdateEvents(this UIconnector source)
        {
            OnGUI += source.OnGui;
            if (stopped) { return; }
            //while (true)
            //{
            //    System.Threading.Thread.Sleep(3000);
                if (OnGUI != null)
                {
                    OnGUI(source, e, string.Empty);
                    hitTime = DateTime.Now;
                }
            //}
        }

        public static void StartListener(this UIconnector source) { stopped = false; }
        public static void StopListener(this UIconnector source)  { stopped = true;  }
        /// <summary>
        /// Int as miliseconds (default value is 20ms).
        /// </summary>
        public static int TickDelay { get { return tickDelay; } set { tickDelay = value; } }
        public static DateTime HitTime { get { return hitTime; } /* set { hitTime = value; } */}
    }
}
