using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Torus : SDFNodeShape<Torus.Op> {

        public float4x4 ToLocalSpace;
        public float SmallRadius;
        public float LargeRadius;

        protected override Op GetOp() {
            return new Op() {
                ToLocalSpace = ToLocalSpace,
                SmallRadius = SmallRadius,
                LargeRadius = LargeRadius
            };
        }

        public struct Op : IShapeOp {
            public float4x4 ToLocalSpace;
            public float SmallRadius;
            public float LargeRadius;

            public float Sample(float3 position) {
                return Torus.Sample(position, ToLocalSpace, SmallRadius, LargeRadius);
            }
        }

        public static float Sample(float3 position, float4x4 toLocalSpace, float smallRadius, float largeRadius) {
            return Sample(mul(toLocalSpace, position), smallRadius, largeRadius);
        }

        public static float Sample(float3 position, float smallRadius, float largeRadius) {
            return length(float2(length(position.xz) - largeRadius, position.y)) - smallRadius;
        }
    }
}
