using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.Structs;

namespace PHYSICS
{
    public enum ObjType
    {
        None = 0,
        Ball
    }
    
    public class Manager
    {
        private List<Wrap<Ball>> _balls = new List<Wrap<Ball>>(20);
        private List<Path> _paths = new List<Path>(20);
        
        public void AddBall(Ball b)
        {
            _balls.Add(new Wrap<Ball>(b));
            _paths.Add(new Path(b.Location, b.Velocity));
        }
        
        public void ElapseTo(floatv time)
        {
            RefObj e1 = new RefObj();
            RefObj e2 = new RefObj();
            
            // Find all init collisions
            Span<Wrap<Ball>> span = CollectionsMarshal.AsSpan(_balls);
            for (int i = 0; i < span.Length; i++)
            {
                Wrap<Ball> o1 = span[i];
                RefObj ro = new RefObj(i, ObjType.Ball);
                
                for (int j = i + 1; j < span.Length; j++)
                {
                    Wrap<Ball> o2 = span[j];
                    
                    RefObj r2 = new RefObj(j, ObjType.Ball);
                    floatv t = FindCol(o1, o2, time, span, ro, r2);
                        
                    if (t < span[e1.Index].ColTime)
                    {
                        e1 = ro;
                        e2 = r2;
                    }
                    
                    // span[j] = o2;
                }
                
                // span[i] = o1;
            }
            
            while (true)
            {
                // No collisions in time
                if (e1.IsNone()) { return; }
                
                // Manage earliest
                Wrap<Ball> c1 = span[e1.Index];
                Wrap<Ball> c2 = span[e2.Index];
                floatv et = c1.ColTime;
                c1.ElapsedTime = et;
                c2.ElapsedTime = et;
                
                // update pos
                c1.Obj.Location = Resolve.Linear(c1.Obj, et);
                c2.Obj.Location = Resolve.Linear(c2.Obj, et);
                (c1.Obj.Velocity, c2.Obj.Velocity) = Resolve.Find(c1.Obj, c2.Obj);
                _paths[e1.Index].Add(c1);
                _paths[e2.Index].Add(c2);
                // span[e1.Index] = c1;
                // span[e2.Index] = c2;
                
                RefObj change1 = e1;
                RefObj change2 = e2;
                
                // Update collisions with the changes to the 2 objects
                // Find missing collisions (ones who lost their earilest collide)
                for (int i = 0; i < span.Length; i++)
                {
                    Wrap<Ball> o1 = span[i];
                    RefObj ro = new RefObj(i, ObjType.Ball);
                    
                    if (o1.CollideTaken || i == change1.Index || i == change2.Index)
                    {
                        o1.CollideTaken = false;
                        for (int j = i + 1; j < span.Length; j++)
                        {
                            Wrap<Ball> o2 = span[j];
                            
                            RefObj r2 = new RefObj(j, ObjType.Ball);
                            floatv t = FindCol(o1, o2, time, span, ro, r2);
                            
                            if (t < span[e1.Index].ColTime)
                            {
                                e1 = ro;
                                e2 = r2;
                            }
                            
                            // span[j] = o2;
                        }
                    }
                    else
                    {
                        Wrap<Ball> o2 = span[change1.Index];
                        floatv t = FindCol(o1, o2, time, span, ro, change1);
                        
                        if (t < span[e1.Index].ColTime)
                        {
                            e1 = ro;
                            e2 = change1;
                        }
                        
                        Wrap<Ball> o3 = span[change2.Index];
                        t = FindCol(o1, o2, time, span, ro, change2);
                        
                        if (t < span[e1.Index].ColTime)
                        {
                            e1 = ro;
                            e2 = change2;
                        }
                    }
                    
                    // span[i] = o1;
                }
            }
            
            // repeat
        }
        
        private static floatv FindCol(
            Wrap<Ball> o1, Wrap<Ball> o2, floatv time,
            Span<Wrap<Ball>> span, RefObj r1, RefObj r2)
        {
            floatv t = Collisions.BallBallLinearOffset(o1.Obj, o2.Obj, o1.ElapsedTime, o2.ElapsedTime);
            if (t < 0d || o2.ColTime < t || o1.ColTime < t || time < t) { return floatv.MaxValue; }
            
            o2.ColTime = t;
            // Remove old collide
            if (!o2.Collide.IsNone())
            {
                int i = o2.Collide.Index;
                span[i].Collide = RefObj.None;
                span[i].ColTime = floatv.MaxValue;
                span[i].CollideTaken = true;
            }
            o2.Collide = r1;
            o1.ColTime = t;
            o1.Collide = r2;
            o1.CollideTaken = false;
            o2.CollideTaken = false;
            
            return t;
        }
    }
    
    public class Wrap<T> where T : IObject
    {
        public Wrap(T t) { Obj = t; ColTime = floatv.MaxValue; }
        
        public T Obj;
        public RefObj Collide;
        public floatv ColTime;
        public floatv ElapsedTime;
        public bool CollideTaken;
    }
    public struct RefObj
    {
        public RefObj(int i, ObjType t)
        {
            Index = i;
            Type = t;
        }
        
        public int Index;
        public ObjType Type;
        
        public bool IsNone() => Type == ObjType.None;
        public static RefObj None { get; } = new RefObj(0, ObjType.None);
    }
    
    public struct Path
    {
        public Path(Vector2 l, Vector2 v)
        {
            References.Add(new Point(l, v, 0));
        }
        public List<Point> References = new List<Point>();
        public void Add<T>(Wrap<T> w) where T : IObject
        {
            References.Add(new Point(w.Obj.Location, w.Obj.Velocity, w.ElapsedTime));
        }
    }
    public struct Point
    {
        public Point(Vector2 l, Vector2 v, floatv t)
        {
            Location = l;
            Veclocity = v;
            Time = t;
        }
        public Vector2 Location;
        public Vector2 Veclocity;
        public floatv Time;
    }
}