using System.Collections.Generic;
using Mopsicus.InfiniteScroll;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Demo3 : MonoBehaviour {

	[SerializeField]
	private InfiniteScroll Scroll;

	[SerializeField]
	private int Count = 100;

	[SerializeField]
	private int PullCount = 25;

	private List<int> _list = new List<int> ();

	private List<int> _heights = new List<int> ();

	void Start () {
		Scroll.OnFill += OnFillItem;
		Scroll.OnHeight += OnHeightItem;
		Scroll.OnPull += OnPullItem;
		for (int i = 0; i < Count; i++) {
			_list.Add (i);
			_heights.Add (Random.Range (100, 200));
		}
		Scroll.InitData (_list.Count);
	}

	void OnFillItem (int index, GameObject item) {
		item.GetComponentInChildren<Text> ().text = _list[index].ToString ();
	}

	int OnHeightItem (int index) {
		return _heights[index];
	}

	void OnPullItem (InfiniteScroll.Direction direction) {
		int index = _list.Count;
		if (direction == InfiniteScroll.Direction.Top) {
			for (int i = 0; i < PullCount; i++) {
				_list.Insert (0, index);
				_heights.Insert (0, Random.Range (100, 200));
				index++;
			}
		} else {
			for (int i = 0; i < PullCount; i++) {
				_list.Add (index);
				_heights.Add (Random.Range (100, 200));
				index++;
			}
		}
		Scroll.ApplyDataTo (_list.Count, PullCount, direction);
	}

	public void SceneLoad (int index) {
		SceneManager.LoadScene (index);
	}

}