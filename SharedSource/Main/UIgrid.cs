﻿using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.UI;
using WaveEngine.Framework.Managers;
using WaveEngine.Common.Graphics;

namespace HumbleGuns
{
    public class UIgrid : Grid
    {
        private Scene cs;
        private VirtualScreenManager vm;
        private ImageControl ic;
        public ImageControl Interface { get; set; }

        public new Color BackgroundColor
        {
            get
            {
                Color color = Color.White;

                ImageControl imageControl = this.entity.FindComponent<ImageControl>();
                if (imageControl != null)
                {
                    color = imageControl.TintColor;
                }
                else
                {
                    throw new Exception("This panel haven't background assigned");
                }

                return color;
            }

            set
            {
                ImageControl imageControl = this.entity.FindComponent<ImageControl>();
                if (imageControl != null)
                {
                    imageControl.TintColor = value;
                }
                else
                {
                    this.entity.AddComponent(new ImageControl(value, (int)this.Width, (int)this.Height))
                               .AddComponent(new ImageControlRenderer());
                }
            }
        }

        public UIgrid(Scene CurrentScene)
        {
            this.cs = CurrentScene;
            vm = cs.VirtualScreenManager;
            vm.Stretch = StretchMode.UniformToFill;
            SetUpGrid();

            //Entity ui = CurrentScene.EntityManager.Find<Entity>("UserInterface");

            var button = new Button()
            {
                //Text = string.Format("Next scene with {0}", this.Name),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = vm.VirtualWidth,
                Height = vm.VirtualHeight,
                Margin = new Thickness(10),
                //DrawOrder = -1
            };

            this.Add(button);
            //CurrentScene.EntityManager.Add(this);
        }

        private void SetUpGrid()
        {
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.Width = vm.VirtualWidth;
            this.Height = vm.VirtualHeight;
            //this.DrawOrder = -1;
            this.IsBorder = true;
            this.IsVisible = true;
            this.Margin = new Thickness(10);
        }
        public void Show() { this.IsVisible = true; }
        public void Hide() { this.IsVisible = false; }
    }
}
