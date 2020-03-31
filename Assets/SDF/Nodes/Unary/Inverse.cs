using System;

namespace SDF {

    [Serializable]
    public class Inverse : SDFNodeUnary<Inverse.Op> {

        protected override Op GetOp() {
            return default;
        }

        public struct Op : IUnaryOp {
            public void Modify(ref float dist) {
                dist = -dist;
            }
        }
    }
}
