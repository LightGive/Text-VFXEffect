using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextMeshVFX : MonoBehaviour
{
    [SerializeField]
    private Font baseFont;
    [SerializeField]
    private int resolution;

    private string activeText = "";

    [SerializeField]
    private Texture testTex;

    public RenderTexture NewTextTex;
    public RenderTexture OldTextTex;

//#if true

    void Start()
    {
        //baseFont = Font.CreateDynamicFontFromOSFont("Helvetica", 64);
        Font.textureRebuilt += OnFontTextureRebuilt;

        RebuildMesh();
    }

    private void Update()
    {
        baseFont.RequestCharactersInTexture(activeText);
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
        var vertices = new Vector3[activeText.Length * 4];
        var uv = new Vector2[activeText.Length * 4];
        var pos = Vector3.zero;

        for (int i = 0; i < activeText.Length; i++)
        {
            CharacterInfo ch;
            baseFont.GetCharacterInfo(activeText[i], out ch);

            vertices[4 * i + 0] = pos + new Vector3(ch.minX, ch.maxY, 0);
            vertices[4 * i + 1] = pos + new Vector3(ch.maxX, ch.maxY, 0);
            vertices[4 * i + 2] = pos + new Vector3(ch.maxX, ch.minY, 0);
            vertices[4 * i + 3] = pos + new Vector3(ch.minX, ch.minY, 0);

            uv[4 * i + 0] = ch.uvTopLeft;
            uv[4 * i + 1] = ch.uvTopRight;
            uv[4 * i + 2] = ch.uvBottomRight;
            uv[4 * i + 3] = ch.uvBottomLeft;

            pos += new Vector3(ch.advance, 0, 0);

            var texSize = baseFont.material.mainTexture.texelSize;
            if(texSize!= testTex.texelSize)
            {
                testTex = new Texture2D((int)texSize.x, (int)texSize.y);
            }
            Graphics.CopyTexture(baseFont.material.mainTexture, testTex);
        }
    }

//#endif

    public void OnEditEnd(string str)
    {
        activeText = str;
        baseFont.RequestCharactersInTexture(str);
        Graphics.CopyTexture(baseFont.material.mainTexture, testTex);

    }
}
