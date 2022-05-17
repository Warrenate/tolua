using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaInterface;
using System;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#endif

//click Lua/Build lua bundle
public class TestABLoaderPB3 : LuaClient
{
    int bundleCount = int.MaxValue;
    string tips = null;

    IEnumerator CoLoadBundle(string name, string path)
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
        yield return request;

        --bundleCount;
        LuaFileUtils.Instance.AddSearchBundle(name, request.assetBundle);
    }

    IEnumerator LoadFinished()
    {
        while (bundleCount > 0)
        {
            yield return null;
        }

        OnBundleLoad();
    }

    public IEnumerator LoadBundles()
    {
        string streamingPath = Application.streamingAssetsPath.Replace('\\', '/');
        string dir = streamingPath + "/" + LuaConst.osDir;

#if UNITY_EDITOR
        if (!Directory.Exists(dir))
        {
            throw new Exception("must build bundle files first");
        }
#endif

        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(dir + "/" + LuaConst.osDir);
        yield return request;

        AssetBundleManifest manifest = (AssetBundleManifest)request.assetBundle.LoadAsset("AssetBundleManifest");
        List<string> list = new List<string>(manifest.GetAllAssetBundles());

        bundleCount = list.Count;

        for (int i = 0; i < list.Count; i++)
        {
            string str = list[i];
            string path = streamingPath + "/" + LuaConst.osDir + "/" + str;
            string name = Path.GetFileNameWithoutExtension(str);
            StartCoroutine(CoLoadBundle(name, path));
        }

        yield return StartCoroutine(LoadFinished());
    }

    void Awake()
    {
        Instance = this;
        Application.logMessageReceived += ShowTips;

        LuaFileUtils file = new LuaFileUtils();
        file.beZip = true;
#if UNITY_ANDROID && UNITY_EDITOR
        if (IntPtr.Size == 8)
        {
            throw new Exception(
                "can't run this on standalone 64 bits, switch to pc platform, or run it in android mobile");
        }
#endif

        StartCoroutine(LoadBundles());
    }

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        tips += msg;
        tips += "\r\n";
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 150, 400, 300), tips);
    }

    void OnApplicationQuit()
    {
        Application.logMessageReceived -= ShowTips;
    }

    void OnBundleLoad()
    {
        Init();
    }
}