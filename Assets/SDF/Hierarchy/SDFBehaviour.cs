using System.Collections.Generic;
using UnityEngine;

namespace SDF.Hierarchy {

  public abstract class SDFBehaviour : MonoBehaviour {

    protected abstract SDFNode GenerateNode();

    public SDFNode GetNode() {
      SDFNode node = GenerateNode();

      node.ClearChildren();
      linkUpChildren(node, transform);

      return node;
    }

    private static List<SDFBehaviour> _tmpList = new List<SDFBehaviour>();
    private void linkUpChildren(SDFNode node, Transform t) {
      for (int i = 0; i < t.childCount; i++) {
        t.GetChild(i).GetComponents(_tmpList);
        if (_tmpList.Count > 0) {
          node.Add(_tmpList[0].GetNode());
        } else {
          linkUpChildren(node, t.GetChild(i));
        }
      }
    }
  }
}
