using Unity.Mathematics;

namespace SDF {
  using Internal;

  /// <summary>
  /// This is a utility base class that can be used to create new types of shape nodes.
  /// 
  /// Shape nodes are leaf nodes in the signed distance hierarchy that generate a distance
  /// directly, and conceptually can represent shapes like spheres or boxes.
  /// 
  /// To create your own shape, you will need to extend this class and create a new shape
  /// 'operation'.  To create the new operation, you will want to create an inner struct
  /// that implements the IShapeOp interface contained within this SDFNodeShape class.  This
  /// operation struct is used as the generic parameter of SDFNodeShape.  This structuring 
  /// allows the base class to generate the instructions needed to execute the shape operation.
  /// 
  /// For an example of how to implement the SDFNodeShape class, you can take a look at Sphere.cs
  /// </summary>
  public abstract class SDFNodeShape<OpType> : SDFNode
  where OpType : struct, SDFNodeShape<OpType>.IShapeOp {

    public OpType Operation;
    public sealed override NodeType NodeType => NodeType.Shape;

    public SDFNodeShape() { }
    public SDFNodeShape(OpType operation) {
      Operation = operation;
    }

    public struct Instruction : IInstruction {
      public OpType Op;

      public int StackOffset => sizeof(float);
      public int StackOffset4x => sizeof(float) * 4;

      public unsafe void Exec(ref float* stack, ref float3 pos) {
        *(stack++) = Op.Sample(pos);
      }

      public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) {
        float4 dists;
        dists.x = Op.Sample(pos0);
        dists.y = Op.Sample(pos1);
        dists.z = Op.Sample(pos2);
        dists.w = Op.Sample(pos3);

        *(stack++) = dists;
      }
    }

    public override void VisitPreOrderInstructions<VisitorType>(ref VisitorType visitor) { }

    public override void VisitPostOrderInstructions<VisitorType>(ref VisitorType visitor) {
      visitor.Visit(new Instruction() { Op = Operation });
    }

    public interface IShapeOp {
      float Sample(float3 position);
    }
  }

  /// <summary>
  /// Similar to SDFNodeShape, but requires you to specify a '4x mode' operation for optimization purposes.
  /// </summary>
  public abstract class SDFNodeShape4x<OpType> : SDFNode
  where OpType : struct, SDFNodeShape4x<OpType>.IShapeOp {

    public OpType Operation;
    public sealed override NodeType NodeType => NodeType.Shape;

    public SDFNodeShape4x() { }
    public SDFNodeShape4x(OpType operation) {
      Operation = operation;
    }

    public struct Instruction : IInstruction {
      public OpType Op;

      public int StackOffset => sizeof(float);
      public int StackOffset4x => sizeof(float) * 4;

      public unsafe void Exec(ref float* stack, ref float3 pos) {
        *(stack++) = Op.Sample(pos);
      }

      public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) {
        *(stack++) = Op.Sample(pos0, pos1, pos2, pos3);
      }
    }

    public override void VisitPreOrderInstructions<VisitorType>(ref VisitorType visitor) { }

    public override void VisitPostOrderInstructions<VisitorType>(ref VisitorType visitor) {
      visitor.Visit(new Instruction() { Op = Operation });
    }

    public interface IShapeOp {
      float Sample(float3 position);
      float4 Sample(float3 position0, float3 position1, float3 position2, float3 position3);
    }
  }
}
