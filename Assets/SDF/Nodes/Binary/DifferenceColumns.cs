using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class DifferenceColumns : SDFNodeBinary<DifferenceColumns.Op>, IColumnsOp {

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
                return DifferenceColumns.Combine(left, right, Radius, N);
            }
        }

        public static float Combine(float left, float right, float radius, int n) {
            left = -left;
            float m = min(left, right);
            //avoid the expensive computation where not needed (produces discontinuity though)
            if ((left < radius) && (right < radius)) {
                float2 p = float2(left, right);
                float columnradius = radius * sqrt(2) / n / 2.0f;
                columnradius = radius * sqrt(2) / ((n - 1) * 2 + sqrt(2));

                Rotate45.Modify(ref p);
                p.y += columnradius;
                p.x -= sqrt(2) / 2 * radius;
                p.x += -columnradius * sqrt(2) / 2;

                if (mod(n, 2) == 1) {
                    p.y += columnradius;
                }
                ModSimple.Mod1(ref p.y, columnradius * 2);

                float result = -length(p) + columnradius;
                result = max(result, p.x);
                result = min(result, left);
                return -min(result, right);
            } else {
                return -m;
            }
        }
    }
}
