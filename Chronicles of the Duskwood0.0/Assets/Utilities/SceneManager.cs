using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;
using DG.Tweening;
using UnityEngine.UI;

public class ScenesManager : MonoBehaviour
{
    [Tooltip("过渡遮罩图片")]
    public RectTransform sprite;
    private Image fadeImage;
    
    [Header("过渡设置")]
    [Tooltip("淡入淡出持续时间")]
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("淡入淡出颜色")]
    [SerializeField] private Color fadeColor = Color.black;
    private bool isTransitioning = false;
    
    void Awake()
    {
        if (sprite != null)
        {
            fadeImage = sprite.GetComponent<Image>();
            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = 0f;
                fadeImage.color = color;
                fadeImage.gameObject.SetActive(true);
            }
        }
    }
    
    private async Task LoadSceneAsync(AssetReference scene)
    {
        string sceneName = await GetSceneName(scene);
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.Log("已经存在" + sceneName);
            return;
        }

        AsyncOperationHandle<SceneInstance> s = Addressables.LoadSceneAsync(scene, LoadSceneMode.Additive);
        await s.Task;

        if (s.Status == AsyncOperationStatus.Succeeded)
        {
            SceneManager.SetActiveScene(s.Result.Scene);
        }
    }

    private Task UnloadSceneTask()
    {
        var op = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        if (op == null || op.isDone)
            return Task.CompletedTask;
        var tcs = new TaskCompletionSource<bool>();
        op.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }

    private async Task<string> GetSceneName(AssetReference sceneRef)
    {
#if UNITY_EDITOR
        if (sceneRef.editorAsset != null)
            return sceneRef.editorAsset.name;
#endif
        var loadOp = Addressables.LoadAssetAsync<Object>(sceneRef);
        await loadOp.Task;
        return loadOp.Status == AsyncOperationStatus.Succeeded ? loadOp.Result.name : "";
    }

    public async void LoadSceneEvent(object data)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        
        Time.timeScale = 1f;
        
        await TransitionRoutine(async () => {
            await UnloadSceneTask();
            await LoadSceneAsync(data as AssetReference);
        });
        isTransitioning = false;
    }
    
    private async Task TransitionRoutine(System.Func<Task> sceneAction)
    {
        if (fadeImage == null) return;
        
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        await fadeImage.DOColor(new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f), fadeDuration)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
            
        await sceneAction?.Invoke();
        await Task.Yield();
        
        await fadeImage.DOColor(new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f), fadeDuration)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }
    
    public async void QuickFade()
    {
        if (fadeImage == null) return;
        
        await fadeImage.DOColor(new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f), 0.2f)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();
        
        await fadeImage.DOColor(new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f), 0.2f)
            .SetEase(Ease.InQuad)
            .AsyncWaitForCompletion();
    }
    void OnDestroy()
    {
        if (fadeImage != null)
        {
            fadeImage.DOKill();
        }
    }
}