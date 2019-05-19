using System;
using static Unity.Mathematics.math;

namespace SDF {

  [Serializable]
  public class UnionSmooth : SDFNodeBinary<UnionSmooth.Op> {

    public float K;
    public override bool IsCommutative => true;

    public UnionSmooth() : this(0.5f) { }
    public UnionSmooth(float k) {
      K = k;
    }

    protected override Op GetOp() {
      return new Op() { K = K };
    }

    public struct Op : IBinaryOp {
      public float K;

      public float Combine(float left, float right) {
        float h = saturate(0.5f + 0.5f * (left - right) / K);
        return lerp(left, right, h) - K * h * (1.0f - h);
      }
    }
  }
}
