using static Unity.Mathematics.math;

namespace SDF {

  public class UnionSmooth : SDFNodeBinary<UnionSmooth.Op> {

    public UnionSmooth() : this(0.5f) { }
    public UnionSmooth(float k) : base(new Op() { K = k }) { }

    public override bool IsCommutative => true;

    public struct Op : IBinaryOp {
      public float K;

      public float Combine(float left, float right) {
        float h = saturate(0.5f + 0.5f * (left - right) / K);
        return lerp(left, right, h) - K * h * (1.0f - h);
      }
    }
  }
}
