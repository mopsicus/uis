using System.Collections.Generic;
using TMPro;
using UIS;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Demo 4
/// </summary>
public class Demo6 : MonoBehaviour {

    /// <summary>
    /// Link to list
    /// </summary>
    [SerializeField]
    Scroller List = null;

    /// <summary>
    /// Link to input item
    /// </summary>
    [SerializeField]
    TMP_InputField ItemInput = null;

    /// <summary>
    /// Items count
    /// </summary>
    [SerializeField]
    int Count = 100;

    /// <summary>
    /// Items data container
    /// </summary>
    readonly List<int> _list = new List<int>();

    /// <summary>
    /// Heights data container
    /// </summary>
    readonly List<int> _heights = new List<int>();

    /// <summary>
    /// Init
    /// </summary>
    void Start() {
        List.OnFill += OnFillItem;
        List.OnHeight += OnHeightItem;
        for (var i = 0; i < Count; i++) {
            _list.Add(i);
            _heights.Add(Random.Range(100, 200));
        }
        List.InitData(_list.Count);
    }

    /// <summary>
    /// Callback on fill item
    /// </summary>
    /// <param name="index">Item index</param>
    /// <param name="item">Item object</param>
    void OnFillItem(int index, GameObject item) {
        item.GetComponentInChildren<TextMeshProUGUI>().text = _list[index].ToString();
    }

    /// <summary>
    /// Callback on request item height
    /// </summary>
    /// <param name="index">Item index</param>
    /// <returns>Current item height</returns>
    int OnHeightItem(int index) {
        return _heights[index];
    }

    /// <summary>
    /// Move to item index
    /// </summary>
    public void ScrollTo() {
        int.TryParse(ItemInput.text, out var index);
        List.ScrollTo(index);
    }

    /// <summary>
    /// Load next demo scene
    /// </summary>
    /// <param name="index">Scene index</param>
    public void SceneLoad(int index) {
        SceneManager.LoadScene(index);
    }
}