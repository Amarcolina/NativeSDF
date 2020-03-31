using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class IntersectionColumns : SDFNodeBinary<IntersectionColumns.Op>, IColumnsOp {

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
                return IntersectionColumns.Combine(left, right, Radius, N);
            }
        }

        public static float Combine(float left, float right, float radius, int n) {
            return DifferenceColumns.Combine(left, -right, radius, n);
        }
    }
}
