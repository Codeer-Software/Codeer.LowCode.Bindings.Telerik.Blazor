# Bindings.Telerik.Blazor

Codeer.LowCode.BlazorにTelerik Blazorコンポーネントを追加するためのライブラリです。

## インストール

### パッケージのインストール

LowCodeApp.Client.Shared プロジェクトにNuGetから次のパッケージをインストールしてください。

- Codeer.LowCode.Bindings.Telerik.Blazor

### コードの修正

ライブラリの使用に必要なコードを以下のプロジェクトにそれぞれ追加する必要があります。

- LowCodeApp.Client
- LowCodeApp.Shared
- LowCodeApp.Designer

#### LowCodeApp.Client

`Program.cs` に以下のコードを追加してください。

```csharp
builder.Services.AddTelerikBlazor();
```

#### LowCodeApp.Server

`Program.cs` に以下のコードを追加してください。

```csharp
typeof(TelerikGanttField).ToString();
```

#### LowCodeApp.Designer

`App.xaml.cs` に以下のコードを追加してください。

```csharp
Services.AddTelerikBlazor();
BlazorRuntime.InstallAssemblyInitializer(typeof(TelerikGanttFieldDesign).Assembly);
```

## 使用方法

DesignerからTelerik UI for Blazorで実装されたガントチャートコンポーネントが配置できるようになっています。

## カスタムコントロール

このライブラリでは次のカスタムコントロールが提供されています。

- TelerikGanttChart

## TelerikGanttChart

ガントチャートを表示するコンポーネントです。