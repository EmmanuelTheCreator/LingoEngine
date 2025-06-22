# ProjectorRays Director Shockwave Decompiler – .NET Version

**Original project:** [https://github.com/ProjectorRays/ProjectorRays](https://github.com/ProjectorRays/ProjectorRays)

This version was created to be used in the [LingoEngine Project](https://github.com/EmmanuelTheCreator/LingoEngine). It is a recreation of the classic Director environment in C#/.NET, designed to help convert old Director projects to .NET and make them runnable in Godot, SDL, or any other future framework.

**ProjectorRays** is a decompiler for [Adobe Shockwave](https://en.wikipedia.org/wiki/Adobe_Shockwave) and [Adobe Director](https://en.wikipedia.org/wiki/Adobe_Director) (formerly Macromedia Shockwave and Macromedia Director—not to be confused with [Shockwave Flash](https://en.wikipedia.org/wiki/Adobe_Flash)).

Director was released in 1987 and quickly became the world’s leading multimedia platform. Beginning in 1995, Director movies could be published as `.dcr` files and played on the web using the Shockwave plugin. Over the years, the platform powered countless CD-ROM and web games before being fully discontinued in 2019.

Today, Shockwave games are no longer playable on the modern web, and the original source code is often lost or unavailable. ProjectorRays can take a published game, reconstruct its [Lingo](https://en.wikipedia.org/wiki/Lingo_(programming_language)) source code, and generate editable project files to support preservation efforts.

- If you have a `.dcr` (published Shockwave movie) or `.dxr` (protected Director movie), ProjectorRays can generate a `.dir` (editable Director movie).
- If you have a `.cct` (published Shockwave cast) or `.cxt` (protected Director cast), ProjectorRays can generate a `.cst` (editable Director cast).

ProjectorRays is a work in progress. If you encounter any issues, please report them on the [issue tracker](https://github.com/ProjectorRays/ProjectorRays/issues).

---

## Credits

ProjectorRays was written by [Debby Servilla](https://github.com/djsrv), based on the [disassembler](https://github.com/Brian151/OpenShockwave/blob/50b3606809b3c8dad13ee41ae20bcbfa70eb3606/tools/lscrtoscript/js/projectorrays.js) by [Anthony Kleine](https://github.com/tomysshadow).  
Converted to .NET by [Emmanuel The Creator](https://github.com/EmmanuelTheCreator).

This project could not exist without the reverse-engineering work of:

- [The Earthquake Project](https://github.com/Earthquake-Project)  
- [Just Solve the File Format Problem Wiki](http://fileformats.archiveteam.org/wiki/Lingo_bytecode)  
- [ScummVM Director engine team](https://www.scummvm.org/credits/#:~:text=Director:)

---

## License

ProjectorRays is licensed under the **Mozilla Public License 2.0**.
