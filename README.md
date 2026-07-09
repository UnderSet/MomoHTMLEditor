# MomoHTMLEditor

A CLI tool to easily edit and create MomoTalk-style stuff using HTML quickly, based off of [**ManicMinic** on AO3](https://archiveofourown.org/users/ManicMinic)'s [**MomoTalk workskin template**](https://archiveofourown.org/works/64821349).

**_Not_** a fully automated tool by any means. You should know how to edit HTML and basic CSS styling to edit on the output of this tool. Styling on messages is only included for convenience.

[See this repository for assets.](https://github.com/UnderSet/MomoHTMLEditorAssets)

## Features
- 3 message types: Sent (from Sensei), Received (from someone else), System
  - "Choice" messages are out of scope (and yes, I'm aware ingame MomoTalk does have it)
- Message reordering, insertion and deletion
- _Automatic sender pictures set up_ based on sender name, complete with end-user configuration support (documentation coming soon)
  - _Note:_ If a profile picture is not found for a specific sender, this will not make a sender picture for you
- Easy(_-ish_) to use (hopefully)
- Probably more?

## Usage

Put the downloaded binary anywhere / copy your compiled binary (or binaries if you didn't build as single file, and in that case into its own folder) and run it. That's it.

No .NET runtime required if using prebuilt binaries.

Usage documentation soon.

## Build

**Requirements:** .NET 10 SDK.

`dotnet build`. That's it, you're done.

To build a singlefile binary like releases, run `dotnet publish -r <runtime ID>`. replacing `<runtime ID>` with a corresponding ID ([list from .NET documentation](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#known-rids); also see table below).

| OS | Status | Runtime ID (for building) |
|:---|:---|:---|
|Windows<br>*(prebuilt binaries soon)*|Windows 11: ***Yes***<br>Windows 10 and earlier: *Untested, may work*|`win-x64`<br>Untested: `win-x86`, `win-arm64`|
|Linux<br>*(no prebuilt binaries)*|Tested to work on Arch Linux; other distros may work|`linux-x64`, `linux-arm64`, et cetera.<br>[Linux runtime ID list on .NET docs](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#linux-rids)|
|macOS<br>*(no prebuilt binaries)*|Intel (x64) [^1]: *Yes (last tested on Sequoia 15.7.7)*<br>Apple Silicon (ARM64): Untested, unsupported [^2]|`osx` (universal binary supporting both x64 and ARM), `osx-x64`, `osx-arm64`|

## Additional Credits
- [**indisputablynobody**](https://github.com/indisputablynobody) for help testing the tool, feedback
- [**MomoTalk Editor**](https://github.com/U1805/momotalk), [**ClosureTalk**](https://github.com/ClosureTalk/closure-talk) for inspiration
  - Note that _this_ tool has significantly different design philosophies and focus from those two

[^1]: Won't be relevant for much longer since Macs on Intel has reached end of the line with macOS 27 dropping support...

[^2]: As in I literally can't provide support for on Apple Silicon. I don't own an Apple Silicon Mac nor really have access to one, especially one running latest macOS.
