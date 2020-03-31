using System;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class UnionChamfer : SDFNodeBinary<UnionChamfer.Op>, IChamferOp {

        public float Radius { get; set; }

        protected override Op GetOp() {
            return new Op() {
                Radius = Radius
            };
        }

        public struct Op : IBinaryOp {
            public float Radius;

            public float Combine(float left, float right) {
                return UnionChamfer.Combine(left, right, Radius);
            }
        }

        public static float Combine(float left, float right, float radius) {
            return min(min(left, right), (left - radius + right) * sqrt(0.5f));
        }
    }
}
