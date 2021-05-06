# DynamicPaper
<p float="left" align="left" width="100%">
 <img src="https://img.shields.io/github/license/Maxstupo/DynamicPaper.svg" />
 <img src="https://img.shields.io/github/release/Maxstupo/DynamicPaper.svg" />
 <a href="https://ci.appveyor.com/project/Maxstupo/DynamicPaper">
    <img src="https://ci.appveyor.com/api/projects/status/l70py6w3qu9tmwm7?svg=true" />
 </a>
</p>

DynamicPaper is a Microsoft Windows wallpaper app, that supports media formats that aren't just images. 

## Features
* Multi-Monitor Support
* Video Wallpapers - with per video volume.
* Image Wallpapers
* [ShaderToy](https://www.shadertoy.com/) Wallpapers
* Playlists - Shuffle, Repeat, and Loop


# Supported Media
* Video Wallpapers
  * mkv, mp4, mov, avi, wmv, gif, webm
  * ... and many more, thanks to [LibVLC](https://www.videolan.org/vlc/libvlc.html) (VLC Media Player's Core)
* Image Wallpapers
  * png, jpeg, bmp, gif
* [ShaderToy](https://www.shadertoy.com/) Shaders
  * `dpst` files from the `stpack` [wiki page](https://github.com/Maxstupo/DynamicPaper/wiki/shadertoy)

## Releases

**Stable releases of DynamicPaper are on the [releases page](https://github.com/Maxstupo/DynamicPaper/releases).**

#### Development Builds
The latest development build of DynamicPaper is provided by AppVeyor. View the latest build status [here](https://ci.appveyor.com/project/Maxstupo/DynamicPaper).
<br/>
Latest dev builds:
- [Zipped Build - Debug](https://ci.appveyor.com/api/projects/Maxstupo/DynamicPaper/artifacts/DynamicPaper.zip?branch=develop&job=Configuration%3A+Debug)
- [Zipped Build - Release](https://ci.appveyor.com/api/projects/Maxstupo/DynamicPaper/artifacts/DynamicPaper.zip?branch=develop&job=Configuration%3A+Release)
- [Installer - Debug](https://ci.appveyor.com/api/projects/Maxstupo/DynamicPaper/artifacts/DynamicPaper-Setup.exe?branch=develop&job=Configuration%3A+Debug)
- [Installer - Release](https://ci.appveyor.com/api/projects/Maxstupo/DynamicPaper/artifacts/DynamicPaper-Setup.exe?branch=develop&job=Configuration%3A+Release)

Note: These builds are temporary and may be unstable as they are built from the [development branch](https://github.com/Maxstupo/DynamicPaper/tree/develop) for every commit automatically.

## Installation & Setup
Requires Microsoft Windows 8 or above.

Install using the `DynamicPaper-Setup.exe` from the downloaded [release](https://github.com/Maxstupo/DynamicPaper/releases/latest), and follow the prompts.

TBD

## Translation
Wanting to contribute by translating DynamicPaper to your language, or fix a typo?
<br><br>
Locales for DynamicPaper are available in the [DynamicPaper_i18n](https://github.com/Maxstupo/DynamicPaper_i18n) repository.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* [LibVLC](https://code.videolan.org/videolan/libvlc-nuget) & [LibVLCSharp](https://code.videolan.org/videolan/LibVLCSharp)
* [OpenTK](https://github.com/opentk/opentk)
* [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
* [MimeTypesMap](https://github.com/hey-red/MimeTypesMap)
* [Fody](https://github.com/Fody/Fody/) & [Fody.Costura](https://github.com/Fody/Costura)

