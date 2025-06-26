# Lingo Feature Conversion Progress

This document tracks the current implementation status of Lingo language elements in **LingoEngine**. The percentages are based on the comparison between the Director MX 2004 scripting manual and the interfaces implemented in the repository.

## Overview

| Lingo element | Progress | Notes |
|--------------|---------|-------|
| Sprite object | 57% properties implemented; no 3D camera methods | Many 3D related properties and methods are not yet present. |
| Movie object | 10% properties, 12% methods implemented | Only a small subset of movie control features exist. |

## Sprite Object Details

The following table lists Sprite properties and whether they are currently implemented in `ILingoSprite`.

| Property | Implemented | Notes |
|----------|-------------|-------|
| backColor | Yes |
| blend | Yes |
| bottom | No |
| constraint | No |
| cursor | No |
| editable | Yes |
| endFrame | Yes |
| flipH | No |
| flipV | No |
| foreColor | Yes |
| height | Yes |
| ink | Yes |
| left | No |
| locH | Yes |
| locV | Yes |
| locZ | Yes |
| member | Yes |
| name | Yes |
| quad | No |
| rect | Yes |
| right | No |
| rotation | Yes |
| skew | Yes |
| spriteNum | Yes |
| startFrame | Yes |
| top | No |
| width | Yes |
| antiAliasingEnabled | No |
| camera | No |
| directToStage | No |

The manual also lists camera-related methods (`addCamera`, `cameraCount`, `deleteCamera`) which are not yet present in the interface.

## Movie Object Details

### Properties

| Property | Implemented | Notes |
|----------|-------------|-------|
| aboutInfo | No |
| active3dRenderer | No |
| actorList | Yes |
| allowCustomCaching | No |
| allowGraphicMenu | No |
| allowSaveLocal | No |
| allowTransportControl | No |
| allowVolumeControl | No |
| allowZooming | No |
| beepOn | No |
| buttonStyle | No |
| castLib | Yes |
| centerStage | No |
| copyrightInfo | No |
| displayTemplate | No |
| dockingEnabled | No |
| editShortCutsEnabled | No |
| enableFlashLingo | No |
| exitLock | No |
| fileFreeSize | No |
| fileSize | No |
| fileVersion | No |
| fixStageSize | No |
| frame | Yes |
| frameLabel | No |
| framePalette | No |
| frameScript | No |
| frameSound1 | No |
| frameSound2 | No |
| frameTempo | No |
| frameTransition | No |
| idleHandlerPeriod | No |
| idleLoadMode | No |
| idleLoadPeriod | No |
| idleLoadTag | No |
| idleReadChunkSize | No |
| imageCompression | No |
| imageQuality | No |
| keyboardFocusSprite | No |
| lastChannel | No |
| lastFrame | No |
| markerList | No |
| member | Yes |
| name | Yes |
| paletteMapping | No |
| path | No |
| preferred3dRenderer | No |
| preLoadEventAbort | No |
| score | No |
| scoreSelection | No |
| script | No |
| sprite | No |
| stage | No |
| timeoutList | Yes |
| traceLoad | No |
| traceLogFile | No |
| traceScript | No |
| updateLock | No |
| useFastQuads | No |
| xtraList | No |

### Methods

| Method | Implemented | Notes |
|-------|------------|------|
| beginRecording | No |
| newMember | No |
| cancelIdleLoad | No |
| preLoad | No |
| clearFrame | No |
| preLoadMember | No |
| constrainH | No |
| preLoadMovie | No |
| constrainV | No |
| printFrom | No |
| delay | No |
| puppetPalette | No |
| deleteFrame | No |
| puppetSprite | Yes |
| duplicateFrame | No |
| puppetTempo | No |
| endRecording | No |
| puppetTransition | Yes |
| finishIdleLoad | No |
| ramNeeded | No |
| frameReady | No |
| rollOver | No |
| go | Yes |
| saveMovie | No |
| goLoop | No |
| sendAllSprites | No |
| goNext | No |
| sendSprite | Yes |
| goPrevious | No |
| stopEvent | No |
| idleLoadDone | No |
| unLoad | No |
| insertFrame | No |
| unLoadMember | No |
| label | No |
| unLoadMovie | No |
| marker | No |
| updateFrame | No |
| mergeDisplayTemplate | No |
| updateStage | Yes |

Percentages are approximate and based on manual comparison of the scripting manual with the repository interfaces.
