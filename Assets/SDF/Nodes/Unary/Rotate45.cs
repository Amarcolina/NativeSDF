//using System;

//namespace SDF {

//  [Serializable]
//  public class Rotate45 : SDFNodeUnary<Rotate45.Op> {

//    protected override Op GetOp() {
//      return default;
//    }

//    public struct Op : IUnaryOp {
//      public void Modify(ref float dist) {
//        p = (p + vec2(p.y, -p.x)) * sqrt(0.5);
//      }
//    }
//  }
//}
