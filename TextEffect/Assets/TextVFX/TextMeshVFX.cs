using System.Collections;
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
    [SerializeField] PositionBaker preBaker = null;
    [SerializeField] VisualEffect effect;
    [SerializeField] RawImage rawImag;

    string currentText = "";
    string preText = "";
    Vector2 currentSize;
    Vector2 preSize;

    private RenderTexture tmpPositionMap;
    private ComputeBuffer buffer;
    private Texture2D newTextTexture;
    private Texture2D oldTextTexture;

    private int renderWidth = 0;
    private int renderHeight = 0;

    public void OnEditEnd(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        preText = currentText;
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

        byte[] data = System.Text.Encoding.Unicode.GetBytes(str);


        //1文字ずつ確認
        for (int i = 0; i < currentText.Length; i++)
        {
            var findCharaList = baseFontTmp.characterTable
                .Select((x, idx) => new { Index = idx, Content = x })
                .Where(x => (int)x.Content.unicode == data[i])
                .Select(x => x.Index);

            Debug.Log("Index"+findCharaList.First().ToString("0"));

            if (findCharaList.Count() <= 0)
                continue;
            else if(findCharaList.Count() >1)
            {
                Debug.LogError("対象の文字データが複数存在する");
            }
            // **Debug
            //string debugStr = "";
            //foreach (var a in baseFontTmp.characterTable)
            //    debugStr += a.unicode.ToString("0") + " " + a.glyphIndex.ToString("0") + ",    ";
            //Debug.Log(debugStr);

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

        Debug.Log(positionList.Count);

        var maxHeight = positionList.Select(position => position.y).Max();
        var maxWidth = basePosition;

        preSize = currentSize;
        currentSize = new Vector2(maxWidth / maxHeight,1.0f);

        effect.SetVector2("PreSize", preSize);
        effect.SetVector2("CurrentSize", currentSize);

        for (int i = 0; i < positionList.Count; i++)
        {
            positionList[i] = new Vector3(positionList[i].x / maxWidth, positionList[i].y / maxHeight, 0.0f);
        }
        
        var log = "";
        positionList.ForEach(x => log += x.ToString("F2")+"\n");
        Debug.Log(log);

        Graphics.CopyTexture(baker.BakeMap, preBaker.BakeMap);
        baker.BakePositionMap(positionList, effect.transform);


    }
}