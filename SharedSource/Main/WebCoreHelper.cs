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
        int tickDelay = 20; //in miliseconds

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
            if ((gameTime.TotalMilliseconds % tickDelay) == 0) { tick(); }
        }

        public void tick() { WebCore.Update(); }
    }

    public static class UIEventListener
    {
        static int tickDelay = 20; //in miliseconds

        public static event OnGuiEventHandler OnGUI;
        public static EventArgs e = null;
        static bool  stopped = true;

        public static void UpdateEvents(this UIconnector source)
        {

            if (stopped) { return; }
            //while (true)
            //{
            //    System.Threading.Thread.Sleep(3000);
                if (OnGUI != null)
                {
                    OnGUI(source, e, string.Empty);
                }
            //}
            stopped = false;
        }

        public static void StartListener(this UIconnector source) { stopped = false; }
        public static void StopListener(this UIconnector source)  { stopped = true;  }
        /// <summary>
        /// Int as miliseconds (default value is 20ms).
        /// </summary>
        public static int TickDelay { get { return tickDelay; } set { tickDelay = value; } }
    }
}
