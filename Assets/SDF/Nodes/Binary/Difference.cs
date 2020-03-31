using System;

namespace SDF {

    [Serializable]
    public class Difference : SDFNodeBinary<Difference.Op> {

        public override bool IsCommutative => true;

        protected override Op GetOp() {
            return default;
        }

        public struct Op : IBinaryOp {
            public float Combine(float left, float right) {
                return Difference.Combine(left, right);
            }
        }

        public static float Combine(float left, float right) {
            return Intersection.Combine(left, -right);
        }
    }
}
