using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class CircleController : MonoBehaviour, ICentersInfoProvider
{
    [SerializeField]
    [Range(100,100000)]
    private int _size=1000;


    [SerializeField]
    private int _numOfCentres = 3;
    [SerializeField]
    private int _numOfIterations = 10;

    [SerializeField]
    private Transform _parent;

    private ObjectPool<GameObject> _pool;

    [SerializeField]
    private Circle _prefab;

    [SerializeField]
    private int _leftScreenBorder = -100;
    [SerializeField]
    private int _rightScreenBorder = 100;
    [SerializeField]
    private int _upScreenBorder = 45;
    [SerializeField]
    private int _downScreenBorder = -45;

    private bool _checkCollection = false;
    private int _maxPoolSize = 100000;
    private int _poolSize = 50000;

    private int _id = 0;

    [Header("Настройка центров")]
    [SerializeField]
    private int _centersMult = 2;
    [SerializeField]
    private Vector3 _circleScale = new Vector3(1,1f,1f);

    [SerializeField]
    private List<Circle> _circles = new List<Circle>(10000);
    [SerializeField]
    private List<Circle> _centers = new List<Circle>();

    [SerializeField]
    private int _waitTime_ms = 1000;
    private float _now = 0f;

    private bool _wasChanged = true;
    private int _curIter;

    private void Start()
    {
        _pool = new ObjectPool<GameObject>(
               createFunc: () => SpawnRandomGameObject(),
                    actionOnGet: (obj) => ResetObject(obj),
                    actionOnRelease: (obj) => ReleaseObject(obj),
                    actionOnDestroy: (obj) => Destroy(obj),
                    collectionCheck: _checkCollection,
                    defaultCapacity: _poolSize,
                    maxSize: _maxPoolSize

            );
    }

    protected virtual GameObject Spawn()
    {
        var obj = _pool.Get();
        return obj;
    }

    private Vector3 GetRandomPosition()
    {
        var x = Random.Range(_leftScreenBorder, _rightScreenBorder);
        var y = Random.Range(_upScreenBorder, _downScreenBorder);

        return new Vector3(x, y, 1);
    }

    private Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    protected GameObject SpawnRandomGameObject()
    {
        var obj = Instantiate(_prefab, new Vector2(0, 0), Quaternion.identity, _parent);

        _circles.Add(obj);

        if (_id < _numOfCentres)
        {

            obj.SetUp(_circleScale * _centersMult, GetRandomPosition(),GetRandomColor(),(ICentersInfoProvider) this, _pool);
            _centers.Add(obj);
        }
        else
        {
            obj.SetUp(_circleScale, GetRandomPosition(), (ICentersInfoProvider)this, _pool);
        }

        _id++;

        return obj.gameObject;
    }
    protected virtual void ResetObject(GameObject obj)
    {
        obj.SetActive(true);
    }

    protected virtual void ReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    public Vector3[] GetCentersPositions()
    {
        Vector3[] positions = new Vector3[_numOfCentres];

        for (int i = 0; i < _numOfCentres; i++)
        {
            positions[i] = _centers[i].transform.position;
        }

        return positions;
    }

    public async void StartGeneration()
    {
        _wasChanged = true;
        _curIter = 1;
        while (_curIter < _numOfIterations+1 && _wasChanged)
        {
            //StartCoroutine(Generate());
            await Generate();

            _curIter++;
       
        }

        Debug.Log("Конец. Выполнено итераций: "+(_curIter-1));
    }

    async Task<int> Generate()
    {
        for (int i = 0; i < _circles.Count; i++)
        {
            _circles[i].Release();
        }
        //_circles.Clear();


        _id = 0;

        for (int i = 0; i < _size; i += 1)
        {
            Spawn();
        }

        //StartCoroutine(Wait());
        await Task.Delay(_waitTime_ms);

        _wasChanged = false;
        for (int i = _numOfCentres; i < _size; i += 1)
        {
            if(_circles[i].CheckDistance())
            {
                _wasChanged = true;
            }
        }

        //StartCoroutine(Wait());
        await Task.Delay(_waitTime_ms);
        _curIter++;
        RecalculateCenters();

        return 1;
    }

    private void Regenerate()
    {

        /*        for (int i = _numOfCentres; i < _circles.Count; i++)
                {
                    _circles[i].Release();
                }
                _circles.Clear();*/
        /*
                for (int i = 0; i < _numOfCentres; i++)
                {
                    _circles.Add(_centers[i]);
                }*/


        /*for (int i = 0; i < _size-_numOfCentres; i += 1)
        {
            Spawn();
        }*/

        _wasChanged = false;
        for (int i = _numOfCentres; i < _size; i += 1)
        {
              if(_circles[i].CheckDistance())
                _wasChanged = true;
        }
    }

    public Color[] GetCentersColors()
    {
        Color[] colors = new Color[_numOfCentres];

        for (int i = 0; i < _numOfCentres; i++)
        {
            colors[i] = _circles[i].Color;
        }

        return colors;
    }

    public Color GetCenterColorByPos(Vector3 pos)
    {
        for (int i = 0; i < _numOfCentres; i++)
        {
            if (_circles[i].transform.position == pos)
            {
                return _circles[i].Color;
            }
        }

        return Color.white;
    }


    private async void RecalculateCenters()
    {
        var newPositions = new Vector3[_numOfCentres];

        for(int i = 0; i < _numOfCentres; i++)
        {
            newPositions[i] = _centers[i].RecalculatePosition();
            _centers[i].transform.position = newPositions[i];
        }

        await Task.Delay(_waitTime_ms);

        Regenerate();
    }

    public Circle GetCenterByPos(Vector3 pos)
    {
        foreach(var center in _centers)
        {
            if(center.transform.position.x == pos.x && center.transform.position.y == pos.y)
            {
                return center;
            }
        }

        return null;
    }
}
