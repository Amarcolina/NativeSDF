
namespace SDF {

  /// <summary>
  /// This node can be used to mimic the behavior of another node instance.  This can be used
  /// when you want to have multiple nodes that are identical, and only want to define the 
  /// node parameters once.  This is also implicitly used by the SDFNode native compiler to
  /// expand commutative binary nodes.
  /// 
  /// NOTE: This node only mimics the behavior of a source node, it does not embed the entire
  /// hierarchy of the source node.
  /// </summary>
  public class SDFNodeMimic : SDFNode {
    private SDFNode _source;

    public override NodeType NodeType => _source.NodeType;

    public SDFNodeMimic(SDFNode source) {
      _source = source;
    }

    public override void VisitPreOrderInstructions<VisitorType>(ref VisitorType visitor) {
      _source.VisitPreOrderInstructions(ref visitor);
    }

    public override void VisitPostOrderInstructions<VisitorType>(ref VisitorType visitor) {
      _source.VisitPostOrderInstructions(ref visitor);
    }
  }
}
