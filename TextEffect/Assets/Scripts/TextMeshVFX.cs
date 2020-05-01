using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TextMeshVFX : MonoBehaviour
{
    [SerializeField] Font baseFont = null;
    [SerializeField] ComputeShader compute = null;
    [SerializeField] TMP_InputField inputText = null;
    
    [SerializeField] RenderTexture positionMap;
    [SerializeField] RenderTexture positionPrevMap;

    [SerializeField] string activeText = "";

    private RenderTexture tmpPositionMap;
    private ComputeBuffer buffer;
    private Texture2D newTextTexture;
    private Texture2D oldTextTexture;

    private int renderWidth = 0;
    private int renderHeight = 0;

    //#if true

    void Start()
    {
        if (positionMap == null || 
            positionPrevMap == null || 
            positionMap.width != positionPrevMap.width || 
            positionMap.height != positionPrevMap.height)
        {
            Destroy(gameObject);
            return;
        }

        int kernel = compute.FindKernel("PositionData");
        Debug.Log("カーネル" + kernel);
        compute.Dispatch(kernel, 1, 1, 1);
        Debug.Log("カーネル" + kernel);

        renderWidth = positionMap.width;
        renderHeight = positionMap.height;
    }

    private void Update()
    {
        baseFont.RequestCharactersInTexture(activeText);
    }

    void OnDestroy()
    {
    }
    
    /// <summary>
    /// ベイクする前のチェック
    /// </summary>
    void BakeCheck()
    {

    }


    public void OnEditEnd(string str)
    {
        activeText = str;
        baseFont.RequestCharactersInTexture(str);

        var vertices = new Vector3[activeText.Length * 4];
        var pos = Vector3.zero;

        //フォントのテクスチャ
        var tex = (Texture2D)baseFont.material.mainTexture;

        //文字毎のテクスチャ
        var characterTexList = new List<Texture2D>();
        characterTexList = Enumerable.Repeat(new Texture2D(0, 0), activeText.Length).ToList();


        var basePosition = 0;
        List<Vector3> positionList = new List<Vector3>();

        //1文字ずつ確認
        for (int i = 0; i < activeText.Length; i++)
        {
            Vector2[] uv = new Vector2[4];
            CharacterInfo ch;
            baseFont.GetCharacterInfo(activeText[i], out ch);

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
            var right = Mathf.FloorToInt(ch.uvBottomRight.x * tex.width);
            var top = Mathf.FloorToInt(ch.uvTopRight.y * tex.height);

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
                //Debug.Log(basePosition);

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

        var maxHeight = positionList.Select(position => position.y).Max();

        var tmpTexture = new Texture2D(renderWidth, renderHeight);

        for (int i = 0; i < positionList.Count; i++)
        {
            var p = new Vector3(
                positionList[i].x / basePosition,
                positionList[i].y / maxHeight,
                0.0f);

            var widthIdx = i / renderHeight % renderWidth;
            var heightIdx = i % renderHeight;
            Debug.Log(widthIdx.ToString() + " , " + heightIdx.ToString() + " : " + p.ToString("F2"));
            tmpTexture.SetPixel(widthIdx, heightIdx, new Color(p.x, p.y, 0.0f, 1.0f));
        }

        tmpTexture.Apply();
        RenderTexture.active = positionMap;
        Graphics.Blit(tmpTexture, positionMap);


        if (buffer == null)
        {
            buffer = new ComputeBuffer(positionList.Count * 3, sizeof(float));
        }

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