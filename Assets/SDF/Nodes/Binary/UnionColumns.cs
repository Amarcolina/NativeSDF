using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class UnionColumns : SDFNodeBinary<UnionColumns.Op>, IColumnsOp {

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
                return UnionColumns.Combine(left, right, Radius, N);
            }
        }

        public static float Combine(float left, float right, float radius, int n) {
            if ((left < radius) && (right < radius)) {
                float2 p = float2(left, right);
                float columnradius = radius * sqrt(2) / ((n - 1) * 2 + sqrt(2));
                Rotate45.Modify(ref p);
                p.x -= sqrt(2) / 2 * radius;
                p.x += columnradius * sqrt(2);
                if (mod(n, 2) == 1) {
                    p.y += columnradius;
                }
                // At this point, we have turned 45 degrees and moved at a point on the
                // diagonal that we want to place the columns on.
                // Now, repeat the domain along this direction and place a circle.
                ModSimple.Mod1(ref p.y, columnradius * 2);
                float result = length(p) - columnradius;
                result = min(result, p.x);
                result = min(result, left);
                return min(result, right);
            } else {
                return min(left, right);
            }
        }
    }
}
