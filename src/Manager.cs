using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zene.Structs;

namespace PHYSICS
{
    public class Manager
    {
        public Manager(Bounds b, floatv e)
        {
            _walls.Add(new Wall(b.TopLeft, b.BottomLeft, e));
            _walls.Add(new Wall(b.TopRight, b.BottomRight, e));
            _walls.Add(new Wall(b.TopLeft, b.TopRight, e));
            _walls.Add(new Wall(b.BottomLeft, b.BottomRight, e));
        }
        
        private List<Wrap<Ball>> _balls = new List<Wrap<Ball>>(20);
        private List<Path> _paths = new List<Path>(20);
        private List<Wall> _walls = new List<Wall>(8);
        
        public floatv Elapsed { get; private set; }
        public List<Wall> Walls => _walls;
        
        public void AddBall(Ball b)
        {
            _balls.Add(new Wrap<Ball>(b));
            _paths.Add(new Path(b.Location, b.Velocity));
        }
        public void AddWall(Wall w) => _walls.Add(w);
        
        public int Fill(List<ObjectRender> or)
        {
            int t = 0;
            Span<Wrap<Ball>> span1 = CollectionsMarshal.AsSpan(_balls);
            Span<Path> span2 = CollectionsMarshal.AsSpan(_paths);
            for (int i = 0; i < span1.Length; i++)
            {
                Path p = span2[i];
                or.Add(new ObjectRender(span1[i].Obj, p));
                t += p.References.Count - 1;
            }
            return t;
        }
        
        public void ElapseTo(floatv time)
        {
            if (time < Elapsed) { return; }
            Elapsed = time;
            
            RefObj e1 = new RefObj();
            RefObj e2 = new RefObj();
            floatv minT = floatv.MaxValue;
            
            // Find all init collisions
            Span<Wrap<Ball>> span = CollectionsMarshal.AsSpan(_balls);
            Span<Wall> walls = CollectionsMarshal.AsSpan(_walls);
            for (int i = 0; i < span.Length; i++)
            {
                Wrap<Ball> o1 = span[i];
                RefObj ro = new RefObj(i, ObjType.Ball);
                
                FindAllCol(o1, i, span, walls, time, ro);
                if (o1.ColTime < minT)
                {
                    minT = o1.ColTime;
                    e1 = ro;
                    e2 = o1.Collide;
                }
                
                // span[i] = o1;
            }
            
            while (true)
            {
                // No collisions in time
                if (e1.IsNone()) { return; }
                
                if (!e2.IsWall())
                {
                    // Manage earliest
                    Wrap<Ball> c1 = span[e1.Index];
                    Wrap<Ball> c2 = span[e2.Index];
                    floatv et = c1.ColTime;
                    
                    // last collide can now collide again - moved to new pos
                    if (c1.LastCollide.IsObject())
                    {
                        span[c1.LastCollide.Index].LastCollide = new RefObj();
                    }
                    if (c2.LastCollide.IsObject())
                    {
                        span[c2.LastCollide.Index].LastCollide = new RefObj();
                    }
                    
                    // update pos
                    c1.Obj.Location = Resolve.Linear(c1.Obj, et - c1.ElapsedTime);
                    c2.Obj.Location = Resolve.Linear(c2.Obj, et - c2.ElapsedTime);
                    c1.ElapsedTime = et;
                    c2.ElapsedTime = et;
                    (c1.Obj.Velocity, c2.Obj.Velocity) = Resolve.Find(c1.Obj, c2.Obj);
                    _paths[e1.Index].Add(c1);
                    _paths[e2.Index].Add(c2);
                    // reset for next detections
                    c1.LastCollide = e2;
                    c2.LastCollide = e1;
                    c1.ColTime = floatv.MaxValue;
                    c2.ColTime = floatv.MaxValue;
                    c1.Collide = new RefObj();
                    c2.Collide = new RefObj();
                    // span[e1.Index] = c1;
                    // span[e2.Index] = c2;
                }
                else
                {
                    // Resolve wall collision
                    Wrap<Ball> c1 = span[e1.Index];
                    Wall w = walls[e2.Index];
                    floatv et = c1.ColTime;
                    
                    // last collide can now collide again - moved to new pos
                    if (c1.LastCollide.IsObject())
                    {
                        span[c1.LastCollide.Index].LastCollide = new RefObj();
                    }
                    
                    // update pos
                    c1.Obj.Location = Resolve.Linear(c1.Obj, et - c1.ElapsedTime);
                    c1.ElapsedTime = et;
                    c1.Obj.Velocity = Resolve.Find(c1.Obj, w);
                    _paths[e1.Index].Add(c1);
                    // reset for next detections
                    c1.LastCollide = e2;
                    c1.ColTime = floatv.MaxValue;
                    c1.Collide = new RefObj();
                    // span[e1.Index] = c1;
                }
                
                RefObj change1 = e1;
                RefObj change2 = e2;
                e1 = new RefObj();
                e2 = new RefObj();
                minT = floatv.MaxValue;
                
                // Update collisions with the changes to the 2 objects
                // Find missing collisions (ones who lost their earliest collide)
                for (int i = 0; i < span.Length; i++)
                {
                    Wrap<Ball> o1 = span[i];
                    RefObj ro = new RefObj(i, ObjType.Ball);
                    
                    // all potential checks covered here
                    // with velocity of 0, is has no path
                    if ((o1.CollideTaken && o1.Obj.Velocity != 0) ||
                        i == change1.Index ||
                        (!change2.IsWall() && i == change2.Index))
                    {
                        FindAllCol(o1, -1, span, walls, time, ro);
                    }
                    o1.CollideTaken = false;
                    
                    // check all availiable collisions for earliest
                    if (o1.ColTime < minT)
                    {
                        minT = o1.ColTime;
                        e1 = ro;
                        e2 = o1.Collide;
                    }
                    
                    // span[i] = o1;
                }
                
                // remove old last collides
                // span[change1.Index].LastCollide = new RefObj();
                // span[change2.Index].LastCollide = new RefObj();
            }
        }
        
        private static floatv FindCol(
            Wrap<Ball> o1, Wrap<Ball> o2, floatv time,
            Span<Wrap<Ball>> span, RefObj r1, RefObj r2)
        {
            if (o1.LastCollide == r2 ||
            // could be done earlier?
                (o1.Obj.Velocity == 0 && o2.Obj.Velocity == 0)) { return -1; }
            
            // Too far out of sync
            if (o2.ColTime < o1.ElapsedTime || o1.ColTime < o2.ElapsedTime) { return -1; }
            floatv t = Collisions.BallBallLinearOffset(o1.Obj, o2.Obj, o1.ElapsedTime, o2.ElapsedTime);
            if (t < 0 || o2.ColTime < t || o1.ColTime < t ||
                time < t || t < o1.ElapsedTime || t < o2.ElapsedTime) { return -1; }
            
            // Remove old collide
            if (o2.Collide.IsObject())
            {
                int i = o2.Collide.Index;
                span[i].Collide = RefObj.None;
                span[i].ColTime = floatv.MaxValue;
                span[i].CollideTaken = true;
            }
            if (o1.Collide.IsObject())
            {
                int i = o1.Collide.Index;
                span[i].Collide = RefObj.None;
                span[i].ColTime = floatv.MaxValue;
                span[i].CollideTaken = true;
            }
            o2.Collide = r1;
            o2.ColTime = t;
            o1.Collide = r2;
            o1.ColTime = t;
            o1.CollideTaken = false;
            o2.CollideTaken = false;
            
            return t;
        }
        private static floatv FindWallCol(
            Wrap<Ball> o1, Span<Wrap<Ball>> span, floatv time,
            Wall w, RefObj rw)
        {
            if (o1.LastCollide == rw) { return -1; }
            
            floatv t = Collisions.BallWallLinearOffset(o1.Obj, w, o1.ElapsedTime);
            if (t < 0 || o1.ColTime < t ||
                time < t || t < o1.ElapsedTime) { return -1; }
            
            if (o1.Collide.IsObject())
            {
                int i = o1.Collide.Index;
                span[i].Collide = RefObj.None;
                span[i].ColTime = floatv.MaxValue;
                span[i].CollideTaken = true;
            }
            
            o1.ColTime = t;
            o1.Collide = rw;
            return t;
        }
        private static void FindAllCol(
            Wrap<Ball> o1, int i, Span<Wrap<Ball>> span,
            Span<Wall> walls, floatv time, RefObj ro)
        {
            for (int j = 0; j < walls.Length; j++)
            {
                RefObj rw = new RefObj(j, ObjType.Wall);
                FindWallCol(o1, span, time, walls[j], rw);
            }
            
            for (int j = i + 1; j < span.Length; j++)
            {
                Wrap<Ball> o2 = span[j];
                
                RefObj r2 = new RefObj(j, ObjType.Ball);
                FindCol(o1, o2, time, span, ro, r2);
                
                // span[j] = o2;
            }
        }
    }
}