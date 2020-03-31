using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class IntersectionRound : SDFNodeBinary<IntersectionRound.Op>, IRoundOp {

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
                return IntersectionRound.Combine(left, right, Radius);
            }
        }

        public static float Combine(float left, float right, float radius) {
            float2 u = max(float2(radius + left, radius + right), float2(0));
            return min(-radius, max(left, right)) + length(u);
        }
    }
}