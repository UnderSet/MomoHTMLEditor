# MomoHTMLEditor

Tool to easily edit and create MomoTalk-style stuff using HTML.

## FAQs (maybe)

### Q: Why are you rewriting this?
Take a *good, good* look at my PowerShell code and tell me that stuff's maintainable. (It is not.)

Also, *look I just wanted an excuse to get into C#.* Was kinda looking to do it at *some* point.

## Usage

Put the downloaded binary anywhere / copy your compiled binary (or binaries if you didn't build as single file, and in that case into its own folder) and run it. That's it.

*User's Manual* soon.

## Build

Open this in Visual Studio. Hit Build. Done.

Alternatively, if you don't fancy VS much or is on macOS/Linux - .NET 10 *SDK* required - `dotnet build`. Done.

| OS | Status |
|:---|:---|
|Windows<br>*(prebuilt binaries soon)*|Windows 11: ***Yes***<br>Windows 10 and earlier: *Untested, may work*|
|Linux<br>*(no prebuilt binaries)*|Arch Linux: *Yes*<br>Other distros: Untested|
|macOS<br>*(no prebuilt binaries)*|Intel (x64) [^1]: *Yes (on Sequoia 15.7.7)*<br>Apple Silicon (ARM64): Untested, unsupported|
## Credits
- [**ManicMinic** on AO3]() for their [MomoTalk workskin template](https://archiveofourown.org/works/64821349), which inspired, influenced and just made this whole project possible.

[^1]: Won't be relevant for much longer since Macs on Intel has reached end of the line with macOS 27 dropping support...