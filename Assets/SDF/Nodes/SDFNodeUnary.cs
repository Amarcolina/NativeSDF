using System;
using Unity.Mathematics;

namespace SDF {
    using Internal;

    /// <summary>
    /// This is a utility base class that can be used to create new types of unary operation nodes.
    /// 
    /// Unary operations are internal nodes in the signed distance hierarchy with a single child.
    /// They operate on the distance of their single child, and do not take the sample position into
    /// consideration.  An example of a unary operation is the inverse, or dilation, of a child shape.
    /// 
    /// To create your own unary node, you will need to extend this class and create a new unary
    /// 'operation'.  To create the new operation, you will want to create an inner struct
    /// that implements the IUnaryOp interface contained within this SDFNodeUnary class.  This
    /// operation struct is used as the generic parameter of SDFNodeUnary.  This structuring 
    /// allows the base class to generate the instructions needed to execute the unary operation.
    /// 
    /// For an example of how to implement the SDFNodeUnary class, you can take a look at Inverse.cs
    /// </summary>
    public abstract class SDFNodeUnary<OpType> : SDFNode
        where OpType : struct, SDFNodeUnary<OpType>.IUnaryOp {

        public override void Add(SDFNode node) {
            if (ChildrenCount >= 1) {
                throw new InvalidOperationException("Cannot add a second child to " + this + " because it is a Unary node which can only accept a single child.");
            }

            base.Add(node);
        }

        public override void VisitInstructions<VisitorType>(ref VisitorType visitor) {
            if (ChildrenCount != 1) {
                throw new InvalidOperationException("The SDFNode " + this + " had " + ChildrenCount + " children but expected exactly 1.");
            }

            //First emit instructions for single child
            this[0].VisitInstructions(ref visitor);

            //Then emit our own instruction for our unary op
            visitor.Visit(new Instruction() { Op = GetOp() });
        }

        protected abstract OpType GetOp();

        public struct Instruction : IInstruction {
            public OpType Op;

            public int StackOffset => 0;
            public int StackOffset4x => 0;

            public unsafe void Exec(ref float* stack, ref float3 pos) {
                Op.Modify(ref *(stack - 1));
            }

            public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) {
                float4 dists = *(stack - 1);
                Op.Modify(ref dists.x);
                Op.Modify(ref dists.y);
                Op.Modify(ref dists.z);
                Op.Modify(ref dists.w);
                *(stack - 1) = dists;
            }
        }

        public interface IUnaryOp {
            void Modify(ref float dist);
        }
    }
}
