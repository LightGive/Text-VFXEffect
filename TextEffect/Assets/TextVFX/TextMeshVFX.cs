using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using TMPro;
using System.Linq;
using System;

public class TextMeshVFX : MonoBehaviour
{
    [SerializeField] TMP_FontAsset baseFontTmp = null;
    [SerializeField] TMP_InputField inputText = null;
    [SerializeField] PositionBaker baker = null;
    [SerializeField] VisualEffect effect;
    [SerializeField] RawImage rawImag;

    string currentText = "";
    Vector2 currentSize;

    public void OnEditEnd(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        currentText = str;

        //フォントのテクスチャ
        var tex = (Texture2D)baseFontTmp.material.mainTexture;

        //文字毎のテクスチャ
        var characterTexList = new List<Texture2D>();
        characterTexList = Enumerable.Repeat(new Texture2D(0, 0), currentText.Length).ToList();

        var basePosition = 0;
        List<Vector3> positionList = new List<Vector3>();

        if(!baseFontTmp.HasCharacters(str))
        {
            Debug.Log("フォントに無い文字があります");
            return;
        }

        //1文字ずつ確認
        for (int i = 0; i < currentText.Length; i++)
        {
            var data = Convert.ToInt32(currentText[i]);
            var findCharaList = baseFontTmp.characterTable
                .Select((x, idx) => new { Index = idx, Content = x })
                .Where(x => (int)x.Content.unicode == data)
                .Select(x => x.Index);

            if (findCharaList.Count() <= 0)
            {
                Debug.LogError("対象の文字データが存在しない");
                continue;
            }
            else if (findCharaList.Count() > 1)
            {
                Debug.LogError("対象の文字データが複数存在する");
                continue;
            }

            var rec = baseFontTmp.characterTable[findCharaList.First()].glyph.glyphRect;
            characterTexList[i] = new Texture2D(rec.width, rec.height, TextureFormat.ARGB32, false);

            for (int j = 0; j < rec.width; j++)
            {
                for (int k = 0; k < rec.height; k++)
                {
                    Color c = Color.black;
                    var pixelColor = baseFontTmp.atlasTexture.GetPixel(rec.x + j, rec.y + k);

                    if (pixelColor.a > 0.5f)
                    {
                        c = Color.white;
                        var p = new Vector3(basePosition + j, k, 0.0f);
                        positionList.Add(p);
                    }
                    characterTexList[i].SetPixel(j, k, c);
                }
            }
            characterTexList[i].Apply();

            basePosition += rec.width;
            rawImag.texture = characterTexList[i];
        }

        if (positionList.Count == 0)
        {
            Debug.Log("Count...0");
            return;
        }

        var maxHeight = positionList.Select(position => position.y).Max();
        var maxWidth = basePosition;

        currentSize = new Vector2(maxWidth / maxHeight,1.0f);
        effect.SetVector2("CurrentSize", currentSize);

        for (int i = 0; i < positionList.Count; i++)
        {
            positionList[i] = new Vector3(positionList[i].x / maxWidth, positionList[i].y / maxHeight, 0.0f);
        }
 
        baker.BakePositionMap(positionList, effect.transform);
    }
}