using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vector3のリストをRenderTextureに焼きこむ
/// </summary>
public class PositionBaker : MonoBehaviour
{
    [SerializeField] ComputeShader compute = null;
    [SerializeField] RenderTexture bakeMap = null;

    RenderTexture tmpMap;
    ComputeBuffer buf;
    int positionCount = 0;
    
    public RenderTexture BakeMap => bakeMap;

    private void OnDestroy()
    {
        if (buf != null)
        {
            buf.Dispose();
            buf = null;
        }

        if (tmpMap != null)
        {
            Destroy(tmpMap);
            tmpMap = null;
        }
    }

    public void BakePositionMap(List<Vector3> posList, Transform target)
    {
        if (posList == null || posList.Count == 0)
            return;

        positionCount = posList.Count * 3;
        for (int i = positionCount; i+3 < bakeMap.width * bakeMap.height; i += 3)
        {
            posList.Add(posList[i / 3 % (positionCount / 3)]);
        }
        positionCount = posList.Count * 3;

        if (CheckConsistency())
            return;

        if (positionCount > bakeMap.width * bakeMap.height)
        {
            Debug.LogError("リストの要素が多すぎます。\n要素数を少なくするかベイクするテクスチャサイズを大きくしてください。");
            return;
        }

        int kernel = compute.FindKernel("PositionData");
        buf.SetData(posList);

        compute.SetMatrix("Transform", target.localToWorldMatrix);
        compute.SetInt("PositionCount", posList.Count);
        compute.SetBuffer(kernel, "PositionBuffer", buf);
        compute.SetTexture(kernel, "PositionMap", tmpMap);
        compute.Dispatch(kernel, bakeMap.width / 8, bakeMap.height / 8, 1);

        Graphics.CopyTexture(tmpMap, bakeMap);
    }

    bool CheckConsistency()
    {
        if(!bakeMap)
        {
            Debug.LogError("Position情報を焼きこむマップを作成して下さい");
            return true;
        }

        if (bakeMap.width % 8 != 0 || bakeMap.height % 8 != 0)
        {
            Debug.LogError("RenderTextureのサイズは8の倍数にして下さい");
            return true;
        }

        if (bakeMap.format != RenderTextureFormat.ARGBFloat)
        {
            Debug.LogError("RenderTextureのフォーマットはARGBFloatにして下さい");
            return true;
        }

        if (tmpMap == null ||
            tmpMap.width != bakeMap.width ||
            tmpMap.height != bakeMap.height)
        {
            tmpMap = new RenderTexture(bakeMap.width, bakeMap.height, 0, bakeMap.format);
            tmpMap.enableRandomWrite = true;
            tmpMap.Create();
        }

        if(buf!= null)
        {
            buf.Dispose();
            buf = null;
        }
        buf = new ComputeBuffer(positionCount, sizeof(float));

        return false;
    }
}