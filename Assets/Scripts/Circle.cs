using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Pool;

public class Circle : MonoBehaviour
{
    private Vector3 _oldScale;

    [SerializeField]
    private SpriteRenderer _render;

    private IObjectPool<GameObject> _pool;


    public Color Color;
    private ICentersInfoProvider _provider;

    [SerializeField]
    private List<Circle> _clasterCircles = new List<Circle>();
    [SerializeField]
    private Circle _prevClaster;

    /*   static Color HsvToRgb(float h, float s, float v)
       {
           int i;
           float f, p, q, t;

           if (s < float.Epsilon)
           {
               int c = (int)(v * 255);
               return Color.HSVToRGB(c, c, c);
           }

           h /= 60;
           i = (int)Math.Floor(h);
           f = h - i;
           p = v * (1 - s);
           q = v * (1 - s * f);
           t = v * (1 - s * (1 - f));

           float r, g, b;
           switch (i)
           {
               case 0: r = v; g = t; b = p; break;
               case 1: r = q; g = v; b = p; break;
               case 2: r = p; g = v; b = t; break;
               case 3: r = p; g = q; b = v; break;
               case 4: r = t; g = p; b = v; break;
               default: r = v; g = p; b = q; break;
           }


           return Color.HSVToRGB((int)(r * 255), (int)(g * 255), (int)(b * 255));
       }*/


    /*  static Color HsvToRgb(float x, float y, float z)
      {
          var x_k = (float)(100 + x) / 200f;
          var y_k = (float)(45 + y) / 90f;
          var z_k = (float)z;

          Debug.Log(x_k + " " + y_k);

          return Color.HSVToRGB((int)(x_k * 255), (int)(y_k * 255), (int)(x_k * 255));
      }

      private void ChangeColorTimer()
      {
          //_time++;
          //if (_time >= 360) _time = 0;
          var pos = transform.position;

          _render.color = HsvToRgb(pos.x, pos.y, pos.z);
      }*/

    public void ChangeColor(in Color color)
    {
        Color = color;
        _render.color = Color;
    }

    #region CenterMethods

    public void AddPointToClaster(Circle circle)
    {
        _clasterCircles.Add(circle);
    }

    public void RemovePointFromClaster(Circle circle)
    {
        _clasterCircles.Remove(circle);
    }

    public Vector3 RecalculatePosition()
    {
        Vector3 sum = new Vector3(0, 0, transform.position.z);
        
        foreach(var circle in _clasterCircles)
        {
            sum.x += circle.transform.position.x;
            sum.y += circle.transform.position.y;
        }

        var avr = sum / _clasterCircles.Count;

        //_clasterCircles.Clear();

        return avr;
    }

    #endregion


    public bool CheckDistance()
    {
        var centers = _provider.GetCentersPositions();

        var minIndex = 0;
        var min = (centers[0] - transform.position).magnitude;

        for (int i = 1; i < centers.Length; i++)
        {
            var dist = (centers[i] - transform.position).magnitude;

            if (dist < min)
            {
                min = dist;
                minIndex = i;
            }
        }

        var centerPos = centers[minIndex];
        var claster = _provider.GetCenterByPos(centerPos);

        //var color = _provider.GetCenterColorByPos(centerPos);

        if (claster != _prevClaster)
        {
            claster.AddPointToClaster(this);
            if(_prevClaster)
                _prevClaster.RemovePointFromClaster(this);
            ChangeColor(claster.Color);

            _prevClaster = claster;


            return true;
        }

        return false;
    }

    public void SetUp(in Vector3 scale, in Vector3 pos, ICentersInfoProvider prov, IObjectPool<GameObject> pool)
    {
        _oldScale = transform.localScale;
        transform.localScale = scale;

        transform.position = pos;
        _provider = prov;
        _pool = pool;
    }

    public void SetUp(in Vector3 scale, in Vector3 pos, in Color color, ICentersInfoProvider prov, IObjectPool<GameObject> pool)
    {
        _oldScale = transform.localScale;
        transform.localScale = scale;

        ChangeColor(color);

        transform.position = pos;
        _provider = prov;
        _pool = pool;
    }

    public void SetUp(in Vector3 scale, in Vector3 pos, in Color color)
    {
        _oldScale = transform.localScale;
        transform.localScale = scale;

        ChangeColor(color);

        transform.position = pos;
    }
    public void SetUp(in Vector3 scale, in Color color)
    {
        _oldScale = transform.localScale;
        transform.localScale = scale;

        ChangeColor(color);
    }

    public void Release()
    {
        //transform.localScale = _oldScale;

        /* if (_clasterCircles.Count > 0)
         {
             foreach (var circle in _clasterCircles)
             {
                 circle.Release();
                 Destroy(circle);
             }

             _clasterCircles.Clear();
         }*/

        _pool.Release(gameObject);     
    }
}
