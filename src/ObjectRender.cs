using System;
using System.Runtime.InteropServices;
using Zene.Graphics;
using Zene.Structs;

namespace PHYSICS
{
    public class ObjectRender : IRenderable
    {
        public ObjectRender(Ball b, Path p)
        {
            _ball = b;
            _path = p;
            _time = 0;
            _current = 0;
        }
        
        private Ball _ball;
        private Path _path;
        private floatv _time;
        private int _current;
        
        public void SetTime(floatv time)
        {
            int start = 0;
            if (time > _time)
            {
                start = _current;
            }
            
            Span<Point> span = CollectionsMarshal.AsSpan(_path.References);
            int i;
            for (i = start; i < span.Length; i++)
            {
                if (span[i].Time > time) { break; }
            }
            _current = i - 1;
            _time = time;
        }
        
        public void OnRender(IDrawingContext dc)
        {
            Point p = _path.References[_current];
            Vector2 pos = Resolve.Linear(p, _time);
            dc.DrawCircle(pos, 2 * _ball.Radius, ColourF.Khaki);
        }
    }
}