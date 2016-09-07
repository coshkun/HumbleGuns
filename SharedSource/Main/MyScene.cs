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

            //var ui = EntityManager.Find<Entity>("UserInterface");
            var button = new Button()
            {
                Text = string.Format("Next scene with {0}", this.Name),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Width = _game.WindowOwner.Width,
                Margin = new Thickness(10),
            };

            button.Click += this.OnUIClick;

            this.EntityManager.Add(button);
        }

        private void OnUIClick(object sender, EventArgs e)
        {
        }
    }
}
