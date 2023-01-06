using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WFCFlower : MonoBehaviour
{
    public Texture2D Flower;
    public RawImage Canvas;
    public int CanvasWidth = 10;
    public int CanvasHeight = 10;
    private int N = 2;
    private int _codeMask = 0xF; //1111 for 15 color per pixel, max 8 pixel per block

    private List<Color> _colorsEncoder = new List<Color>();
    private List<int> _encodedBlocks = new List<int>();

    private List<int>[,] _blocks;
    
    private enum Direction
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    // Start is called before the first frame update
    void Start()
    {
        _colorsEncoder.Clear();
        for (int x = 0; x < Flower.width - 1; x++)
        {
            for (int y = 0; y < Flower.height - 1; y++)
            {
                Color[] currentColor = Flower.GetPixels(x, y, N, N);
                int encodedBlock = EncodeBlock(currentColor);
                if (!_encodedBlocks.Contains(encodedBlock))
                {
                    _encodedBlocks.Add(encodedBlock);
                    Debug.Log(encodedBlock);
                }
            }
        }

        _blocks = new List<int>[CanvasWidth,CanvasHeight];
        for (int x = 0; x < CanvasWidth; x++)
        {
            for (int y = 0; y < CanvasHeight; y++)
            {
                _blocks[x,y] = new List<int>(_encodedBlocks);    
            }
        }
        
        Texture2D canvasTexture = new Texture2D(CanvasWidth * N, CanvasHeight * N, TextureFormat.RGBA32, false);
        Canvas.rectTransform.sizeDelta = new Vector2(CanvasWidth * N, CanvasHeight * N);
        Canvas.texture = canvasTexture;
    }

    private int EncodeBlock(Color[] blockColors)
    {
        int encodeResult = 0;
        if (blockColors.Length != N * N)
        {
            Debug.LogError("Wrong block size");
            return -1;
        }

        for (int index = 0; index < N * N; index++)
        {
            int encoderIndex = _colorsEncoder.FindIndex(x => x == blockColors[index]);
            if (encoderIndex == -1)
            {
                _colorsEncoder.Add(blockColors[index]);
                encoderIndex = _colorsEncoder.Count;
            }

            encodeResult |= (encoderIndex & _codeMask) << (index * (N*N));
        }

        return encodeResult;
    }

    private Color[] DecodeBlock(int blockCode)
    {
        if (blockCode == -1)
        {
            Debug.LogError("Wrong block code");
            return null;
        }

        Color[] decodeColors = new Color[N * N];
        for (int index = 0; index < N * N; index++)
        {
            decodeColors[index] = _colorsEncoder[(blockCode >> (index * (N * N))) & _codeMask];
        }

        return decodeColors;
    }

    private (int,int) FindLowestEntropy()
    {
        int posX = -1;
        int posY=-1;
        int lowestEntropy=0;
        
        for (int x = 0; x < CanvasWidth; x++)
        {
            for (int y = 0; y < CanvasHeight; y++)
            {
                if (_blocks[x, y].Count != 0 && _blocks[x,y].Count > lowestEntropy)
                {
                    lowestEntropy = _blocks[x, y].Count;
                    posX = x;
                    posY = y;
                }
            }
        }
        return (posX,posY);
    }

    private void Observe(int x = -1, int y = -1)
    {
        if (x == -1 && y == -1)
        {
            x = Random.Range(0, CanvasWidth - 1);
            y = Random.Range(0, CanvasHeight - 1);
        }

        int targetCode = Random.Range(0, _blocks[x, y].Count - 1);
        DrawBlock(x,y,_blocks[x, y][targetCode]);
        _blocks[x,y].Clear();
    }

    private void Propagation(List<int> sourceCodes, List<int> destCodes, Direction from)
    {
        switch (from)
        {
            
        }
    }

    private void DrawBlock(int x, int y, int code)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
