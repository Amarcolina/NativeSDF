using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class UnionRound : SDFNodeBinary<UnionRound.Op>, IRoundOp {

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
                return UnionRound.Combine(left, right, Radius);
            }
        }

        public static float Combine(float left, float right, float radius) {
            float2 u = max(new float2(radius - left, radius - right), new float2(0));
            return max(radius, min(left, right)) - length(u);
        }
    }
}
