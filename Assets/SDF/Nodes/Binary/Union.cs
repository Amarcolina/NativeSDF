using static Unity.Mathematics.math;

namespace SDF {

  public class Union : SDFNodeBinary<Union.Op> {

    public override bool IsCommutative => true;

    public struct Op : IBinaryOp {
      public float Combine(float left, float right) {
        return min(left, right);
      }
    }
  }
}
