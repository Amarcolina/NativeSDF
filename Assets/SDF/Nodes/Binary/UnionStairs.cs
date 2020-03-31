using System;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class UnionStairs : SDFNodeBinary<UnionStairs.Op>, IStairsOp {

        public float Radius { get; set; }
        public int N { get; set; }

        public override bool IsCommutative => false;

        protected override Op GetOp() {
            return new Op() {
                Radius = Radius,
                N = N
            };
        }

        public struct Op : IBinaryOp {
            public float Radius;
            public int N;

            public float Combine(float left, float right) {
                return UnionStairs.Combine(left, right, Radius, N);
            }
        }

        public static float Combine(float left, float right, float radius, int n) {
            float s = radius / n;
            float u = right - radius;
            return min(min(left, right), 0.5f * (u + left + abs((mod(u - left + s, 2 * s)) - s)));
        }
    }
}
