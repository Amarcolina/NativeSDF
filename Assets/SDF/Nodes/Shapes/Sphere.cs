using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Sphere : SDFNodeShape<Sphere.Op> {

        public float3 Center;
        public float Radius;

        protected override Op GetOp() {
            return new Op() {
                Center = Center,
                Radius = Radius
            };
        }

        public struct Op : IShapeOp {
            public float3 Center;
            public float Radius;

            public float Sample(float3 position) {
                float3 toCenter = Center - position;
                return length(toCenter) - Radius;
            }
        }
    }
}
