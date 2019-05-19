using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDF.Hierarchy {

  public class DomainBehaviour : SDFBehaviour {

    public DomainType Type;
    public float CellSize = 1;
    public bool ModX, ModY, ModZ;

    private SDFNode _cachedNode;

    protected override SDFNode GenerateNode() {
      var modSimple = _cachedNode as ModSimple;

      switch (Type) {
        case DomainType.ModSimple:
          if (modSimple == null) _cachedNode = modSimple = new ModSimple();
          modSimple.CellSize = CellSize;
          modSimple.ModX = ModX;
          modSimple.ModY = ModY;
          modSimple.ModZ = ModZ;
          break;
      }

      return _cachedNode;
    }

    public enum DomainType {
      ModSimple
    }
  }
}
