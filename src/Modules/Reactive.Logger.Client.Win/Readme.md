![](https://img.shields.io/nuget/v/Xpand.XAF.Modules.Reactive.Logger.Client.Win.svg?&style=flat) ![](https://img.shields.io/nuget/dt/Xpand.XAF.Modules.Reactive.Logger.Client.Win.svg?&style=flat)

[![GitHub issues](https://img.shields.io/github/issues/eXpandFramework/expand/Reactive.Logger.Client.Win.svg)](https://github.com/eXpandFramework/eXpand/issues?utf8=%E2%9C%93&q=is%3Aissue+is%3Aopen+sort%3Aupdated-desc+label%3AStandalone_xaf_modules+Reactive.Logger.Client.Win) [![GitHub close issues](https://img.shields.io/github/issues-closed/eXpandFramework/eXpand/Reactive.Logger.Client.Win.svg)](https://github.com/eXpandFramework/eXpand/issues?utf8=%E2%9C%93&q=is%3Aissue+is%3Aclosed+sort%3Aupdated-desc+label%3AStandalone_XAF_Modules+Reactive.Logger.Client.Win)
# About 

The `Reactive.Logger.Client.Win` is a XAF Windows Forms application capable to connect to any XAF application that has the [Reactive.Logger.Hub](https://github.com/eXpandFramework/DevExpress.XAF/tree/master/src/Modules/Reactive.Logger.Hub) module installed. More in the details section.

The module uses the next two strategies:
1. It monitors the `DetailView` creation and modifies its Reactive.Logger.Client.Win property according to model configuration. However later Reactive.Logger.Client.Win property modifications are allowed.
2. It monitors the `Reactive.Logger.Client.Win` modifiation and cancels it if the `LockReactive.Logger.Client.Win` attribute is used.
## Installation 
1. First you need the nuget package so issue this command to the `VS Nuget package console` 

   `Install-Package Xpand.XAF.Modules.Reactive.Logger.Client.Win`.

    The above only references the dependencies and nexts steps are mandatory.

2. [Ways to Register a Module](https://documentation.devexpress.com/eXpressAppFramework/118047/Concepts/Application-Solution-Components/Ways-to-Register-a-Module)
or simply add the next call to your module constructor
    ```cs
    RequiredModuleTypes.Add(typeof(Xpand.XAF.Modules.Reactive.Logger.Client.WinModule));
    ```

The module is not integrated with any `eXpandFramework` module. You have to install it as described.

## Versioning
The module is **not bound** to **DevExpress versioning**, which means you can use the latest version with your old DevExpress projects [Read more](https://github.com/eXpandFramework/XAF/tree/master/tools/Xpand.VersionConverter).

The module follows the Nuget [Version Basics](https://docs.microsoft.com/en-us/nuget/reference/package-versioning#version-basics).
## Dependencies
`.NetFramework: `

|<!-- -->|<!-- -->
|----|----
|**DevExpress.ExpressApp**|**Any**
|System.Reactive|4.1.6
 |[Xpand.XAF.Modules.Reactive](https://github.com/eXpandFramework/DevExpress.XAF/tree/master/src/Modules/Xpand.XAF.Modules.Reactive)|1.2.46
 |[Xpand.VersionConverter](https://github.com/eXpandFramework/DevExpress.XAF/tree/master/tools/Xpand.VersionConverter)|1.0.34

## Issues-Debugging-Troubleshooting

To `Step in the source code` you need to `enable Source Server support` in your Visual Studio/Tools/Options/Debugging/Enable Source Server Support. See also [How to boost your DevExpress Debugging Experience](https://github.com/eXpandFramework/DevExpress.XAF/wiki/How-to-boost-your-DevExpress-Debugging-Experience#1-index-the-symbols-to-your-custom-devexpresss-installation-location).

If the package is installed in a way that you do not have access to uninstall it, then you can `unload` it with the next call when [XafApplication.SetupComplete](https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.XafApplication.SetupComplete).
```ps1
((Xpand.XAF.Modules.Reactive.Logger.Client.WinModule) Application.Modules.FindModule(typeof(Xpand.XAF.Modules.Reactive.Logger.Client.WinModule))).Unload();
```

## Details
The client will connect and persist remote calls that describe the pipeline flow. Bellow is a screencast when running the client and execute one of our tests. The `Bind_Only_NullAble_Properties_That_are_not_Null` test makes sure that the `ModelMapper` module will only bind those model properties that have value.
![2019-09-21_23-53-04](https://user-images.githubusercontent.com/159464/65379147-1da06200-dccc-11e9-93c4-98c3a21ea3ed.gif)

The information can be overwhelming however simple filters let us extra very useful data as to what the ModelMapper did. 

The client is Hybrid logger and and uses both the Logger and the Hub so everything discussed there applies also here.
### Tests
The module is tested on Azure for each build with these [tests](https://github.com/eXpandFramework/Packages/tree/master/src/Tests/Reactive.Logger.Client.Win)

### Examples
Head to [Reactive.Logger](https://github.com/eXpandFramework/DevExpress.XAF/tree/lab/src/Modules/Reactive.Logger)