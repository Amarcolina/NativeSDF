using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Plane : SDFNodeShape<Plane.Op> {

        public float3 Normal;
        public float DistFromOrigin;

        protected override Op GetOp() {
            return new Op() {
                Normal = Normal,
                DistFromOrigin = DistFromOrigin
            };
        }

        public struct Op : IShapeOp {
            public float3 Normal;
            public float DistFromOrigin;

            public float Sample(float3 position) {
                return Plane.Sample(position, Normal, DistFromOrigin);
            }
        }

        public static float Sample(float3 position, float3 normal, float distFromOrigin) {
            return dot(position, normal) + distFromOrigin;
        }
    }
}
