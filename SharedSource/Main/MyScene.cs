#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
using WaveEngine.Components.UI;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;
#endregion

namespace HumbleGuns
{
    public class MyScene : Scene
    {
        public static VirtualScreenManager vm;

        private Game _game;
        public MyScene() : base() { }
        public MyScene(Game game) : base() { _game = game; }

        protected override void CreateScene()
        {
            this.Load(WaveContent.Scenes.MyScene);

            var cam2 = EntityManager.Find<Entity>("defaultCamera2D");
            var t = cam2.FindComponent<Transform2D>();
            t.LocalPosition = new Vector2(VirtualScreenManager.VirtualWidth/2, 
                                          VirtualScreenManager.VirtualHeight/2);

            var ui = EntityManager.Find<Entity>("UserInterface");
            var uic = new UIconnector(this);
            ui.AddComponent(uic);
        }
    }
}
