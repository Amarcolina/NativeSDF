using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Cone : SDFNodeShape<Cone.Op> {

        public float4x4 ToLocalSpace;
        public float Radius;
        public float Height;

        protected override Op GetOp() {
            return new Op() {
                ToLocalSpace = ToLocalSpace,
                Radius = Radius,
                Height = Height
            };
        }

        public struct Op : IShapeOp {
            public float4x4 ToLocalSpace;
            public float Radius;
            public float Height;

            public float Sample(float3 position) {
                return Cone.Sample(position, ToLocalSpace, Radius, Height);
            }
        }

        public static float Sample(float3 position, float4x4 toLocalSpace, float radius, float height) {
            return Sample(mul(toLocalSpace, position), radius, height);
        }

        public static float Sample(float3 position, float radius, float height) {
            float2 q = float2(length(position.xz), position.y);
            float2 tip = q - float2(0, height);
            float2 mantleDir = normalize(float2(height, radius));
            float mantle = dot(tip, mantleDir);
            float d = max(mantle, -q.y);
            float projected = dot(tip, float2(mantleDir.y, -mantleDir.x));

            // distance to tip
            if ((q.y > height) && (projected < 0)) {
                d = max(d, length(tip));
            }

            // distance to base ring
            if ((q.x > radius) && (projected > length(float2(height, radius)))) {
                d = max(d, length(q - float2(radius, 0)));
            }
            return d;
        }
    }
}
