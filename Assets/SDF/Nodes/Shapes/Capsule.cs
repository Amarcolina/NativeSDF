using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Capsule : SDFNodeShape<Capsule.Op> {

        public float3 PointA, PointB;
        public float Radius;

        protected override Op GetOp() {
            return new Op() {
                PointA = PointA,
                PointB = PointB,
                Radius = Radius
            };
        }

        public struct Op : IShapeOp {
            public float3 PointA, PointB;
            public float Radius;

            public float Sample(float3 position) {
                return Capsule.Sample(position, PointA, PointB, Radius);
            }
        }

        public static float Sample(float3 position, float3 pointA, float3 pointB, float radius) {
            float3 ab = pointB - pointA;
            float t = saturate(dot(position - pointA, ab) / dot(ab, ab));
            return length((ab * t + pointA) - position) - radius;
        }
    }
}
