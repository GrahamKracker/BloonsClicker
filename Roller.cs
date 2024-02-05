using System;
using UnityEngine;

namespace BloonsClicker;

[RegisterTypeInIl2Cpp(false)]
public class Roller : MonoBehaviour
{
    public Roller(IntPtr ptr) : base(ptr)
    {
    }

    private const float Offset = .875f;

    private const float Length = 1 / 4f;

    private const float Speed = 4f;

    public void Update()
    {
        var scale = Mathf.Repeat(Time.unscaledTime / Speed, Length) + Offset;

        transform.localScale = new Vector3(scale, scale, scale);
    }
}