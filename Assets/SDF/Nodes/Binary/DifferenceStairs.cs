using System;

namespace SDF {

    [Serializable]
    public class DifferenceStairs : SDFNodeBinary<DifferenceStairs.Op>, IStairsOp {

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
                return DifferenceStairs.Combine(left, right, Radius, N);
            }
        }

        public static float Combine(float left, float right, float radius, int n) {
            return -UnionStairs.Combine(-left, right, radius, n);
        }
    }
}
