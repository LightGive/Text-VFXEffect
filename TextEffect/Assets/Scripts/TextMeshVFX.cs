using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TextMeshVFX : MonoBehaviour
{
    [SerializeField]
    private Font baseFont;
    [SerializeField]
    private int resolution;

    [SerializeField]
    private TMP_InputField inputText;
    [SerializeField]
    private string activeText = "";
    [SerializeField]
    private RawImage[] rawImages;
    [SerializeField]
    private RenderTexture positionTex;
    [SerializeField]
    private RenderTexture oldTextRenderTex;
    [SerializeField]
    private RawImage totalTextImage;


    private Texture2D newTextTexture;
    private Texture2D oldTextTexture;

    private int renderWidth = 0;
    private int renderHeight = 0;

    //#if true

    void Start()
    {
        if (positionTex == null || 
            oldTextRenderTex == null || 
            positionTex.width != oldTextRenderTex.width || 
            positionTex.height != oldTextRenderTex.height)
        {
            Destroy(gameObject);
            return;
        }

        renderWidth = positionTex.width;
        renderHeight = positionTex.height;
    }

    private void Update()
    {
        baseFont.RequestCharactersInTexture(activeText);
    }

    void OnDestroy()
    {
    }

    void OnFontTextureRebuilt(Font changedFont)
    {
        Debug.Log("Rebuild");
        if (changedFont != baseFont)
            return;

        RebuildMesh();
    }

    void RebuildMesh()
    {
        Debug.Log("BuildMesh");

    }

    //#endif

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

            for (int j = 0; j < wid; j++)
            {
                for (int k = 0; k < hei; k++)
                {
                    var c = copyTex.GetPixel(left + (j * dirX), bottom + (k * dirY));
                    if (c.a > 0.01f)
                    {
                        positionList.Add(
                            new Vector3(
                                (float)(basePosition + j) / renderWidth,
                                (float)k / renderHeight,
                                0.0f));
                    }
                    
                    characterTexList[i].SetPixel(j, k, new Color(1.0f,1.0f,1.0f, c.a));
                }
            }

            characterTexList[i].Apply();
            rawImages[i].texture = characterTexList[i];

            basePosition += tex.width;
        }

        var tmpTexture = new Texture2D(renderWidth, renderHeight);
        for (int i = 0; i < positionList.Count; i++)
        {
            var p = positionList[i];
            Debug.Log(p);
 
            var widthIdx = (i / renderHeight) % renderWidth;
            var heightIdx = i % renderHeight;
            Debug.Log(widthIdx.ToString()+" , "+heightIdx.ToString());
            tmpTexture.SetPixel(widthIdx, heightIdx, new Color(p.x, p.y, 0.0f, 1.0f));
        }

        tmpTexture.Apply();
        RenderTexture.active = positionTex;
        Graphics.Blit(tmpTexture, positionTex);



        //var basePosition = 0;
        //var maxHeight = characterTexList.Select(x => x.height).Max();
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