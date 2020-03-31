using System;

namespace SDF {

    [Serializable]
    public class Offset : SDFNodeUnary<Offset.Op> {

        public float Value;

        public Offset() { }
        public Offset(float value) {
            Value = value;
        }

        protected override Op GetOp() {
            return new Op() { Offset = Value };
        }

        public struct Op : IUnaryOp {
            public float Offset;

            public void Modify(ref float dist) {
                dist += Offset;
            }
        }
    }
}
