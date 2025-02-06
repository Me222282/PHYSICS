#pragma warning disable CS8981
global using floatv =
#if DOUBLE
    System.Double;
#else
	System.Single;
#endif

global using Maths =
#if DOUBLE
    System.Math;
#else
    System.MathF;
#endif
#pragma warning restore CS8981

using System;
using Zene.Windowing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.Graphics;
using Zene.Structs;

namespace PHYSICS
{
    class Program : Window
    {
        static void Main(string[] args)
        {
            Core.Init();
            
            Manager m = new Manager();
            
            m.AddBall(new Ball(0f, (20f, 5f), 0.8f, 10f, 1f));
            m.AddBall(new Ball((30, 0), (-10f, 10f), 0.8f, 10f, 1f));
            m.ElapseTo(100f);
            
            Program p = new Program(800, 500, "WEEEEEEEEE", m);
            p.Run();
            p.Dispose();
            
            // Test t = new Test(800, 500, "sdrtfgyh");
            // t.Run();
            // t.Dispose();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title, Manager m)
            : base(width, height, title)
        {
            m.Fill(_renders);
        }
        
        private floatv _time = 0;
        private bool _playing = false;
        private List<ObjectRender> _renders = new List<ObjectRender>(20);
        
        private void ElapseTime(floatv time)
        {
            time += _time;
            if (time < 0) { return; }
            
            Span<ObjectRender> span = CollectionsMarshal.AsSpan(_renders);
            for (int i = 0; i < span.Length; i++)
            {
                span[i].SetTime(time);
            }
            _time = time;
        }

        protected override void OnUpdate(FrameEventArgs e)
        {
            base.OnUpdate(e);
            
            e.Context.Framebuffer.Clear(BufferBit.Colour);
            
            if (_playing)
            {
                ElapseTime(e.DeltaTime);
            }
            
            Vector2I s = Size;
            e.Context.Projection = Matrix4.CreateOrthographic(s.X, s.Y, 0f, 1f);
            
            Span<ObjectRender> span = CollectionsMarshal.AsSpan(_renders);
            for (int i = 0; i < span.Length; i++)
            {
                e.Context.Render(span[i]);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.Space])
            {
                _playing = !_playing;
                return;
            }
            if (e[Keys.Right])
            {
                ElapseTime(0.01f);
                return;
            }
            if (e[Keys.Left])
            {
                ElapseTime(-0.01f);
                return;
            }
        }
    }
}
