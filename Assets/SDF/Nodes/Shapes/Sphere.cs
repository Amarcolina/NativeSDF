using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace SDF {

  public class Sphere : SDFNodeShape<Sphere.Op> {

    public Sphere() : base() { }
    public Sphere(float3 center, float radius) : base(new Op() { Center = center, Radius = radius }) { }

    public struct Op : IShapeOp {
      public float3 Center;
      public float Radius;

      public float Sample(float3 position) {
        float3 toCenter = Center - position;
        return length(toCenter) - Radius;
      }
    }
  }
}
