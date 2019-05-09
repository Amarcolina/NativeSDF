using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

  public class Box : SDFNodeShape<Box.Op> {

    public Box() : base() { }
    public Box(float4x4 toLocalSpace, float3 extents) : base(new Op() { ToLocalSpace = toLocalSpace, Extents = extents }) { }

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