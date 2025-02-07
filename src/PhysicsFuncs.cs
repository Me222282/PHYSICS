using System;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Zene.Structs;

namespace PHYSICS
{
    public static class Collisions
    {
        private static floatv MinMag(floatv a, floatv b)
        {
            if (a < 0) { return b; }
            if (b < 0) { return a; }
            if (a < b) { return a; }
            return b;
        }
        private static floatv MinMag(floatv a, floatv b, floatv tp)
        {
            if (a < tp) { return b; }
            if (b < tp) { return a; }
            if (a < b) { return a; }
            return b;
        }
        
        public static floatv BallBallLinear(Ball a, Ball b)
        {
            Vector2 velDiff = a.Velocity - b.Velocity;
            Vector2 posDiff = a.Location - b.Location;
            floatv gap = a.Radius + b.Radius;
            
            if (velDiff == 0) { return -1; }
            floatv A = velDiff.SquaredLength;
            floatv B = velDiff.Dot(posDiff);
            floatv C = posDiff.SquaredLength - (gap * gap);
            
            floatv discriminant = (B * B) - (A * C);
            if (discriminant < 0f) { return -1; }
            discriminant = Maths.Sqrt(discriminant);
            
            floatv div = 1 / A;
            floatv t1 = (-B + discriminant) * div;
            floatv t2 = (-B - discriminant) * div;
            
            return MinMag(t1, t2);
        }
        public static floatv BallBallLinearOffset(Ball a, Ball b, floatv t1, floatv t2)
        {
            if (t1 < t2)
            {
                return BallBallLinearOffset(b, a, t2, t1);
            }
            
            floatv tOff = t1 - t2;
            
            b.Location = Resolve.Linear(b, tOff);
            return BallBallLinear(a, b) + t1;
        }
        
        public static floatv BallWallLinear(Ball a, Wall w)
        {
            Vector2 n = (w.A - w.B).Rotated90();
            
            floatv div = 1 / a.Velocity.Dot(n);
            floatv nl = n.Length;
            floatv q = (w.A - a.Location).Dot(n);
            floatv rnl = a.Radius * nl;
            floatv t1 = (rnl + q) * div;
            floatv t2 = (q - rnl) * div;
            
            n *= a.Radius / nl;
            // Outside wall
            Vector2 p1 = Resolve.Linear(a, t1) - n;
            Vector2 p2 = Resolve.Linear(a, t2) + n;
            
            floatv div2;
            if (w.A.X - w.B.X == 0)
            {
                div2 = 1 / (w.A.Y - w.B.Y);
                
                floatv u1 = (p1.Y - w.B.Y) * div2;
                if (u1 < 0 || u1 > 1) { t1 = -1; }
                
                floatv u2 = (p2.Y - w.B.Y) * div2;
                if (u2 < 0 || u2 > 1) { t2 = -1; }
            }
            else
            {
                div2 = 1 / (w.A.X - w.B.X);
                
                floatv u1 = (p1.X - w.B.X) * div2;
                if (u1 < 0 || u1 > 1) { t1 = -1; }
                
                floatv u2 = (p2.X - w.B.X) * div2;
                if (u2 < 0 || u2 > 1) { t2 = -1; }
            }
            
            return MinMag(t1, t2);
        }
        public static floatv BallWallLinearOffset(Ball a, Wall w, floatv t)
            => BallWallLinear(a, w) + t;
        
        // public static floatv BallBallLinearDrag(Ball a, Ball b, floatv ka, floatv kb)
        // {
            
            
        //     Vector2 zDiff = a.Velocity - b.Velocity;
        //     Vector2 posDiff = a.Location - b.Location;
        //     floatv gap = a.Radius + b.Radius;
            
        //     floatv A = zDiff.SquaredLength;
        //     floatv B = zDiff.Dot(posDiff);
        //     floatv C = posDiff.SquaredLength - (gap * gap);
            
        //     floatv discriminant = (B * B) - (A * C);
        //     discriminant = Maths.Sqrt(discriminant);
            
        //     floatv div = 1 / A;
        //     floatv z1 = (-B + discriminant) * div;
        //     floatv z2 = (-B - discriminant) * div;
            
        //     return MinMag(t1, t2);
        // }
        // private static (Vector2, Vector2) GetDragCP(Vector2 vel)
        // {
            
        // }
    }
    
    public static class Resolve
    {
        public static (Vector2, Vector2) Find<A, B>(A a, B b)
            where A : IObject
            where B : IObject
        {
            floatv e = a.Elasticity * b.Elasticity;
            Vector2 axis = a.Location - b.Location;
            // axis = axis.Normalised();
            Vector2 perp = axis.Rotated90();
            
            floatv pd = 1 / axis.PerpDot(perp);
            Vector2 ua = Align(axis, perp, a.Velocity, pd);
            Vector2 ub = Align(axis, perp, b.Velocity, pd);
            
            floatv p = a.Mass * ua.X + b.Mass * ub.X;
            floatv div = 1 / (a.Mass + b.Mass);
            floatv eu = e * (ua.X - ub.X);
            floatv va = (p - (b.Mass * eu)) * div;
            floatv vb = (p + (a.Mass * eu)) * div;
            
            // WARNING
            // don't know why its negative
            Vector2 resultA = (axis * va) - (perp * ua.Y);
            Vector2 resultB = (axis * vb) - (perp * ub.Y);
            return (resultA, resultB);
        }
        private static Vector2 Align(Vector2 i, Vector2 j, Vector2 v, floatv pd)
        {
            Vector2 r = new Vector2();
            // divided by i.PerpDot(j) - is 1 for normalised vectors
            r.Y = ((v.X * i.Y) - (v.Y * i.X)) * pd;
            // r.X = (v.X - (r.Y * j.X)) / i.X;
            // divided by i.PerpDot(j) - is 1 for normalised vectors
            r.X = ((v.X * j.Y) - (v.Y * j.X)) * pd;
            return r;
        }
        public static Vector2 Linear(Ball b, floatv t) => b.Location + (t * b.Velocity);
        public static Vector2 Linear(Point p, floatv t)
            => p.Location + ((t - p.Time) * p.Velocity);
        
        public static Vector2 Find(Ball a, Wall wall)
        {
            floatv e = a.Elasticity * wall.Elasticity;
            // axis = axis.Normalised();
            Vector2 perp = wall.A - wall.B;
            Vector2 axis = perp.Rotated90();
            
            floatv pd = 1 / axis.PerpDot(perp);
            Vector2 u = Align(axis, perp, a.Velocity, pd);
            
            floatv v = u.X * e;
            return -((axis * v) + (perp * u.Y));
        }
    }
}