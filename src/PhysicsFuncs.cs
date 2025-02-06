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
        
        public static floatv BallBoundsLinear(Ball a, Bounds b, floatv et, out bool side)
        {
            b = b.Expanded(-a.Radius);
            
            Vector2 div = 1 / a.Velocity;
            floatv t1 = (b.Left - a.Location.X) * div.X;
            floatv t2 = (b.Right - a.Location.X) * div.X;
            floatv t3 = (b.Top - a.Location.Y) * div.Y;
            floatv t4 = (b.Bottom - a.Location.Y) * div.Y;
            
            floatv t = MinMag(MinMag(t1, t2, et), MinMag(t3, t4, et), et);
            side = t == t3 || t == t4;
            
            if (t < et) { return -1; }
            return t;
        }
        
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
        
        public static Vector2 Find(Ball a, Vector2 wall, floatv we)
        {
            floatv e = a.Elasticity * we;
            Vector2 axis = wall.Rotated90();
            // axis = axis.Normalised();
            Vector2 perp = wall;
            
            floatv pd = 1 / axis.PerpDot(perp);
            Vector2 u = Align(axis, perp, a.Velocity, pd);
            
            floatv v = u.X * e;
            return (axis * v) + (perp * u.Y);
        }
    }
}