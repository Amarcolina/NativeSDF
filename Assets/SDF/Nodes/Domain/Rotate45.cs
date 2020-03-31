using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

    [Serializable]
    public class Rotate45 : SDFNodeDomainSimple<Rotate45.Op> {

        public int Axis;

        protected override Op GetOp() {
            return new Op() {
                Axis = Axis
            };
        }

        public struct Op : IDomainOp {
            public int Axis;

            public void Modify(ref float3 pos) {
                Rotate45.Modify(ref pos, Axis);
            }
        }

        public static void Modify(ref float3 pos, int axis) {
            float3 delta = pos;

            int norm0 = (axis + 1) % 3;
            int norm1 = (axis + 2) % 3;

            delta[norm0] = -pos[norm1];
            delta[norm1] = pos[norm0];

            pos = (pos + delta) * sqrt(0.5f);
        }

        public static void Modify(ref float2 pos) {
            pos = (pos + new float2(pos.y, -pos.x)) * sqrt(0.5f);
        }
    }
}
