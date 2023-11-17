using System;
using UnityEngine;

namespace BloonsClicker;

[RegisterTypeInIl2Cpp(false)]
public class Roller : MonoBehaviour
{
    public Roller(IntPtr ptr) : base(ptr) { }

    private float offset = .875f;

    private float length = 1/4f;

    private float speed = 4f;

    public void Update()
    {
        var scale = Mathf.Repeat(Time.unscaledTime/speed, length) + offset;
        
        transform.localScale = new Vector3(scale, scale, scale);
    }
}