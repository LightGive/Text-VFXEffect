using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionBaker : MonoBehaviour
{
    [SerializeField] ComputeShader compute = null;
    [SerializeField] RenderTexture bakeMap = null;

    RenderTexture tmpMap;
    int positionCount = 0;
    ComputeBuffer buf;

    public RenderTexture BakeMap => bakeMap;

    private void Start()
    {
        
    }

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
        Debug.Log("座標数" + positionCount.ToString() + " ,幅と高さ" + bakeMap.width * bakeMap.height);

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
        compute.SetInt("PositionCount", positionCount);
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

        if(bakeMap.format != RenderTextureFormat.ARGBFloat)
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