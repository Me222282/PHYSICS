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

namespace PHYSICS
{
    class Program : Window
    {
        static void Main(string[] args)
        {
            Core.Init();
            
            Manager m = new Manager();
            
            
            
            Program p = new Program(800, 500, "WEEEEEEEEE", m);
            p.Run();
            p.Dispose();
            
            Core.Terminate();
        }
        
        public Program(int width, int height, string title, Manager m)
            : base(width, height, title)
        {
            m.Fill(_renders);
        }
        
        private floatv _time = 0;
        private List<ObjectRender> _renders = new List<ObjectRender>(20);
        
        private void SetTime(floatv time)
        {
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
            
            Span<ObjectRender> span = CollectionsMarshal.AsSpan(_renders);
            for (int i = 0; i < span.Length; i++)
            {
                e.Context.Render(span[i]);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e[Keys.Right])
            {
                SetTime(_time + 0.01f);
                return;
            }
            if (e[Keys.Left])
            {
                SetTime(_time - 0.01f);
                return;
            }
        }
    }
}
