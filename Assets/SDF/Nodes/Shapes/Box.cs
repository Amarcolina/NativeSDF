using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

  [Serializable]
  public class Box : SDFNodeShape<Box.Op> {

    public float4x4 ToLocalSpace;
    public float3 Extents;

    protected override Op GetOp() {
      return new Op() {
        ToLocalSpace = ToLocalSpace,
        Extents = Extents
      };
    }

    public struct Op : IShapeOp {
      public float4x4 ToLocalSpace;
      public float3 Extents;

      public float Sample(float3 position) {
        float4 l = mul(ToLocalSpace, new float4(position, 1.0f));
        float3 localPos;
        localPos.x = l.x;
        localPos.y = l.y;
        localPos.z = l.z;

        float3 d = abs(localPos) - Extents;
        return length(max(d, 0.0f))
               + min(max(d.x, max(d.y, d.z)), 0.0f);
      }
    }
  }
}
