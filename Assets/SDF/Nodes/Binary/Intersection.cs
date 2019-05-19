﻿using static Unity.Mathematics.math;

namespace SDF {

  public class Intersection : SDFNodeBinary<Intersection.Op> {

    public override bool IsCommutative => true;

    protected override Op GetOp() {
      return default;
    }

    public struct Op : IBinaryOp {
      public float Combine(float left, float right) {
        return max(left, right);
      }
    }
  }
}
