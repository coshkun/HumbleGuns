#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Managers;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
using WaveEngine.Components.UI;
using WaveEngine.Framework.UI;
#endregion

namespace HumbleGuns
{
    public class MyScene : Scene
    {
        private Game _game;
        public MyScene() : base() { }
        public MyScene(Game game) : base() { _game = game; }

        protected override void CreateScene()
        {
            this.Load(WaveContent.Scenes.MyScene);
            var vm = this.VirtualScreenManager;
            vm.Stretch = StretchMode.UniformToFill;

            var ui = EntityManager.Find<Entity>("UserInterface");

            //gc.Click += this.OnUIClick;
            this.EntityManager.Add(gc);
        }

        private void OnUIClick(object sender, EventArgs e)
        {
        }
    }
}
