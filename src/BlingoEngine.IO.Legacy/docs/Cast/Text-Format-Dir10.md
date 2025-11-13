# Member Text Format : Director Version 10+

[← Back to the format overview](./README.md)

Modern Director releases store per-character formatting inside an `XMED` stream. The chunk begins
with an ASCII directory that announces the offsets and capacities for each data block before listing
the text runs and style descriptors.

## extra bytes breakdown example
Typical bytes just before ending N/A: 
```
68 EF 75 86    68    EF 77 75
```
4 first are almost identical to next 4.



## Specific bytes:

Theses are the bytes from the extruder


| RAW bytes   	| Property				| Value/Description					|
|---------------|-----------------------|-----------------------------------|
| 00 00 00 04 	| type length			| 4									|
| 74 65 78 74 	| type					| text								|
| 00 00 01 B0	| 						| 432								|
| 00 00	00 01	| Editable				| ON  								|
| 00 00 00 00 	| Framing				| 1=Scroling, 2=Fixed				|
| 00 00 00 00 	| Tab on/off			| 1=ON, 0=OFF						|
| 00 00 00 00 	| DTD on/off			| 1=ON, 0=OFF						|
| 00 00 00 01 	| Antialias on/off		| 1=ON, 0=OFF						|
| 00 00 00 0E 	| Antialias Mode		| 0=AllText, E0=Default? ,13=LargetThen		|
| 00 00 00 00 	| AntaAlias Larger Size	| 15								|
| 00 00 00 00 	| 						| 									|
| 00 00 00 0F 	| Kerning Larger Size	| 15								|
| 00 00 1F F4 	| 						| 81.80								|
| 00 00 00 01 	| Kerning On/Off		| 1=ON, 0=OFF						|
| 00 00 00 0E 	| Kerning Mode			| 0=AllText, E0=Default? ,13=LargetThen		|
| 00 00 00 01 	| UseHyperlinkStyles	| 1=ON, 0=OFF						|
| 00 00 00 00 	| 						| 									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 00 	| PreRender Ink			| 1=InkCopy, 2=InkOther				|
| 00 00 00 00 	| PreRender Save BMP	| 1=ON, 0=OFF						|
| 33 54 45 58 	| 						| 3TEX or XET3						|
| 00 00 01 64 	| 						| 356								|
| FF FF FF FF 	| 						| White								|
| 00 53 21 47 	| Tunnel Depth			| 50 or 83.13, 16.16 fixed-point, big-endian. 0x00532147 / 65536 = 83.13 |
| 00 00 00 01 	| 						| 1									|
| 00 02 CC CC	| Bevel Amount			| 2.80 = 16.16 fixed (big-endian).	0x0002CCCC / 65536 = 2.8027 ≈ 2.80 |
| 00 00 00 01 	| 						| 1									|
| 00 00 00 05 	| 						| 5									|
| 00 00 00 01 	| 						| 1									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 02 	| Perhaps light dir?	| 2									|
| 00 00 00 00 	| 						| 									|
| 00 00 00 1E  	| Reflectivity     		| 30								|
| 99 66 33 00  	| Directional			| ≈ #A0522D	16-bit RGB				|
| 99 33 66 00  	| Ambient				| ≈ #DA70D6	16-bit RGB				|
| 00 99 66 00 	| Background			| ≈ #008080	16-bit RGB				|
| 41 40 00 00  	| Camera Pos X			| IEEE-754 32-bit floating-point	|
| 42 08 00 00  	| Camera Pos Y			| IEEE-754 32-bit floating-point	|
| 42 60 00 00 	| Camera Pos Z			| IEEE-754 32-bit floating-point	|
| 02 19 F6 10 	| Distance?				| 0x0219F610 / 65536 = 537.9612		|
| 42 9C 00 00  	| Camera Rot X			| IEEE-754 32-bit floating-point	|
| 42 C4 00 00  	| Camera Rot Y			| IEEE-754 32-bit floating-point	|
| 42 98 00 00 	| Camera Rot Z			| IEEE-754 32-bit floating-point	|
| 02 19 EF A8	| Focal?				| 0x0219EFA8 / 65536 = 537.9362		|
| 4E 6F 54 65   | TextureName			| NoTexture							|
| 78 74 75 72   | ...					|									| 
| 65            | ...					|									| 


#### Example values:
| Label            | Value                          |
|------------------|--------------------------------|
| Camera Pos (XYZ) | [12.00, 34.00, 56.00]          | 
| Rotation (XYZ)   | [78.00, 98.00, 76.00]          | 
| Face: Front      | ✗                             |
| Face: Back       | ✓                              |
| Face: Tunnel     | ✗                             |
| Smoothness       | 2                              |
| Tunnel Depth     | 83.13                          | 
| Bevel Amount     | 2.80                           | 
| Bevel Edge       | Miter                          |
| Light            | Middle Left                    |
| Directional      | #A0522D (brown)                | 
| Ambient          | #DA70D6 (orchid purple)        | 
| Background       | #008080 (teal)                 | 
| Shader Texture   | Default                        |
| Texture Name     | NoTexture (disabled)           |
| Diffuse          | #0000FF (blue)                 | 
| Specular         | #008000 (green)                | 
| Reflectivity     | 53                             | 

[← Back to the format overview](./README.md)