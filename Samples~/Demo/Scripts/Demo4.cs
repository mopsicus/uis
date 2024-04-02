using System.Collections.Generic;
using TMPro;
using UIS;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Demo 4
/// </summary>
public class Demo4 : MonoBehaviour {

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
    /// Count to add after pull
    /// </summary>
    [SerializeField]
    int PullCount = 25;

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
        List.OnPull += OnPullItem;
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
    /// Callback after pull
    /// </summary>
    /// <param name="direction">Director pulled from</param>
    void OnPullItem(ScrollerDirection direction) {
        var index = _list.Count;
        if (direction == ScrollerDirection.Top) {
            for (var i = 0; i < PullCount; i++) {
                _list.Insert(0, index);
                _heights.Insert(0, Random.Range(100, 200));
                index++;
            }
        } else {
            for (var i = 0; i < PullCount; i++) {
                _list.Add(index);
                _heights.Add(Random.Range(100, 200));
                index++;
            }
        }
        List.ApplyDataTo(_list.Count, PullCount, direction);
    }

    /// <summary>
    /// Remove item from list
    /// </summary>
    /// <param name="index">Item index</param>
    public void RemoveItem(int index) {
        _list.RemoveAt(index);
        _heights.RemoveAt(index);
        List.Recycle(index);
    }

    /// <summary>
    /// Clear list
    /// </summary>
    public void RemoveAll() {
        List.RecycleAll();
        _list.Clear();
        _heights.Clear();
    }

    /// <summary>
    /// Load next demo scene
    /// </summary>
    /// <param name="index">Scene index</param>
    public void SceneLoad(int index) {
        SceneManager.LoadScene(index);
    }
}