# UIS documentation

UIS aka Unity infinite scroller is an extension allows you to use the `ScrollRect` control as an infinite scroller/spinner. 

## Introduction

UIS uses a data-driven method to scroll and display list items. When items on one side disappear, they are moved and displayed on the other side. This saves a lot of memory and is very fast.

## Setup

1. Add `UIS` to uses section
2. Add `Scroll View` component to game object on UI canvas
3. Add `Scroller` script after it
4. Add handlers to your code
5. Init list

```charp
using UIS;
using UnityEngine;

public class Demo : MonoBehaviour {

    /// <summary>
    /// Link to list
    /// </summary>
    [SerializeField]
    Scroller List = null;

    /// <summary>
    /// Init list
    /// </summary>
    void Start() {
        List.OnFill += OnFillItem;
        List.OnHeight += OnHeightItem;
        List.OnWidth += OnWidthItem; // for horizontal scroller
        List.InitData(100);
    }

    /// <summary>
    /// Callback on fill item
    /// </summary>
    /// <param name="index">Item index</param>
    /// <param name="item">Item object</param>
    void OnFillItem(int index, GameObject item) {
        ...
    }

    /// <summary>
    /// Callback on request item height
    /// </summary>
    /// <param name="index">Item index</param>
    /// <returns>Current item height</returns>
    int OnHeightItem(int index) {
        ...
    }
}
```

When you call `InitData`, UIS creates list items from the prefab and displays them. If you place the game objects in the container yourself, UIS will not create any more list items.

The second parameter of `InitData` method determines on which side the list will be initiated.

## Customization in the inspector

`Prefab` – link to list item prefab

`Padding` – padding for list: top/bottom and left/right

`Spacing` – spacing between list items

`Labels` – customize labels for pull-to-refresh

`Pull directions` – switch on/ff pull-to-refresh function for each side

`Offsets` – customize pull-to-refresh labels 
offsets and pull value for drag

`Parent container` – you can set container size by another game object with RectTransform

`AddonViewsCount` – views count which created on initialization to make gap for scrolling

## Recycle

UIS doesn't destroy or create list items each time. When you did the first initialization, UIS created the pool of elements and uses them for scrolling. And when you call `Recycle(int index)` or `RecycleAll()`, the items are not destroyed, they are just "hidden" and remain in that state until they are needed again.

`Recycle(int index)` – remove item by index from list (not from from your data stotage)

`RecycleAll()` – clear list

## Pull to refresh

To enable pull-to-refresh for your list, check the direction checkbox in the inspector, add a handler to your code, and apply new items to the list.

```csharp
    /// <summary>
    /// Init list
    /// </summary>
    void Start() {
        List.OnFill += OnFillItem;
        List.OnHeight += OnHeightItem;
        List.OnPull += OnPullItem;
        ...
    }

    /// <summary>
    /// Callback after pull
    /// </summary>
    /// <param name="direction">Director pulled from</param>
    void OnPullItem(ScrollerDirection direction) {
        if (direction == ScrollerDirection.Top) {
            // load data and insert it to your storage
            // new data will be displayed at the top
        } else {
            // load data and add it to your storage
            // new data will be displayed at the bottom            
        }
        // call ApplyDataTo to update Scroller and show new data
        // pass current list items count, new loaded items count and direction
        List.ApplyDataTo(_list.Count, PullCount, direction);
    }
```

## Scroll to

You can scroll list to the desired item by calling `ScrollTo` method. Pass item index and list will be scrolled to the desired position.

> [!NOTE] 
> If you pass an index greater than the number of elements – list will scroll to the end.

## Change prefab

You can change the prefab of a list item at runtime. This is a rare situation, but it can be useful. To do that, apply new prefab to Scroller, call `RefreshViews` and next call `InitData` again.

```csharp
    /// <summary>
    /// Apply new prefab
    /// </summary>
    void OnPullItem() {
        List.Prefab = NewPrefab;
        List.RefreshViews(_listData.Count);
        List.InitData(_listData.Count);
    }
```

The Scroller will then display items with the new prefab.

## Runtime methods

`GetAllViews()` – return all list game objects

`RefreshViews(int count)` – update prefab of game objects, recalculate sizes and positions

`UpdateVisible()` – update visible items on the screen, `OnFill` will be called

`Recycle(int index)` – remove list item by index

`RecycleAll()` – remove all items

`ScrollTo(int index)` – scroll list to desired list item

`ApplyDataTo(int count, int newCount, ScrollerDirection direction)` – add new items to list from desired direction

`InitData(int count, bool isOtherSide)` – init and show list items

`NormalizedPosition` – current normalized position of ScrollRect

`ViewsCount` – current list game objects count

`IsInited` – flag to check list initialization state