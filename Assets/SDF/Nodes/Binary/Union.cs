using System;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Union : SDFNodeBinary<Union.Op> {

        public override bool IsCommutative => true;

        protected override Op GetOp() {
            return default;
        }

        public struct Op : IBinaryOp {
            public float Combine(float left, float right) {
                return Union.Combine(left, right);
            }
        }

        public static float Combine(float left, float right) {
            return min(left, right);
        }
    }
}
