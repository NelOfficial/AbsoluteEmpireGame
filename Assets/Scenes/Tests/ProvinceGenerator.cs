using System.Collections.Generic;
using UnityEngine;

public class ProvinceGenerator : MonoBehaviour
{
    public Texture2D mapTexture;  // Текстура карты с провинциями
    public GameObject provincePrefab;  // Префаб для создания провинций
    public Color backgroundColor = Color.black;  // Цвет фона, который нужно игнорировать

    private Dictionary<Color, List<Vector2>> provincePixels = new Dictionary<Color, List<Vector2>>();

    void Start()
    {
        GenerateProvinces();
    }

    void GenerateProvinces()
    {
        // Собираем пиксели для каждой провинции
        for (int x = 0; x < mapTexture.width; x++)
        {
            for (int y = 0; y < mapTexture.height; y++)
            {
                Color pixelColor = mapTexture.GetPixel(x, y);

                if (pixelColor == backgroundColor || pixelColor.a == 0)
                    continue;

                if (!provincePixels.ContainsKey(pixelColor))
                {
                    provincePixels[pixelColor] = new List<Vector2>();
                }

                provincePixels[pixelColor].Add(new Vector2(x, y));
            }
        }

        foreach (var province in provincePixels)
        {
            Color provinceColor = province.Key;
            List<Vector2> pixels = province.Value;

            Rect provinceRect = CalculateProvinceBounds(pixels);

            Texture2D provinceTexture = CreateProvinceTexture(mapTexture, pixels, (int)provinceRect.width + 1, (int)provinceRect.height + 1, provinceRect);

            GameObject newProvince = Instantiate(provincePrefab);
            newProvince.name = $"Province {provinceColor}";
            newProvince.transform.position = new Vector3(provinceRect.x + provinceRect.width / 2, provinceRect.y + provinceRect.height / 2, 0);

            SpriteRenderer sr = newProvince.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = Sprite.Create(provinceTexture, new Rect(0, 0, provinceTexture.width, provinceTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                sr.color = Color.white;
            }
        }
    }

    Rect CalculateProvinceBounds(List<Vector2> pixels)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Vector2 pixel in pixels)
        {
            if (pixel.x < minX) minX = pixel.x;
            if (pixel.x > maxX) maxX = pixel.x;
            if (pixel.y < minY) minY = pixel.y;
            if (pixel.y > maxY) maxY = pixel.y;
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    Texture2D CreateProvinceTexture(Texture2D originalTexture, List<Vector2> pixels, int width, int height, Rect bounds)
    {
        Texture2D provinceTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color[] transparentPixels = new Color[width * height];
        for (int i = 0; i < transparentPixels.Length; i++)
        {
            transparentPixels[i] = new Color(0, 0, 0, 0);
        }
        provinceTexture.SetPixels(transparentPixels);

        foreach (Vector2 pixel in pixels)
        {
            int x = (int)(pixel.x - bounds.x);
            int y = (int)(pixel.y - bounds.y);

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                provinceTexture.SetPixel(x, y, originalTexture.GetPixel((int)pixel.x, (int)pixel.y));
            }
        }

        provinceTexture.Apply();
        return provinceTexture;
    }
}
