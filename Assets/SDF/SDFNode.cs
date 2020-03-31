using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using static Unity.Mathematics.math;

namespace SDF {
    using Internal;
    using Unity.Mathematics;

    /// <summary>
    /// Represents a single node in a tree of signed distance field operations.  Through
    /// this class you can manipulate the operation tree, or construct NativeSDF structures
    /// to evaluate the tree efficiently.  NativeSDF instances can also be used inside of jobs
    /// and be burst compiled.
    /// 
    /// To create new types of signed distance operations, you will want to inherit from this
    /// class directly, or from one of its useful subclasses.  For reference, these are some
    /// useful subclasses to look at when creating new distance operations:
    ///   SDFNodeShape  - Inherit to create new types of primitives.  Some examples are things 
    ///                   like spheres, boxes, or capsules.
    ///   SDFNodeUnary  - Inherit to create new types of unary operations that modify the distances
    ///                   of child nodes.  Some examples are things like inverting shapes, or adding
    ///                   skins.
    ///   SDFNodeBinary - Inherit to create new types of binary operations that combine the two
    ///                   distances of two child nodes to create a new distance.  Some examples are
    ///                   things like union, intersection, or subtraction operations.
    /// </summary>
    public abstract class SDFNode : IEnumerable<SDFNode> {

        #region API

        /// <summary>
        /// The number of child nodes this node has.
        /// </summary>
        public int ChildrenCount {
            get {
                return _children.Count;
            }
        }

        /// <summary>
        /// Removes all children from this node.
        /// </summary>
        public void ClearChildren() {
            _children.Clear();
        }

        /// <summary>
        /// Adds a new child to this node. 
        /// </summary>
        public virtual void Add(SDFNode node) {
            if (node == null) {
                throw new ArgumentNullException("node");
            }

            _children.Add(node);
        }

        /// <summary>
        /// Compiles this tree into a NativeSDF instance that can be used to efficiently sample the distance 
        /// field represented by this hierarchy.  A compiled NativeSDF instance will not reflect any changes 
        /// made to the hierarchy after the compile step.
        /// 
        /// The resulting NativeSDF instance can also be used inside of the Unity Job system, and is compatible
        /// with the Unity Burst compiler.
        /// </summary>
        public NativeSDF Compile(Allocator allocator = Allocator.Persistent) {
            var countVisitor = new InstructionCountVisitor();
            VisitInstructions(ref countVisitor);

            var sizeVisitor = new BufferSizeVisitor();
            VisitInstructions(ref sizeVisitor);

            var stackVisitor = new StackSizeVisitor();
            VisitInstructions(ref stackVisitor);

            IntPtr instructions;
            unsafe {
                instructions = (IntPtr)UnsafeUtility.Malloc(sizeVisitor.BufferSize,
                                                            sizeof(float),
                                                            allocator);
            }

            var bufferWriter = new InstructionWriteVisitor();
            bufferWriter.ptr = instructions;
            bufferWriter.byteOffset = 0;
            VisitInstructions(ref bufferWriter);

            return new NativeSDF(instructions, countVisitor.InstructionCount, stackVisitor.MaxStack, allocator);
        }

        /// <summary>
        /// Visits the tree using the provided visitor, emitting instructions along the way.
        /// </summary>
        public abstract void VisitInstructions<VisitorType>(ref VisitorType visitor)
            where VisitorType : IInstructionVisitor;

        public SDFNode this[int index] {
            get {
                return _children[index];
            }
        }

        public List<SDFNode>.Enumerator GetEnumerator() {
            return _children.GetEnumerator();
        }

        public override string ToString() {
            return GetType().Name;
        }

        public interface IInstructionVisitor {
            void Visit<InstructionType>(InstructionType instruction) where InstructionType : struct, IInstruction;
        }

        protected static float mod(float x, float y) {
            return x - y * floor(x / y);
        }

        protected static float3 mul(float4x4 mat, float3 pos) {
            float4 result = math.mul(mat, new float4(pos.x, pos.y, pos.z, 1.0f));
            return new float3(result.x, result.y, result.z);
        }

        #endregion

        #region IMPLEMENTATION

        private List<SDFNode> _children = new List<SDFNode>();

        IEnumerator<SDFNode> IEnumerable<SDFNode>.GetEnumerator() {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private struct InstructionCountVisitor : IInstructionVisitor {
            public int InstructionCount;

            public void Visit<InstructionType>(InstructionType instruction) where InstructionType : struct, IInstruction {
                InstructionCount++;
            }
        }

        private struct BufferSizeVisitor : IInstructionVisitor {
            public int BufferSize;

            public void Visit<InstructionType>(InstructionType instruction) where InstructionType : struct, IInstruction {
                BufferSize += sizeof(uint);
                BufferSize += UnsafeUtility.SizeOf<InstructionType>();
            }
        }

        private struct StackSizeVisitor : IInstructionVisitor {
            public int CurrStack;
            public int MaxStack;

            public void Visit<InstructionType>(InstructionType instruction) where InstructionType : struct, IInstruction {
                CurrStack += instruction.StackOffset4x;
                MaxStack = Math.Max(MaxStack, CurrStack);
            }
        }

        private struct InstructionWriteVisitor : IInstructionVisitor {
            public IntPtr ptr;
            public int byteOffset;

            public void Visit<InstructionType>(InstructionType instruction) where InstructionType : struct, IInstruction {
                byte opCode = Instruction.GetOpCode<InstructionType>();

                unsafe {
                    UnsafeUtility.CopyStructureToPtr(ref opCode, ((byte*)ptr + byteOffset));
                    byteOffset += sizeof(uint);

                    UnsafeUtility.CopyStructureToPtr(ref instruction, ((byte*)ptr + byteOffset));
                    byteOffset += UnsafeUtility.SizeOf<InstructionType>();
                }
            }
        }
        #endregion
    }
}
