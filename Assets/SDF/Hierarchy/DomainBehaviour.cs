using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDF.Hierarchy {

  public class DomainBehaviour : SDFBehaviour {

    public DomainType Type;
    public float CellSize = 1;

    private SDFNode _cachedNode;

    protected override SDFNode GenerateNode() {
      var repeat = _cachedNode as Repeat;

      switch (Type) {
        case DomainType.Repeat:
          if (repeat == null) _cachedNode = repeat = new Repeat();
          repeat.CellSize = CellSize;
          break;
      }

      return _cachedNode;
    }

    public enum DomainType {
      Repeat
    }
  }
}
