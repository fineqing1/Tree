using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    [Tooltip("持久化场景引用")]
    public AssetReference persistentScene;
    [Tooltip("菜单场景引用")]
    public AssetReference menuScene;
    async void Start()
    {
        await menuScene.LoadSceneAsync().Task;
        await persistentScene.LoadSceneAsync(LoadSceneMode.Additive).Task;
    }
}