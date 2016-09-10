using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Framework;

namespace HumbleGuns
{
    public class WebCoreHelper : Behavior
    {
        int tickDelay = 20; //in miliseconds
        /// <summary>
        /// Instantiates Webcore Updater Class with given tick delay in miliseconds (20ms is default)
        /// </summary>
        public WebCoreHelper() : base("WebCoreHelper") {}
        /// <summary>
        /// Instantiates Webcore Updater Class with given tick delay in miliseconds (20ms is default)
        /// </summary>
        /// <param name="TickDelay">Tick delay in miliseconds (20ms is default)</param>
        public WebCoreHelper(int TickDelay) : base("WebCoreHelper") { tickDelay = TickDelay; }

        protected override void Update(TimeSpan gameTime)
        {
            if ((gameTime.TotalMilliseconds % tickDelay) == 0) { tick(); }
        }

        public void tick()
        {
                WebCore.Update();
        }
    }
}
