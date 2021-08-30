using System;
using OpenTK.Input;

namespace pr1
{
    public class Button
    {
        private readonly Window base_w;
        private readonly Plane plane;
        private bool pressed;
        private readonly Func<bool> reaction;
        private readonly Shader shader;
        private readonly Texture texture;

        public Button(string tex_path, Func<bool> func, Window base_wnd)
        {
            base_w = base_wnd;
            reaction = func;
            texture = Texture.LoadFromFile(tex_path);
            shader = new Shader("shaders/ui/button.vert", "shaders/ui/button.frag");
            plane = new Plane(Misc.fullscreenverticies, shader, texture);
        }

        public void ReshapeWithCoords(float top_x, float top_y, float bottom_x, float bottom_y)
        {
            plane.ReshapeWithCoords(top_x, top_y, bottom_x, bottom_y);
        }

        public void Load()
        {
            plane.Load();
        }

        public void OnPress()
        {
            reaction();
        }

        public void Render()
        {
            plane.Render();
        }

        public void Unload()
        {
            plane.Unload();
        }

        public void Update(MouseState mouse)
        {
            var info = plane.GetRelativeCursorPosition(base_w, mouse.X, mouse.Y);
            if (info[0] <= 1.0 && info[0] >= 0.0 && info[1] <= 1.0 && info[1] >= 0.0)
            {
                shader.SetFloat("mouse_hover", 1.0f);
                if (mouse.IsButtonDown(MouseButton.Left) && pressed == false)
                {
                    OnPress();
                    pressed = true;
                }
            }
            else
            {
                shader.SetFloat("mouse_hover", 0.0f);
            }

            if (mouse.IsButtonUp(MouseButton.Left)) pressed = false;
        }
    }
}