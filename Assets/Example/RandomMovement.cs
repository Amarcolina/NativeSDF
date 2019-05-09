using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour {

  public Vector3 amplitude;
  public float freq;
  public float variation;
  public Vector3 offset;

  public float freqX, freqY, freqZ;
  public float rotX, rotY, rotZ;

  public bool moving = true;
  public float time = 0;

  private void Start() {
    freqX = Random.Range(freq - variation, freq + variation);
    freqY = Random.Range(freq - variation, freq + variation);
    freqZ = Random.Range(freq - variation, freq + variation);

    rotX = 45 * Random.Range(freq - variation, freq + variation);
    rotY = 45 * Random.Range(freq - variation, freq + variation);
    rotZ = 45 * Random.Range(freq - variation, freq + variation);
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      moving = !moving;
    }

    if (moving) {
      time += Time.deltaTime;
      transform.position = new Vector3(sin(freqX) * amplitude.x, sin(freqY) * amplitude.y, sin(freqZ) * amplitude.z) + offset;
      transform.eulerAngles = new Vector3(time * rotX, time * rotY, time * rotZ);
    }
  }

  private float sin(float freq) {
    return Mathf.Sin(time * freq) * 0.5f + 0.5f;
  }
}
