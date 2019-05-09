using Unity.Mathematics;

namespace SDF {
  using Internal;

  /// <summary>
  /// This is a utility base class that can be used to create new types of binary nodes.
  /// 
  /// Binary nodes are internal nodes in the signed distance hierarchy with exactly two children.
  /// They operate by combining the two distances of their children and outputing a resulting distance.
  /// An example of a binary operation is the intersection, union, or subtraction operations.
  /// 
  /// To create your own binary node, you will need to extend this class and create a new binary
  /// 'operation'.  To create the new operation, you will want to create an inner struct
  /// that implements the IBinaryOp interface contained within this SDFNodeBinary class.  This
  /// operation struct is used as the generic parameter of SDFNodeBinary.  This structuring 
  /// allows the base class to generate the instructions needed to execute the binary operation.
  /// 
  /// For an example of how to implement the SDFNodeBinary class, you can take a look at Union.cs
  /// </summary>
  public class SDFNodeBinary<OpType> : SDFNode<Nop, SDFNodeBinary<OpType>.Instruction>
  where OpType : struct, SDFNodeBinary<OpType>.IBinaryOp {

    public SDFNodeBinary() : base() { }
    public SDFNodeBinary(OpType op) : base(default, new Instruction() { Op = op }) { }

    public sealed override NodeType NodeType => IsCommutative ? NodeType.BinaryCommutative : NodeType.Binary;
    public virtual bool IsCommutative => false;

    public struct Instruction : IInstruction {
      public OpType Op;

      public int StackOffset => sizeof(float) * -1;
      public int StackOffset4x => sizeof(float) * -4;

      public unsafe void Exec(ref float* stack, ref float3 pos) {
        stack--;
        *(stack - 1) = Op.Combine(*(stack - 1), *(stack - 0));
      }

      public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) {
        stack--;
        float4 left = *(stack - 1);
        float4 right = *(stack - 0);

        float4 dists;
        dists.x = Op.Combine(left.x, right.x);
        dists.y = Op.Combine(left.y, right.y);
        dists.z = Op.Combine(left.z, right.z);
        dists.w = Op.Combine(left.w, right.w);

        *(stack - 1) = dists;
      }
    }

    public override void VisitPostOrderInstructions<VisitorType>(ref VisitorType visitor) {
      //This logic is to handle commutative operations with more than 2 children.
      //If we have N children, we have N distances on the stack.
      //Each instruction reduces that amount by 1, so we want to emit N-1 instructions
      //to wind up with a final single distance on the stack.
      for (int i = 0; i < ChildrenCount - 1; i++) {
        visitor.Visit(ref PostInstruction);
      }
    }

    public interface IBinaryOp {
      float Combine(float left, float right);
    }
  }

  /// <summary>
  /// Similar to SDFNodeUnary, but requires you to specify a '4x mode' operation for optimization purposes.
  /// </summary>
  public class SDFNodeBinary4x<OpType> : SDFNode<Nop, SDFNodeBinary4x<OpType>.Instruction>
  where OpType : struct, SDFNodeBinary4x<OpType>.IBinaryOp {

    public SDFNodeBinary4x() : base() { }
    public SDFNodeBinary4x(OpType op) : base(default(Nop), new Instruction() { Op = op }) { }

    public sealed override NodeType NodeType => IsCommutative ? NodeType.BinaryCommutative : NodeType.Binary;
    public virtual bool IsCommutative => false;

    public struct Instruction : IInstruction {
      public OpType Op;

      public int StackOffset => sizeof(float) * -1;
      public int StackOffset4x => sizeof(float) * -4;

      public unsafe void Exec(ref float* stack, ref float3 pos) {
        stack--;
        *(stack - 1) = Op.Combine(*(stack - 1), *(stack - 0));
      }

      public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) {
        stack--;
        *(stack - 1) = Op.Combine(*(stack - 1), *(stack - 0));
      }
    }

    public override void VisitPostOrderInstructions<VisitorType>(ref VisitorType visitor) {
      for (int i = 0; i < ChildrenCount - 1; i++) {
        visitor.Visit(ref PostInstruction);
      }
    }

    public interface IBinaryOp {
      float Combine(float left, float right);
      float4 Combine(float4 left, float4 right);
    }
  }
}
