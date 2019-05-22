using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;
using SDF;
using SDF.Hierarchy;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class TextureSlice : MonoBehaviour {

  public MeshRenderer QuadRenderer;
  [Range(8, 128)]
  public int Resolution;

  private Texture2D _tex;

  private void OnValidate() {
    Resolution = Mathf.RoundToInt(Resolution / 4.0f) * 4;
  }

  private void Update() {
    if (!_tex || _tex.width != Resolution) {
      _tex = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, mipChain: false, linear: true);
      _tex.wrapMode = TextureWrapMode.Clamp;
      _tex.filterMode = FilterMode.Trilinear;

      QuadRenderer.material.mainTexture = _tex;
    }

    NativeArray<Color32> color = _tex.GetRawTextureData<Color32>();

    SDFBehaviour sdfBehaviour = GetComponent<SDFBehaviour>();
    var node = sdfBehaviour.GetNode();
    if (node == null) {
      return;
    }

    NativeSDF sdf = node.Compile(Allocator.TempJob);

    Vector3 rectCorner = QuadRenderer.transform.TransformPoint(new Vector3(-0.5f, -0.5f) + new Vector3(0.5f, 0.5f) / Resolution);
    Vector3 rectXAxis = QuadRenderer.transform.TransformVector(Vector3.right) / Resolution;
    Vector3 rectYAxis = QuadRenderer.transform.TransformVector(Vector3.up) / Resolution;

    new BuildTextureJob() {
      SDF = sdf,
      Texture = color,
      TextureWidth = Resolution,
      RectCorner = rectCorner,
      RectXAxis = rectXAxis,
      RectYAxis = rectYAxis
    }.Schedule(color.Length, 32).Complete();

    _tex.Apply(updateMipmaps: false, makeNoLongerReadable: false);

    sdf.Dispose();
  }

  [BurstCompile]
  public struct BuildTextureJob : IJobParallelFor {
    public NativeSDF SDF;
    public NativeArray<Color32> Texture;
    public int TextureWidth;

    public float3 RectCorner; //Corner is centered on bottom lower texel
    public float3 RectXAxis;  //Axes have length of 1 texel
    public float3 RectYAxis;

    public void Execute(int index) {
      float x = index % TextureWidth;
      float y = index / TextureWidth;

      float3 pos = RectCorner + x * RectXAxis + y * RectYAxis;

      float dist = SDF.Sample(pos);

      float ring = smoothstep(0.0f, 0.1f, abs(frac(abs(dist) * 3f) - 0.5f));
      float color = smoothstep(-0.02f, 0.02f, dist);
      Texture[index] = ring * Color.Lerp(new Color(1, 0, 0, 1), new Color(0, 1, 0, 1), color);
    }
  }
}
