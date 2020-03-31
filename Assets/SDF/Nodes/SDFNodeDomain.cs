using System;
using Unity.Mathematics;

namespace SDF {
    using Internal;

    public abstract class SDFNodeDomainBase : SDFNode {

        public struct PopPositionsInstruction : IInstruction {

            public int StackOffset => sizeof(float) * -3;
            public int StackOffset4x => sizeof(float) * 4 * -3;

            public unsafe void Exec(ref float* stack, ref float3 pos) {
                float distance = *(--stack);

                float3* posPtr = (float3*)stack;
                pos = *(--posPtr);
                stack = (float*)posPtr;

                *(stack++) = distance;
            }

            public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) {
                float4 distances = *(--stack);

                float3* posPtr = (float3*)stack;
                pos3 = *(--posPtr);
                pos2 = *(--posPtr);
                pos1 = *(--posPtr);
                pos0 = *(--posPtr);
                stack = (float4*)posPtr;

                *(stack++) = distances;
            }
        }
    }

    public abstract class SDFNodeDomainSimple<OpType> : SDFNodeDomainBase
        where OpType : struct, SDFNodeDomainSimple<OpType>.IDomainOp {

        public SDFNodeDomainSimple() { }

        public override void Add(SDFNode node) {
            base.Add(node);
        }

        public override void VisitInstructions<VisitorType>(ref VisitorType visitor) {
            if (ChildrenCount != 1) {
                throw new InvalidOperationException("The Domain Node " + this + " had " + ChildrenCount + " but expected at exactly 1.");
            }

            //First push the positions and execute the domain operation
            visitor.Visit(new Instruction() { Op = GetOp() });

            //Then execute the single child, which should result in a single position being put onto
            //the stack
            this[0].VisitInstructions(ref visitor);

            //Finally pop the original positions off the stack
            visitor.Visit(new PopPositionsInstruction());
        }

        protected abstract OpType GetOp();

        public struct Instruction : IInstruction {
            public OpType Op;

            public int StackOffset => sizeof(float) * 3;
            public int StackOffset4x => sizeof(float) * 4 * 3;

            public unsafe void Exec(ref float* stack, ref float3 pos) {
                float3* posPtr = (float3*)stack;
                *(posPtr++) = pos;
                stack = (float*)posPtr;

                Op.Modify(ref pos);
            }

            public unsafe void Exec(ref float4* stack, ref float3 pos0, ref float3 pos1, ref float3 pos2, ref float3 pos3) {
                float3* posPtr = (float3*)stack;
                *(posPtr++) = pos0;
                *(posPtr++) = pos1;
                *(posPtr++) = pos2;
                *(posPtr++) = pos3;
                stack = (float4*)posPtr;

                Op.Modify(ref pos0);
                Op.Modify(ref pos1);
                Op.Modify(ref pos2);
                Op.Modify(ref pos3);
            }
        }

        public interface IDomainOp {
            void Modify(ref float3 pos);
        }
    }
}