# Third-party notices

## CodeWalker / RPF algorithms

RPF key derivation, the encrypted `magic.dat` resource and NG decryption use [gtav-enhanced-rpf](https://github.com/wjy000/gtav-enhanced-rpf) as a reference implementation (MIT, Copyright (c) 2026 Codey). Its algorithms and encrypted resource originate from [CodeWalker](https://github.com/dexyfex/CodeWalker). The [GTACrypto module](https://github.com/dexyfex/CodeWalker/blob/master/CodeWalker.Core/GameFiles/Utils/GTACrypto.cs) carries the following MIT notice: Copyright (c) 2015 Neodymium.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## GFX format references

The Scaleform compacted-font format was checked against the [JPEXS Free Flash Decompiler](https://github.com/jindrapetrik/jpexs-decompiler) format definitions (LGPL-3.0-or-later). GTAVFix has an independently written codec; no JPEXS source files or binaries are bundled.

## Game fonts

Font outlines are read from the user's local game installation at runtime. Extracted fonts, game archives and generated patches are excluded from this repository and its release package. GTA V and its assets belong to their respective owners.
