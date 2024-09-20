<a href="./README.md">![Static Badge](https://img.shields.io/badge/english-118027)</a>
<a href="./README.ru.md">![Static Badge](https://img.shields.io/badge/русский-0390fc)</a>
<p align="center">
    <img alt="UIS (Unity infinite scroller)" height="256" width="448" src="Media/logo-uis.png">
</p>
<h3 align="center">UIS</h3>
<h4 align="center">Unity infinite scroller for ScrollRect component</h4>
<p align="center">
    <a href="#quick-start">Quick start</a> · <a href="/Documentation~/index.md">Documentation</a> · <a href="https://github.com/mopsicus/uis/issues">Report Bug</a>
</p>

# 💬 Overview

This extension allows you to use the `ScrollRect` control as an infinite scroller/spinner. It is a fast, easy and mobile-friendly way to create lists with thousands of rows without lags and jitters.

### Problem

In Unity, you can use the `ScrollRect` control to scroll multiple objects in the UI. But when there are too many objects in the container, you will see lags and jitter when scrolling.

### Solution

This script uses a data-driven method to scroll and display list items. This means that you have the data of the list items and their height or width, and the script creates and shows only those items that fit on the screen. When you want to scroll the list, the list items at the top or bottom move in opposite directions.

# ✨ Features

- easy, light, mobile-friendly, just one script
- list items are fully customizable to fit your project
- dynamic, data-driven lists
- efficient reusing of list items
- `pull-to-refresh` feature
- `scroll-to` feature
- diffirent list items height/width support

# 🚀 Usage

### Installation

Get it from [releases page](https://github.com/mopsicus/uis/releases) or add the line to `Packages/manifest.json` and module will be installed directly from Git url:

```
"com.mopsicus.uis": "https://github.com/mopsicus/uis.git",
```

### Quick start

See the samples section to get a [demo app](./Samples~/Demo). This demo contains 6 scenes with different ways to use UIS. It will show you how to initiate and use it in your app, how to use pull-to-refresh feature, how to use lists with different height/width of elements.

_Tested in Unity 2020.3.x._

### How to use

1. Add `UIS` to uses section
2. Add `Scroll View` component to game object on UI canvas
3. Add `Scroller` script after it
4. Setup script as you need
5. Add callbacks and init list

```csharp
using UIS;
using UnityEngine;

public class Demo : MonoBehaviour {

    [SerializeField]
    Scroller List = null;

    void Start() {
        List.OnFill += OnFillItem;
        List.OnHeight += OnHeightItem;
        List.InitData(100);
    }

    void OnFillItem(int index, GameObject item) {
        // get data from your storage, JSON, etc
        //
        // var data = jsonArray[index]; for example
        //
        // fill list item
        //
        // item.GetComponent<ItemController>().Set(data);
    }

    int OnHeightItem(int index) {
        // get item height from your storage, JSON, etc
        // or calc it here and return
        //
        return 100;
    }
}
```

Read the [docs](Documentation~/index.md) for more details.

# 🏗️ Contributing

We invite you to contribute and help improve UIS. Please see [contributing document](./CONTRIBUTING.md). 🤗

You also can contribute to the uis project by:

- Helping other users 
- Monitoring the issue queue
- Sharing it to your socials
- Referring it in your projects

### Environment setup

For a better experience, you can set up an environment for local development. Since this project is developed with VS Code, all settings are provided for it.

1. Use `Monokai Pro` or `eppz!` theme
2. Use `FiraCode` font
3. Install extensions:
    - C#
    - C# Dev Kit
    - Unity
4. Enable `Inlay Hints` in C# extension
5. Install `Visual Studio Editor` package in Unity
6. Put `.editorconfig` in root project directory
7. Be cool

# 🤝 Support

You can support the project by using any of the ways below:

* Bitcoin (BTC): 1VccPXdHeiUofzEj4hPfvVbdnzoKkX8TJ
* USDT (TRC20): TMHacMp461jHH2SHJQn8VkzCPNEMrFno7m
* TON: UQDVp346KxR6XxFeYc3ksZ_jOuYjztg7b4lEs6ulEWYmJb0f
* Visa, Mastercard via [Boosty](https://boosty.to/mopsicus/donate)
* MIR via [CloudTips](https://pay.cloudtips.ru/p/9f507669)

# ✉️ Contact

Before you ask a question, it is best to search for existing [issues](https://github.com/mopsicus/uis/issues) that might help you. Anyway, you can ask any questions and send suggestions by [email](mailto:mail@mopsicus.ru) or [Telegram](https://t.me/mopsicus).

# 🔑 License

UIS is licensed under the [MIT License](./LICENSE.md). Use it for free and be happy. 🎉