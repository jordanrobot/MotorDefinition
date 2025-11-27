# Motor Torque Curve Editor - Architecture Options

This document outlines viable architecture options for the motor torque curve editor application.

---

## Option 1: .NET MAUI

### Overview
.NET MAUI (Multi-platform App UI) is Microsoft's evolution of Xamarin.Forms, providing a single codebase for cross-platform applications.

### Pros
- ✅ Native C# and .NET 8 support
- ✅ Single codebase for Windows, macOS, iOS, Android
- ✅ Can publish as single-file, self-contained executable
- ✅ Direct NuGet package support (Tare package)
- ✅ Good charting libraries available (LiveCharts2, OxyPlot, ScottPlot)
- ✅ Native Windows 11 look and feel
- ✅ Strong Microsoft support and documentation
- ✅ MVVM architecture support

### Cons
- ❌ Larger file size for self-contained apps (~100-150MB)
- ❌ MAUI still maturing, occasional quirks
- ❌ Linux support is community-driven (not official)

### Deployment
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Recommended Charting Library
- **LiveCharts2** - Best for interactive, animated charts with MAUI
- **ScottPlot** - Lightweight, good for scientific plots

---

## Option 2: Avalonia UI (Best Cross-Platform)

### Overview
Avalonia is a cross-platform XAML-based UI framework that runs on Windows, macOS, Linux, iOS, Android, and WebAssembly.

### Pros
- ✅ C# and .NET 8 native
- ✅ Truly cross-platform (including Linux desktop)
- ✅ Smaller binary size than MAUI
- ✅ Similar XAML syntax to WPF (familiar for C# devs)
- ✅ Can publish as single-file executable
- ✅ Direct NuGet package support
- ✅ Excellent charting with LiveCharts2 or ScottPlot

### Cons
- ❌ Not as "native" looking on Windows as MAUI
- ❌ Smaller community than MAUI
- ❌ Learning curve if not familiar with XAML

### Deployment
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## Option 3: Blazor Hybrid (.NET MAUI Blazor)

### Overview
Combines Blazor's web-based UI with MAUI's native container. Write UI in Razor/HTML/CSS, run as native desktop app.

### Pros
- ✅ Web technologies (HTML/CSS) for UI
- ✅ C# for logic, full .NET 8 support
- ✅ Excellent charting libraries (Chart.js, Plotly, ApexCharts via Blazor wrappers)
- ✅ Single executable deployment
- ✅ Familiar if you know web development
- ✅ UI can be styled with modern CSS frameworks

### Cons
- ❌ WebView2 dependency on Windows (usually pre-installed on Windows 11)
- ❌ Slightly more complex architecture
- ❌ Performance slightly lower than pure native

### Deployment
Same as MAUI - self-contained executable.

---

## Option 4: Electron.NET

### Overview
Electron for .NET allows building cross-platform desktop apps using ASP.NET Core and Electron.

### Pros
- ✅ Full web technologies (HTML/CSS/JS)
- ✅ Many charting libraries available (Chart.js, D3, Plotly)
- ✅ Cross-platform (Windows, macOS, Linux)
- ✅ C# backend with Razor or Blazor Server

### Cons
- ❌ Large bundle size (~150-200MB+)
- ❌ Higher memory usage (Chromium-based)
- ❌ Slower startup compared to native
- ❌ More complex build process
- ❌ Electron.NET is less maintained than pure Electron

### Deployment
Produces portable folder or installer.

---

## Option 5: WPF (Windows Only)

### Overview
Windows Presentation Foundation - mature, native Windows desktop framework.

### Pros
- ✅ Mature and stable
- ✅ Native Windows look and feel
- ✅ Excellent charting (LiveCharts, OxyPlot, ScottPlot)
- ✅ C# and .NET 8 support
- ✅ Can publish as single-file
- ✅ Smaller size than MAUI

### Cons
- ❌ Windows only (no cross-platform)
- ❌ Older technology (but still supported)
- ❌ No future cross-platform expansion path

### Deployment
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## Option 6: Pure Electron (with TypeScript/JavaScript)

### Overview
Standard Electron app with Node.js backend.

### Pros
- ✅ Huge ecosystem of charting libraries
- ✅ Cross-platform
- ✅ Very flexible UI design
- ✅ Large community

### Cons
- ❌ Not C# (TypeScript/JavaScript)
- ❌ Large bundle size
- ❌ Higher memory usage
- ❌ Would require learning new stack

---

## Option 7: Tauri (Rust + Web Frontend)

### Overview
Lightweight alternative to Electron using native webview and Rust backend.

### Pros
- ✅ Very small bundle size (~5-15MB)
- ✅ Low memory usage
- ✅ Cross-platform
- ✅ Web UI flexibility

### Cons
- ❌ Backend in Rust, not C#
- ❌ Would require learning Rust
- ❌ Younger ecosystem than Electron

---

## Comparison Matrix

| Criteria | MAUI | Avalonia | Blazor Hybrid | Electron.NET | WPF | Pure Electron | Tauri |
|----------|------|----------|---------------|--------------|-----|---------------|-------|
| C#/.NET Native | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Windows Support | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Cross-Platform | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ |
| Bundle Size | Medium | Small | Medium | Large | Small | Large | Tiny |
| Portable Exe | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ | ✅ |
| Charting Options | Good | Good | Excellent | Excellent | Excellent | Excellent | Excellent |
| Learning Curve | Low | Medium | Low | Medium | Low | Medium | High |
| Tare NuGet Support | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| Native Look | ✅ | Medium | Medium | Medium | ✅ | Medium | Medium |

---

## Recommendation

See [03-recommended-approach.md](./03-recommended-approach.md) for detailed recommendation.
