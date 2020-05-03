using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using TMPro;
using System.Linq;

public class TextMeshVFX : MonoBehaviour
{
    [SerializeField] Font baseFont = null;
    [SerializeField] TMP_FontAsset baseFontTmp = null;
    [SerializeField] TMP_InputField inputText = null;
    [SerializeField] PositionBaker baker = null;
    [SerializeField] PositionBaker preBaker = null;
    [SerializeField] VisualEffect effect;
    

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

    //#if true

    void Start()
    {
    }

    private void Update()
    {
        baseFont.RequestCharactersInTexture(currentText);
    }

    void OnDestroy()
    {
    }

    public void OnEditEnd(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;

        preText = currentText;
        currentText = str;


        baseFont.RequestCharactersInTexture(preText + currentText);

        var vertices = new Vector3[currentText.Length * 4];
        var pos = Vector3.zero;

        //フォントのテクスチャ
        //var tex = (Texture2D)baseFont.material.mainTexture;
        var tex = (Texture2D)baseFontTmp.atlas;


        //文字毎のテクスチャ
        var characterTexList = new List<Texture2D>();
        characterTexList = Enumerable.Repeat(new Texture2D(0, 0), currentText.Length).ToList();

        var basePosition = 0;
        List<Vector3> positionList = new List<Vector3>();

        //1文字ずつ確認
        for (int i = 0; i < currentText.Length; i++)
        {
            Vector2[] uv = new Vector2[4];
            CharacterInfo ch;
            baseFont.GetCharacterInfo(currentText[i], out ch);
            

            vertices[4 * i + 0] = pos + new Vector3(ch.minX, ch.maxY, 0);
            vertices[4 * i + 1] = pos + new Vector3(ch.maxX, ch.maxY, 0);
            vertices[4 * i + 2] = pos + new Vector3(ch.maxX, ch.minY, 0);
            vertices[4 * i + 3] = pos + new Vector3(ch.minX, ch.minY, 0);

            uv[0] = ch.uvTopLeft;
            uv[1] = ch.uvTopRight;
            uv[2] = ch.uvBottomRight;
            uv[3] = ch.uvBottomLeft;

            pos += new Vector3(ch.advance, 0, 0);

            var left = Mathf.FloorToInt(ch.uvBottomLeft.x * tex.width);
            var bottom = Mathf.FloorToInt(ch.uvBottomLeft.y * tex.height);
            var right = Mathf.FloorToInt(ch.uvTopRight.x * tex.width);
            var top = Mathf.FloorToInt(ch.uvTopRight.y * tex.height);
            Debug.Log("Left:" + left.ToString() + " Bottom:" + bottom + ToString() + " Right:" + right.ToString() + " Top:" + top.ToString());
            var dirX = right > left ? 1 : -1;
            var dirY = top > bottom ? 1 : -1;

            var wid = Mathf.Abs(right - left);
            var hei = Mathf.Abs(top - bottom);

            var copyTex = new Texture2D(tex.width, tex.height, tex.format, false);
            Graphics.CopyTexture(tex, copyTex);

            characterTexList[i] = new Texture2D(wid, hei,TextureFormat.ARGB32, false);
            
            Debug.Log("幅" + wid.ToString() + "," + "高さ" + hei.ToString());

            int min = 0;
            int max = 0;

            for (int j = 0; j < wid; j++)
            {
                for (int k = 0; k < hei; k++)
                {
                    var c = copyTex.GetPixel(left + (j * dirX), bottom + (k * dirY));
                    if (c.a > 0.01f)
                    {
                        var p = new Vector3(basePosition + j, k, 0.0f);
                        positionList.Add(p);

                        min = p.x < min || min == 0 ? basePosition + j : min;
                        max = p.x > max || max == 0 ? basePosition + j : max;
                    }

                    characterTexList[i].SetPixel(j, k, new Color(1.0f,1.0f,1.0f, c.a));
                }
            }

            characterTexList[i].Apply();

            if (min != 0 && max != 0)
                basePosition += ch.advance;
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

        //var maxHeight = positionList.Select(position => position.y).Max();

        //var tmpTexture = new Texture2D(renderWidth, renderHeight);

        //for (int i = 0; i < positionList.Count; i++)
        //{
        //    var p = new Vector3(
        //        positionList[i].x / basePosition,
        //        positionList[i].y / maxHeight,
        //        0.0f);

        //    var widthIdx = i / renderHeight % renderWidth;
        //    var heightIdx = i % renderHeight;
        //    Debug.Log(widthIdx.ToString() + " , " + heightIdx.ToString() + " : " + p.ToString("F2"));
        //    tmpTexture.SetPixel(widthIdx, heightIdx, new Color(p.x, p.y, 0.0f, 1.0f));
        //}

        //tmpTexture.Apply();
        //RenderTexture.active = positionMap;
        //Graphics.Blit(tmpTexture, positionMap);


        //var basePosition = 0;
        //newTextTexture = new Texture2D(renderWidth, renderHeight,TextureFormat.ARGB32 ,false);

        //Debug.Log("最大の高さ   " + maxHeight);

        ////TextureからRenderTextureへ変換
        //for (int i = 0; i < characterTexList.Count; i++)
        //{
        //    var c = characterTexList[i];

        //    for (int j = 0; j < c.width && (basePosition + j) < renderWidth; j++)
        //    {
        //        for (int k = 0; k < c.height && k < renderHeight; k++)
        //        {
        //            var color = c.GetPixel(j, k);


        //            var resultColor = Color.black;
        //            if (color.a > 0.01f)
        //            {
        //                resultColor = new Color((float)(basePosition + j) / renderWidth, (float)k / renderHeight, 0.0f);
        //                Debug.Log(resultColor);
        //            }

        //            newTextTexture.SetPixel(basePosition + j, k, resultColor);
        //        }
        //    }
        //    basePosition += c.width;
        //}
        //newTextTexture.Apply();
        //totalTextImage.texture = newTextTexture;

    }
}