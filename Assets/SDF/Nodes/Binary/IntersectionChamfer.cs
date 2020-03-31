using System;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class IntersectionChamfer : SDFNodeBinary<IntersectionChamfer.Op>, IChamferOp {

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
                return IntersectionChamfer.Combine(left, right, Radius);
            }
        }

        public static float Combine(float left, float right, float radius) {
            return max(max(left, right), (left + radius + right) * sqrt(0.5f));
        }
    }
}
