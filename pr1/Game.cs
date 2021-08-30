using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace pr1
{
    public class Window : GameWindow
    {
        private CGOLScene scene;
        public static bool Windoed;
        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        protected override void OnLoad(EventArgs e)
        {
            scene = new CGOLScene(this);
            GL.ClearColor(0.1f, 0.12f, 0.12f, 1.0f);
            scene.Load();
            base.OnLoad(e);
            ;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            scene.Render();

            SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var mouse = Mouse.GetCursorState();
            var kbrd = Keyboard.GetState();

            if (kbrd.IsKeyDown(Key.F11))
            {
                if (this.WindowBorder != WindowBorder.Hidden)
                {
                    this.WindowBorder = WindowBorder.Hidden;
                    this.WindowState = WindowState.Fullscreen;
                    Windoed = false;
                }
                else
                {
                    this.WindowBorder = WindowBorder.Resizable;
                    this.WindowState = WindowState.Normal;
                    Windoed = true;
                }
            }
            if (kbrd.IsKeyDown(Key.Escape))
            {
                this.WindowBorder = WindowBorder.Resizable;
                this.WindowState = WindowState.Normal;
            }

            scene.Update(this, mouse, kbrd);
            base.OnUpdateFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            scene.Unload();
            base.OnUnload(e);
        }
    }
}