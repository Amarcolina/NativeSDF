using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Cylinder : SDFNodeShape<Cylinder.Op> {

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
                return Cylinder.Sample(position, ToLocalSpace, Radius, Height);
            }
        }

        public static float Sample(float3 position, float4x4 toLocalSpace, float radius, float height) {
            return Sample(mul(toLocalSpace, position), radius, height);
        }

        public static float Sample(float3 position, float radius, float height) {
            float d = length(position.xz) - radius;
            d = max(d, abs(position.y) - height);
            return d;
        }
    }
}