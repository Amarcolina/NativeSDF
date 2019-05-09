using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace SDF {
  using Internal;

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
    /// The type of this node.  Override in a child class to specify.
    /// </summary>
    public abstract NodeType NodeType { get; }

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
    /// Adds a new child to this node.  Depending on this node type, this operation might fail.  For example,
    /// it is always invalid to try to add a node as a child of a Shape node, since Shape nodes cannot have children.
    /// 
    /// These node-types can fail under certain conditions:
    ///   Shape             - Always fails, Shape nodes cannot have children
    ///   Unary             - Fails if the node already has a child, Unary nodes can only have one child.
    ///   Binary            - Fails if the node already has two children, Binary nodes can only have two children.
    /// </summary>
    public void Add(SDFNode node) {
      if (node == null) {
        throw new ArgumentNullException("node");
      }

      switch (NodeType) {
        case NodeType.Shape:
          throw new InvalidOperationException("The SDFNode " + this + " is a Shape node and so cannot have any children.");
        case NodeType.Unary:
          if (_children.Count >= 1) {
            throw new InvalidOperationException("The SDFNode " + this + " is a Unary node and so can have only a single child.");
          }
          break;
        case NodeType.Binary:
          if (_children.Count >= 2) {
            throw new InvalidOperationException("The SDFNode " + this + " is a Binary node and so can only have two children.\n" +
                                                "If you are extending SDFNodeBinary, and are writing a commutative node that can support " +
                                                "more than two children, override IsCommutative to return true.");
          }
          break;
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
      validate();

      var countVisitor = new InstructionCountVisitor();
      VisitInstructionsDepthFirst(ref countVisitor);

      var sizeVisitor = new BufferSizeVisitor();
      VisitInstructionsDepthFirst(ref sizeVisitor);

      var stackVisitor = new StackSizeVisitor();
      VisitInstructionsDepthFirst(ref stackVisitor);

      IntPtr instructions;
      unsafe {
        instructions = (IntPtr)UnsafeUtility.Malloc(sizeVisitor.BufferSize,
                                                    sizeof(float),
                                                    allocator);
      }

      var bufferWriter = new InstructionWriteVisitor();
      bufferWriter.ptr = instructions;
      bufferWriter.byteOffset = 0;
      VisitInstructionsDepthFirst(ref bufferWriter);

      return new NativeSDF(instructions, countVisitor.InstructionCount, stackVisitor.MaxStack, allocator);
    }

    /// <summary>
    /// Visits the tree in depth first order using the provided visitor.
    /// </summary>
    public void VisitInstructionsDepthFirst<VisitorType>(ref VisitorType visitor) where VisitorType : IInstructionVisitor {
      VisitPreOrderInstructions(ref visitor);

      for (int i = 0; i < _children.Count; i++) {
        _children[i].VisitInstructionsDepthFirst(ref visitor);
      }

      VisitPostOrderInstructions(ref visitor);
    }

    /// <summary>
    /// When called, you should call the Visit method on the visitor with every instruction you want to
    /// emit in the pre-order hierarchy pass.
    /// </summary>
    public abstract void VisitPreOrderInstructions<VisitorType>(ref VisitorType visitor) where VisitorType : IInstructionVisitor;

    /// <summary>
    /// When called, you should call the Visit method on the visitor with every instruction you want to
    /// emit in the post-order hierarchy pass.
    /// </summary>
    public abstract void VisitPostOrderInstructions<VisitorType>(ref VisitorType visitor) where VisitorType : IInstructionVisitor;

    public List<SDFNode>.Enumerator GetEnumerator() {
      return _children.GetEnumerator();
    }

    public override string ToString() {
      return GetType().Name;
    }

    public interface IInstructionVisitor {
      void Visit<InstructionType>(InstructionType instruction) where InstructionType : struct, IInstruction;
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

    private void validate() {
      switch (NodeType) {
        case NodeType.Shape:
          if (_children.Count != 0) {
            throw new InvalidOperationException("The SDFNode " + this + " had " + _children.Count + " children but expected to have 0.");
          }
          break;
        case NodeType.Unary:
          if (_children.Count != 1) {
            throw new InvalidOperationException("The SDFNode " + this + " had " + _children.Count + " children but expected to have 1.");
          }
          break;
        case NodeType.Binary:
          if (_children.Count != 2) {
            throw new InvalidOperationException("The SDFNode " + this + " had " + _children.Count + " children but expected to have 2.");
          }
          break;
        case NodeType.BinaryCommutative:
          if (_children.Count < 2) {
            throw new InvalidOperationException("The SDFNode " + this + " had " + _children.Count + " children but expected to have at least 2.");
          }
          break;
      }

      for (int i = 0; i < _children.Count; i++) {
        _children[i].validate();
      }
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
