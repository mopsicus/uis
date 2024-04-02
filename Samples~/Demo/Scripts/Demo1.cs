using TMPro;
using UIS;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Demo 1
/// </summary>
public class Demo1 : MonoBehaviour {

    /// <summary>
    /// Link to list
    /// </summary>
    [SerializeField]
    Scroller List = null;

    /// <summary>
    /// Items count
    /// </summary>
    [SerializeField]
    int Count = 100;

    /// <summary>
    /// Init
    /// </summary>
    void Start() {
        List.OnFill += OnFillItem;
        List.OnHeight += OnHeightItem;
        List.InitData(Count);
    }

    /// <summary>
    /// Callback on fill item
    /// </summary>
    /// <param name="index">Item index</param>
    /// <param name="item">Item object</param>
    void OnFillItem(int index, GameObject item) {
        item.GetComponentInChildren<TextMeshProUGUI>().text = index.ToString();
    }

    /// <summary>
    /// Callback on request item height
    /// </summary>
    /// <param name="index">Item index</param>
    /// <returns>Current item height</returns>
    int OnHeightItem(int index) {
        return 150;
    }

    /// <summary>
    /// Load next demo scene
    /// </summary>
    /// <param name="index">Scene index</param>
    public void SceneLoad(int index) {
        SceneManager.LoadScene(index);
    }
}