using System;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class DifferenceRound : SDFNodeBinary<DifferenceRound.Op>, IRoundOp {

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
                return DifferenceRound.Combine(left, right, Radius);
            }
        }

        public static float Combine(float left, float right, float radius) {
            return IntersectionRound.Combine(left, -right, radius);
        }
    }
}