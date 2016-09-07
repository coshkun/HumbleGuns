#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
#endregion

namespace HumbleGuns
{
    public class Game : WaveEngine.Framework.Game
    {
        public App WindowOwner { get; private set; }
        public ScreenContext ScreenContext { get; private set; }
        public Game() : base() { }
        public Game(App sender) : base() { WindowOwner = sender; }

        public override void Initialize(IApplication application)
        {
            base.Initialize(application);

			ScreenContext screenContext = new ScreenContext(new MyScene(this)); ScreenContext = screenContext;
            WaveServices.ScreenContextManager.To(screenContext);
        }
    }
}
