using System;
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
        
        public static floatv BallBallLinear(Ball a, Ball b)
        {
            Vector2 velDiff = a.Velocity - b.Velocity;
            Vector2 posDiff = a.Location - b.Location;
            floatv gap = a.Radius + b.Radius;
            
            floatv A = velDiff.SquaredLength;
            floatv B = velDiff.Dot(posDiff);
            floatv C = posDiff.SquaredLength - (gap * gap);
            
            floatv discriminant = (B * B) - (A * C);
            discriminant = Maths.Sqrt(discriminant);
            
            floatv div = 1 / A;
            floatv t1 = (-B + discriminant) * div;
            floatv t2 = (-B - discriminant) * div;
            
            return MinMag(t1, t2);
        }
        public static Vector2 BallBallLinearOffset(Ball a, Ball b, floatv t1, floatv t2)
        {
            if (t1 < t2)
            {
                return BallBallLinearOffset(b, a, t2, t1);
            }
            
            floatv tOff = t1 - t2;
            
            b.Location = Linear(b, tOff);
            floatv t = BallBallLinear(a, b);
            return (t, t + tOff);
        }
        public static Vector2 Linear(Ball b, floatv t) => b.Location + (t * b.Velocity);
        
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
}