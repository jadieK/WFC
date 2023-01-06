using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class WFCSudoku : MonoBehaviour
{
    public TMP_Text LabelPrefab;
    public Canvas LabelCanvas;
    
    private const int N = 9;
    private const int GroupSize = 3;
    private const int _initStateCode = 0x1FF;
    private const int _labelWidth = 50;
    private const int _labelHeight = 50;
    private int[,] _blocks = new int[9, 9];
    private TMPro.TMP_Text[,] _lables = new TMP_Text[9, 9];
    private bool _startCollapse = false;

    private TMP_Text _lastLabel;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 10;
        for (int blockX = 0; blockX < N; blockX++)
        {
            for (int blockY = 0; blockY < N; blockY++)
            {
                _blocks[blockX, blockY] = _initStateCode;
                var label = Instantiate(LabelPrefab, LabelCanvas.transform);
                label.rectTransform.localPosition = new Vector3(-4 * _labelWidth + blockX * _labelWidth, -4 * _labelHeight + blockY * _labelHeight) ;
                _lables[blockX, blockY] = label;
            }
        }
    }

    public void OnCollapse()
    {
        _startCollapse = true;
    }

    private (int, int) FindLowestEntropy()
    {
        int x = -1;
        int y = -1;
        int lowest = int.MaxValue;
        
        for(int indexX = 0; indexX < N; indexX++)
        {
            for (int indexY = 0; indexY < N; indexY++)
            {
                if (_blocks[indexX, indexY] == 0)
                {
                    continue;
                }
                var entropy = CheckEntropy(_blocks[indexX, indexY]);
                if (entropy < lowest)
                {
                    x = indexX;
                    y = indexY;
                    lowest = entropy;
                }
            }
        }

        return (x, y);
    }

    private int Collapse(int x = -1, int y = -1)
    {
        if (x == -1 && y == -1)
        {
            x = Random.Range(0, N - 1);
            y = Random.Range(0, N - 1);
        }
        var result = GetRandomResult(_blocks[x, y]);
        Debug.Log($"Collapse ({x},{y}) to value {result}");
        _lables[x, y].text = result.ToString();
        _blocks[x, y] = 0;
        return 1 << (result - 1);
    }

    private List<int> GetAllNumbers(int stateCode)
    {
        List<int> allNumbers = new List<int>();
        int currentState = stateCode;
        while (currentState != 0)
        {
            int number = currentState & ~(currentState - 1);
            allNumbers.Add((int)Math.Log(number, 2) + 1);
            currentState &= ~number;
        }

        return allNumbers;
    }

    private int GetRandomResult(int stateCode)
    {
        var allNumbers = GetAllNumbers(stateCode);
        return allNumbers[Random.Range(0, allNumbers.Count - 1)];
    }

    private int GetNumberByTick(int stateCode, int tick)
    {
        var allNumbers = GetAllNumbers(stateCode);
        return allNumbers[tick % allNumbers.Count];
    }

    private void Propagation(int sourceX, int sourceY)
    {
        Propagation(sourceX, sourceY, _blocks[sourceX, sourceY]);
    }
    
    private void Propagation(int sourceX, int sourceY, int sourceStateCode)
    {
        for (int blockX = 0; blockX < N; blockX++)
        {
            if (sourceX != blockX)
            {
                PropagationCell(sourceX, sourceY, sourceStateCode, blockX, sourceY);
            }
        }

        for (int blockY = 0; blockY < N; blockY++)
        {
            if (sourceY != blockY)
            {
                PropagationCell(sourceX, sourceY, sourceStateCode, sourceX, blockY);
            }
        }

        int groupStartX = (sourceX / GroupSize) * GroupSize;
        int groupStartY = (sourceY / GroupSize) * GroupSize;
        for(int blockX = groupStartX; blockX < groupStartX + GroupSize; blockX++)
        {
            for (int blockY = groupStartY; blockY < groupStartY + GroupSize; blockY++)
            {
                if (blockX != sourceX && blockY != sourceY)
                {
                    PropagationCell(sourceX, sourceY, sourceStateCode, blockX, blockY);
                }
            }
        }
    }

    private bool PropagationCell(int sourceX, int sourceY, int sourceStateCode, int destX, int destY)
    {
        Debug.Log($"Propagate from ({sourceX},{sourceY}) to ({destX},{destY})");
        int destStateCode = _blocks[destX, destY];
        bool result = (sourceStateCode & destStateCode) != 0;
        if (result)
        {
            var temp = _blocks[destX, destY];
            _blocks[destX, destY] &= ~sourceStateCode;
            Debug.Log($"State code change from {temp} to {_blocks[destX,destY]}");
        }
        else
        {
            Debug.Log("Nothing changed");
        }

        return result;
    }

    private int CheckEntropy(int stateCode)
    {
        return CountSetBits(stateCode);
    }

    private int CountSetBits(int n)
    {
        if (n == 0)
        {
            return 0;
        }

        return 1 + CountSetBits(n & (n - 1));
    }

    private int _updateCount = 0;
    // Update is called once per frame
    void Update()
    {
        for (int blockX = 0; blockX < N; blockX++)
        {
            for (int blockY = 0; blockY < N; blockY++)
            {
                if (_blocks[blockX, blockY] != 0)
                {
                    _lables[blockX, blockY].text = GetNumberByTick(_blocks[blockX, blockY], _updateCount).ToString(); //GetRandomResult(_blocks[blockX, blockY]).ToString();    
                }
            }
        }

        _updateCount++;
        

        if (_startCollapse)
        {
            int nextX;
            int nextY;
            (nextX, nextY) = FindLowestEntropy();
            if (nextX == -1 && nextY == -1)
            {
                _startCollapse = false;
            }
            else
            {
                var code = Collapse(nextX, nextY);
                Propagation(nextX, nextY, code);
                if (_lastLabel != null)
                {
                    _lastLabel.color = Color.cyan;
                }
                _lables[nextX, nextY].color = Color.red;
                _lastLabel = _lables[nextX, nextY];
            }

            _startCollapse = false;

        }
    }
}
