# Lingo Feature Conversion Progress

This document tracks the current implementation status of Lingo language elements in **LingoEngine**. The percentages are based on the comparison between the Director MX 2004 scripting manual and the interfaces implemented in the repository.

Completed features are omitted from the tables to keep them concise. Only unimplemented or partial features are listed and marked "No".

## Overview

| Lingo element | Progress | Notes |
|--------------|---------|-------|
| Sprite object | 96% properties implemented | 2D sprite features largely complete; 3D handled separately |
| Movie object | 10% properties, 12% methods implemented | Only a small subset of movie control features exist. |
| Sprite3D object | 20% implemented | Basic camera management available |
| Movie3D object | 5% implemented | Renderer properties only |
| Player object | 42% properties, 58% methods implemented | Basic environment control only |

## Sprite Object Details
Sprite properties implemented: 96% (29 of 30)

| Property | Implemented | Notes |
|----------|-------------|-------|
| antiAliasingEnabled | No |

## Sprite3D Object Details
Initial Sprite3D support has been added with camera management APIs.

### Properties

### Methods
## Movie Object Details

### Properties
Movie properties implemented: 10% (6 of 63)


| Property | Implemented | Notes |
|----------|-------------|-------|
| aboutInfo | No |
| allowCustomCaching | No |
| allowGraphicMenu | No |
| allowSaveLocal | No |
| allowTransportControl | No |
| allowVolumeControl | No |
| allowZooming | No |
| beepOn | No |
| buttonStyle | No |
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
| paletteMapping | No |
| path | No |
| preLoadEventAbort | No |
| score | No |
| scoreSelection | No |
| script | No |
| sprite | No |
| stage | No |
| traceLoad | No |
| traceLogFile | No |
| traceScript | No |
| updateLock | No |
| useFastQuads | No |
| xtraList | No |

### Methods
Movie methods implemented: 12% (5 of 41)


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
| duplicateFrame | No |
| puppetTempo | No |
| endRecording | No |
| finishIdleLoad | No |
| ramNeeded | No |
| frameReady | No |
| rollOver | No |
| saveMovie | No |
| goLoop | No |
| sendAllSprites | No |
| goNext | No |
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

## Movie3D Object Details
Basic Movie3D functionality is available.

### Properties

## Player Object Details
Player properties implemented: 42% (15 of 36)

### Properties
| Property | Implemented | Notes |
|----------|-------------|-------|
| debugPlaybackEnabled | No |
| digitalVideoTimeScale | No |
| disableImagingTransformation | No |
| emulateMultibuttonMouse | No |
| externalParamCount | No |
| frontWindow | No |
| inlineImeEnabled | No |
| lastRoll | No |
| mediaXtraList | No |
| netThrottleTicks | No |
| scriptingXtraList | No |
| searchCurrentFolder | No |
| searchPathList | No |
| serialNumber | No |
| switchColorDepth | No |
| toolXtraList | No |
| transitionXtraList | No |
| userName | No |
| window | No |
| xtra | No |
| xtraList | No |

### Methods
Player methods implemented: 58% (7 of 12)

| Method | Implemented | Notes |
|-------|------------|------|
| externalParamName | No |
| externalParamValue | No |
| flushInputEvents | No |
| getPref | No |
| setPref | No |

Percentages are approximate and based on manual comparison of the scripting manual with the repository interfaces.
