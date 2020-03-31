using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Pipe : SDFNodeBinary<Pipe.Op> {

        public float Radius { get; set; }

        public override bool IsCommutative => false;

        protected override Op GetOp() {
            return new Op() {
                Radius = Radius
            };
        }

        public struct Op : IBinaryOp {
            public float Radius;

            public float Combine(float left, float right) {
                return Pipe.Combine(left, right, Radius);
            }
        }

        public static float Combine(float left, float right, float radius) {
            return length(new float2(left, right)) - radius;
        }
    }
}
