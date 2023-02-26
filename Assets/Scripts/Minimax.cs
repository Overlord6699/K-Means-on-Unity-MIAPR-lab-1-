using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = System.Random;

public struct Shape
{
    public Point Point { get; set; }

    public int AmountOfClasses { get; set; }
}

public class Minimax : MonoBehaviour
{
    private const int MAX_COLOR_NUM = 256;


    [SerializeField]
    private int _leftScreenBorder = -100;
    [SerializeField]
    private int _rightScreenBorder = 100;
    [SerializeField]
    private int _upScreenBorder = 45;
    [SerializeField]
    private int _downScreenBorder = -45;


    [SerializeField]
    private int _maxNumOfClasses = 20;

    [SerializeField]
    private float ShapePointWidth = 1;
    [SerializeField]
    private float ShapePointHeight = 1;
    [SerializeField]
    private float ClassPointWidth = 4;
    [SerializeField]
    private float ClassPointHeight = 4;

    [SerializeField]
    private int _amountOfShapes = 100;

    [SerializeField]
    private Circle _prefab;
    [SerializeField]
    private Transform _parent;

    private readonly Random _random = new Random();
    private readonly Color[] _colors = new Color[20];

    private Shape[] _shapes;
    private Shape[] _kernels;
    private Shape[] _newKernels;
    private int _amountOfClasses;

    [SerializeField]
    private List<Circle> _circles = new List<Circle>();
    [SerializeField]
    private List<Circle> _classes = new List<Circle>();

    private bool _isReady = true;

    private void Start()
    {
        for (int i = 0; i < _colors.Length; i++)
        {
            _colors[i] = new Color(_random.Next(MAX_COLOR_NUM), _random.Next(MAX_COLOR_NUM),
                _random.Next(MAX_COLOR_NUM));
        }

        Show();
    }

    private void Show()
    {

        _shapes = new Shape[_amountOfShapes];

        for (int i = 0; i < _amountOfShapes; i++)
        {
            var x = _random.Next(_leftScreenBorder, _rightScreenBorder);
            var y = _random.Next(_downScreenBorder, _upScreenBorder);
            _shapes[i].Point = new Point(x, y);
            _shapes[i].AmountOfClasses = 0;

            var obj = Instantiate(_prefab, new Vector3(_shapes[i].Point.X, _shapes[i].Point.Y, 1f), Quaternion.identity, _parent);
            _circles.Add(obj);
        }

        _kernels = new Shape[_maxNumOfClasses];
        int randomNumber = _random.Next(_amountOfShapes);
        _kernels[0].Point = _shapes[randomNumber].Point;

        for (int i = randomNumber; i < _amountOfShapes - 1; i++)
        {
            _shapes[i] = _shapes[i + 1];
        }

        var kernel =Instantiate(_prefab, new Vector3(_kernels[0].Point.X, _kernels[0].Point.Y, 1f), Quaternion.identity, _parent);
        kernel.SetUp(
            new Vector3(ClassPointWidth, ClassPointHeight, 1),
            Color.black
        );
        _classes.Add(kernel);
    }

    public void Generate()
    {
        //StartCoroutine(Process());
        Process();
    }


    private void Process()
    {
        double maxDistance = -1;
        double distance;
        int index = 0;

        for (int i = 0; i < _amountOfShapes; i++)
        {
            distance = Math.Sqrt(Math.Pow(_shapes[i].Point.X - _kernels[0].Point.X, 2) +
                                    Math.Pow(_shapes[i].Point.Y - _kernels[0].Point.Y, 2));
            if (distance > maxDistance)
            {
                maxDistance = distance;
                _kernels[1].Point = _shapes[i].Point;
                index = i;
            }
        }

        for (int i = index; i < _amountOfShapes - 1; i++)
        {
            _shapes[i] = _shapes[i + 1];
        }

        _amountOfShapes -= 1;
        _kernels[1].AmountOfClasses = 1;
        _amountOfClasses = 2;

        _isReady = true;




        while (_isReady)
        {

            //yield return new WaitForSeconds(2);

            foreach (var obj in _circles)
            {
                obj.gameObject.SetActive(true);
            }
            foreach (var obj in _classes)
            {
                obj.gameObject.SetActive(true);
            }


            for (int i = 0; i < _amountOfClasses; i++)
            {
                //var obj = Instantiate(_prefab, new Vector3(_kernels[i].Point.X, _kernels[i].Point.Y, 1f), Quaternion.identity, _parent);
                _circles[i].SetUp(
                    new Vector3(ClassPointWidth, ClassPointHeight, 1),
                    new Vector3(_kernels[i].Point.X, _kernels[i].Point.Y, 1f),
                    Color.black);
                _classes.Add(_circles[i]);

                //_circles.Add(obj);
            }

            for (int i = 0; i < _amountOfShapes; i++)
            {
                double minDistance = 100000000;
                for (int j = 0; j < _amountOfClasses; j++)
                {
                    distance = Math.Sqrt(
                        (_shapes[i].Point.X - _kernels[j].Point.X) * (_shapes[i].Point.X - _kernels[j].Point.X) +
                        (_shapes[i].Point.Y - _kernels[j].Point.Y) * (_shapes[i].Point.Y - _kernels[j].Point.Y));

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        _shapes[i].AmountOfClasses = j;
                    }
                }
            }

            for (int i = 0; i < _amountOfShapes; i++)
            {
                //var obj = Instantiate(_prefab, new Vector3(_shapes[i].Point.X, _shapes[i].Point.Y, 1f), Quaternion.identity, _parent);
                //_circles.Add(obj);
                _circles[i].SetUp(new Vector3(2, 2, 2),
                    new Vector3(_shapes[i].Point.X, _shapes[i].Point.Y, 1f),
                    _colors[_shapes[i].AmountOfClasses]
                );
            }

            for (int i = 0; i < _amountOfClasses; i++)
            {
                _classes[i].SetUp(new Vector3(ClassPointWidth, ClassPointHeight, 1),
                    new Vector3(_kernels[i].Point.X, _kernels[i].Point.Y, 1),
                    Color.black
                );
                //var obj = Instantiate(_prefab, new Vector3(_kernels[i].Point.X, _shapes[i].Point.Y, 1f), Quaternion.identity, _parent);
                //_circles.Add(obj);
            }

            double averageDistance = 0;
            int numberOfDistance = 0;

            for (int i = 0; i < _amountOfClasses - 1; i++)
            {
                for (int j = i + 1; j < _amountOfClasses; j++)
                {
                    averageDistance +=
                        Math.Sqrt(
                            (_kernels[i].Point.X - _kernels[j].Point.X) *
                            (_kernels[i].Point.X - _kernels[j].Point.X) +
                            (_kernels[i].Point.Y - _kernels[j].Point.Y) *
                            (_kernels[i].Point.Y - _kernels[j].Point.Y));
                    numberOfDistance += 1;
                }
            }

            averageDistance /= numberOfDistance;
            averageDistance /= 2;

            maxDistance = -1;

            for (int i = 0; i < _amountOfClasses; i++)
            {
                for (int j = 0; j < _amountOfShapes; j++)
                {

                    if (_shapes[j].AmountOfClasses == i)
                    {
                        distance = Math.Sqrt(Math.Pow(_shapes[j].Point.X - _kernels[i].Point.X, 2) +
                                                Math.Pow(_shapes[j].Point.Y - _kernels[i].Point.Y, 2));
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            _kernels[_amountOfClasses].Point = _shapes[j].Point;
                            index = j;
                        }
                    }
                }
            }

            if (maxDistance > averageDistance)
            {
                _amountOfClasses += 1;

                for (int i = index; i < _amountOfShapes - 1; i++) _shapes[i] = _shapes[i + 1];

                _amountOfShapes -= 1;

                //очистка
                Clear();
            }
            else
            {
                _isReady = false;
            }
        }
    }

    private void Clear()
    {
        foreach (var obj in _circles)
        {
            obj.gameObject.SetActive(false);
        }
        foreach (var obj in _classes)
        {
            obj.gameObject.SetActive(false);
        }
    }
}
