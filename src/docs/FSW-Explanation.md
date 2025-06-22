# Formal SignWriting (FSW) - Comprehensive Technical Explanation

*Analysis Date: June 4, 2025*  
*Source: SignWriting Libraries Documentation*  
*Version: draft-slevinski-formal-signwriting-09*

## Executive Summary

Formal SignWriting (FSW) is a computerized encoding system for Sutton SignWriting, designed as a formal language that represents sign language visually through structured character strings. This comprehensive analysis explores the complete technical specifications, encoding models, and implementation details of FSW as documented in the signwritinglibraries documentation.

## Table of Contents

1. [Introduction and Background](#introduction-and-background)
2. [Core Concepts](#core-concepts)
3. [Character Sets and Encoding](#character-sets-and-encoding)
4. [Format Specifications](#format-specifications)
5. [Symbol System](#symbol-system)
6. [Regular Expressions and Pattern Matching](#regular-expressions-and-pattern-matching)
7. [Coordinate System and Spatial Layout](#coordinate-system-and-spatial-layout)
8. [Query Language](#query-language)
9. [Technology Integration](#technology-integration)
10. [Font Technology](#font-technology)
11. [Transformations and Conversions](#transformations-and-conversions)
12. [Unicode Considerations](#unicode-considerations)
13. [Implementation Examples](#implementation-examples)
14. [Standards and Compliance](#standards-and-compliance)

## Introduction and Background

### What is Formal SignWriting?

Formal SignWriting (FSW) is one particular computerized design for Sutton SignWriting that envisions a sign as a two-part word of time and space. Unlike American Sign Language which is a natural language, Formal SignWriting is a formal language that uses words and punctuation to form text, where each word is expressed as a string of characters governed by structural grammar rules.

### Historical Context

- **1974**: Sutton SignWriting system invented by Valerie Sutton
- **2010**: International SignWriting Alphabet 2010 (ISWA 2010) established
- **2012**: Formal SignWriting in ASCII (FSW) specification released
- **2015**: Sutton SignWriting Block added to Unicode Standard
- **2017**: SignWriting in Unicode (SWU) specification released
- **2020-present**: Steve Slevinski assumed full responsibility for SignWriting websites

### Mission and Purpose

FSW serves as the foundational technology for:
- **Visual Sign Representation**: Converting sign language into written form
- **Multi-Format Support**: Handling FSW, SWU, KSW, BSW, CSW formats
- **Web Integration**: JavaScript libraries for browser applications
- **Educational Tools**: Character exploration and symbol browsing
- **Research Applications**: Linguistic analysis and documentation

## Core Concepts

### Formal Language Principles

FSW operates as a formal language with these characteristics:
- **Character Processing**: Uses regular expressions for pattern matching
- **Two-Part Structure**: Time (temporal prefix) and Space (spatial signbox)
- **String Representation**: Each sign written as a character string
- **Grammar Rules**: Well-formed words governed by structural rules
- **Mathematical Foundation**: Useful in mathematics, computer science, and linguistics

### Sign Structure Model

Every FSW sign consists of two main components:

1. **Temporal Prefix** (Optional): Sequence of symbols representing time-based elements
   - Format: `A[symbols]+`
   - Contains writing symbols and detailed locations
   - Represents sequential aspects of sign production

2. **Spatial Signbox** (Required): 2D arrangement of symbols with coordinates
   - Format: `[BLMR][width]x[height][symbols+coordinates]+`
   - Contains symbol positioning information
   - Represents spatial relationships between symbols

### Character Set Foundation

FSW uses a restricted ASCII character set: `ABLMRS0123456789xabcdef`

**Character Categories:**
- **Structural Markers**: A, B, L, M, R
- **Numbers**: 0-9 (coordinates use range 250-749)
- **Hexadecimal**: a-f (for symbol identification)
- **Separator**: x (coordinate separator)

## Character Sets and Encoding

### Formal SignWriting in ASCII (FSW)

**Key Characteristics:**
- Released: January 2012
- Stability: Stable since release
- Character Set: ASCII subset "ABLMRS0123456789xabcdef"
- URL Safe: All characters are URL-safe
- Database Friendly: Optimized for storage and retrieval

**FSW Character Mapping:**
```
Structural: A, B, L, M, R
Numbers: 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
Hexadecimal: a, b, c, d, e, f
Separator: x
```

### SignWriting in Unicode (SWU)

**Key Characteristics:**
- First Published: October 2016
- Unicode Submission: July 2017
- Status: Experimental (not part of Unicode standard)
- Character Planes: Unicode Plane 15 and 16
- Symbol Range: U+40001 to U+4F428

**SWU Character Ranges:**
- **Structural Markers**: U+1D800-U+1D9FF
- **Symbol Characters**: U+40001-U+4F480
- **Coordinate Characters**: U+1D80C-U+1DDFF

### Bidirectional Conversion

FSW and SWU character sets are isomorphic with easy bidirectional conversion:
- Every FSW string has a corresponding SWU representation
- Conversion algorithms maintain semantic equivalence
- Both formats support identical functionality

## Format Specifications

### Basic FSW Structure

```
[TemporalPrefix][SpatialSignbox][StylingString]

Where:
- TemporalPrefix: (A([ws]io)+)?  [optional]
- SpatialSignbox: [BLMR]nnn×nnn(Siiinn×nn)+ [required]
- StylingString: Various styling options [optional]
```

### Detailed Format Breakdown

#### 1. Temporal Prefix Format
```
A[Symbol1][Symbol2]...[SymbolN]

Examples:
- AS14c20S27106  (sequence with two symbols)
- AS10000         (single symbol sequence)
```

#### 2. Spatial Signbox Format
```
[Lane][Width]x[Height]S[SymbolKey][X]x[Y]...

Where:
- Lane: B (signbox) | L (left) | M (middle) | R (right)
- Width/Height: 250-749 (coordinate range)
- SymbolKey: 6-character identifier
- X/Y: Symbol coordinates (250-749)

Example:
M518x533S1870a489x515S18701482x490S20500508x496S2e734500x468
```

#### 3. Symbol Key Structure
```
S[Base][Fill][Rotation]

Where:
- S: Symbol indicator
- Base: 3 hexadecimal characters [123][0-9a-f]{2}
- Fill: 1 character [0-5]
- Rotation: 1 hexadecimal character [0-9a-f]

Example: S1870a
- S: Symbol marker
- 187: Base symbol (handshape)
- 0: Fill modifier (outline)
- a: Rotation modifier (specific orientation)
```

## Symbol System

### International SignWriting Alphabet 2010 (ISWA 2010)

The symbol system contains **37,811 symbols** organized hierarchically:

#### Symbol Categories

1. **Writing Symbols** (Category 01-30)
   - Hand symbols
   - Movement symbols
   - Face and head symbols
   - Body symbols
   - Contact symbols

2. **Detailed Location Symbols** (Category 31-37)
   - Precise positioning markers
   - Spatial relationship indicators

3. **Punctuation Symbols** (Category 38)
   - Sentence delimiters
   - Text flow markers

#### Symbol Type Ranges

| Type | FSW Range | SWU Range | Description |
|------|-----------|-----------|-------------|
| all symbols | S100 - S38b | U+40001 - U+4F480 | Complete symbol set |
| writing | S100 - S37e | U+40001 - U+4EFA0 | Main writing symbols |
| hand | S100 - S204 | U+40001 - U+461E0 | Handshapes |
| movement | S205 - S2f6 | U+461E1 - U+49980 | Movement arrows |
| dynamics | S2f7 - S2fe | U+49981 - U+49A80 | Dynamic indicators |
| timing | S2ff - S2ff | U+49A81 - U+49A81 | Timing markers |
| head | S300 - S36c | U+4A001 - U+4C5E0 | Head/face symbols |
| body | S36d - S376 | U+4C5E1 - U+4C880 | Body symbols |
| limb | S377 - S37e | U+4C881 - U+4EFA0 | Limb symbols |
| location | S37f - S386 | U+4EFA1 - U+4F2A0 | Location markers |
| punctuation | S387 - S38b | U+4F2A1 - U+4F480 | Punctuation |

### Symbol Modifiers

#### Fill Modifiers (0-5)
- **0**: Outline only
- **1**: Solid fill
- **2**: Thick outline
- **3**: Thick solid
- **4**: Dotted outline
- **5**: Dotted solid

#### Rotation Modifiers (0-f)
- **0-7**: Standard rotations (0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°)
- **8-f**: Mirrored rotations (flipped versions of 0-7)

## Regular Expressions and Pattern Matching

### Core Token Patterns

FSW defines 11 different tokens grouped in 4 layers:

#### Structural Markers (5 tokens)
| Token | Description |
|-------|-------------|
| A | Sequence Marker |
| B | Signbox Marker |
| L | Left Lane Marker |
| M | Middle Lane Marker |
| R | Right Lane Marker |

#### Base Symbol Ranges (3 tokens)
| Token | Description |
|-------|-------------|
| w | Writing BaseSymbols |
| s | Detailed Location BaseSymbols |
| P | Punctuation BaseSymbols |

#### Modifier Indexes (2 tokens)
| Token | Description |
|-------|-------------|
| i | Fill Modifiers |
| o | Rotation Modifiers |

#### Numbers (1 token)
| Token | Description |
|-------|-------------|
| n | Number from 250 to 749 |

### Pattern Definitions

#### FSW Patterns
```javascript
// Complete FSW sign pattern
fsw_sign = /(A([BLMRS]([0-9]{3}x[0-9]{3}))?(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*)?([BLMRS][0-9]{3}x[0-9]{3}(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})+)/

// Individual symbol pattern
fsw_symbol = /S[123][0-9a-f]{2}[0-5][0-9a-f]/

// Spatial symbol (symbol + coordinates)
fsw_spatial = /S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3}/

// Coordinate pattern
fsw_coord = /[0-9]{3}x[0-9]{3}/
```

#### Number Patterns
```javascript
// FSW numbers (restricted range)
fsw_number = /(2[5-9][0-9]|[3-6][0-9]{2}|7[0-4][0-9])/

// General 3-digit pattern
general_number = /[0-9]{3}/

// Coordinate pair
coordinate_pair = /[0-9]{3}x[0-9]{3}/
```

## Coordinate System and Spatial Layout

### Cartesian Coordinate System

FSW uses a 2D Cartesian coordinate system with specific constraints:

#### Coordinate Range
- **Valid Range**: 250 to 749
- **Total Range**: 500 units
- **Center Point**: Typically around 500x500
- **Precision**: Integer coordinates only

#### Coordinate Format
```
XXXxYYY

Where:
- XXX: X-coordinate (250-749)
- x: Separator character
- YYY: Y-coordinate (250-749)

Examples:
- 489x515 (X=489, Y=515)
- 500x500 (center position)
- 250x250 (minimum coordinates)
- 749x749 (maximum coordinates)
```

### Spatial Relationships

#### Signbox Dimensions
The signbox defines the containing rectangle for all symbols:
```
[Lane][Width]x[Height]

Examples:
- M518x533 (518 units wide, 533 units tall)
- B450x400 (450 units wide, 400 units tall)
- L600x550 (left lane, 600x550)
```

#### Symbol Positioning
Each symbol has precise coordinates within the signbox:
```
S[SymbolKey][X]x[Y]

Where coordinates are relative to signbox origin.

Example Analysis:
M518x533S1870a489x515S18701482x490S20500508x496S2e734500x468

- Signbox: M518x533 (518×533 middle lane)
- Symbol 1: S1870a at 489×515
- Symbol 2: S18701 at 482×490  
- Symbol 3: S20500 at 508×496
- Symbol 4: S2e734 at 500×468
```

## Query Language

### Query Structure

FSW provides a sophisticated query language for searching signs:

#### Query Components
1. **Prefix Query**: Search temporal sequences
2. **Signbox Query**: Search spatial arrangements
3. **Variable Query**: Flexible pattern matching
4. **Style Query**: Visual styling options

### FSW Query Patterns

#### Basic Query Elements
| Token | Variable | Regular Expression |
|-------|----------|-------------------|
| Q | Q | Q |
| T | T | T |
| A | prefix | A(S[123][0-9a-f]{2}[0-5][0-9a-f])+ |
| S | symbol | S[123][0-9a-f]{2}[0-5][0-9a-f] |
| R | range | R[123][0-9a-f]{2}t[123][0-9a-f]{2} |

#### Query Types
```javascript
// Exact symbol search
QS1870a  // Find exact symbol S1870a

// Symbol range search  
QRS1000tS2000  // Find symbols in range S1000 to S2000

// Prefix search
QAS1870aS18701  // Find signs with temporal sequence

// Combined search
QTAS1870aS20500  // Temporal + exact symbols
```

### SWU Query Patterns

Similar structure but using Unicode characters:
```javascript
// SWU query elements use Unicode equivalents
// Structure remains identical to FSW queries
```

## Technology Integration

### Font Technology

#### Font Files
1. **SuttonSignWritingLine.ttf**: Outline/stroke rendering
2. **SuttonSignWritingFill.ttf**: Fill/solid rendering  
3. **SuttonSignWritingOneD.ttf**: One-dimensional Unicode
4. **SuttonSignWritingNull.ttf**: Fallback for invalid characters

#### Font Loading Strategy
```css
@font-face {
  font-family: "SuttonSignWritingLine";
  src: local('SuttonSignWritingLine'),
       url('https://unpkg.com/@sutton-signwriting/font-ttf@1.0.0/font/SuttonSignWritingLine.ttf');
}

@font-face {
  font-family: "SuttonSignWritingFill";
  src: local('SuttonSignWritingFill'),
       url('https://unpkg.com/@sutton-signwriting/font-ttf@1.0.0/font/SuttonSignWritingFill.ttf');
}
```

### SVG Rendering

#### Font-Based SVG
```svg
<svg viewBox="0 0 518 533">
  <text class="sym-fill" style="font-family:'SuttonSignWritingFill';font-size:30px;fill:white;">
    {plane 16 codepoint}
  </text>
  <text class="sym-line" style="font-family:'SuttonSignWritingLine';font-size:30px;fill:black;">
    {plane 15 codepoint}
  </text>
</svg>
```

#### Stand-Alone SVG
```svg
<svg viewBox="0 0 518 533">
  <g transform="translate(489,515)">
    <path d="M10,10 L20,20 M30,30 L40,40" stroke="black" fill="white"/>
  </g>
</svg>
```

### JavaScript Packages

#### @sutton-signwriting/core
- Core functionality library
- Format conversion utilities
- Regular expression patterns
- Symbol processing functions

#### @sutton-signwriting/font-ttf
- TrueType font distribution
- Font loading utilities
- Cross-platform compatibility

#### @sutton-signwriting/font-db
- Symbol database access
- Metadata and properties
- Search and filtering

#### @sutton-signwriting/sgnw-components
- Web components for SignWriting
- Custom HTML elements
- Framework integration

## Font Technology

### Multi-Platform Support

#### Windows, Linux, and Mac
- Direct font installation
- System font integration
- Desktop application support

#### Mac and iOS
- Configuration profiles
- App Store compliance
- iOS Safari support

#### Android
- Custom font loading
- Web font fallbacks
- Android browser support

### CSS Integration

#### Font Loading
```css
/* Font face declarations */
@font-face {
  font-family: "SuttonSignWriting";
  src: url("SuttonSignWritingLine.ttf");
}

/* Usage in styles */
.signwriting {
  font-family: "SuttonSignWriting", monospace;
  font-size: 30px;
}
```

#### Styling Options
```css
/* Color customization */
.sign-red { color: red; }
.sign-blue { color: blue; }

/* Size scaling */
.sign-large { font-size: 48px; }
.sign-small { font-size: 18px; }

/* Background and effects */
.sign-highlighted { background: yellow; }
.sign-shadow { text-shadow: 2px 2px 4px gray; }
```

## Transformations and Conversions

### Format Conversion

#### FSW to Query String
```javascript
// Original FSW
"AS14c20S27106M518x529S14c20481x471S27106503x489"

// Temporal prefix transformation
"QT"              // Query temporal
"A(S14c20S27106)" // Exact temporal sequence

// Signbox transformation  
"Q"               // Query signbox
"S14c20S27106"    // Exact symbols
```

#### Query String to Regular Expression
```javascript
// Query: QS14c20
// Regex: /S14c20/

// Query: QRS100tS200  
// Regex: /S1[0-9a-f]{2}[0-5][0-9a-f]|S2[0-9a-f]{2}[0-5][0-9a-f]/

// Query: QAS14c20S27106
// Regex: /A.*S14c20.*S27106/
```

### Cross-Format Operations

#### FSW ↔ SWU Conversion
```javascript
// FSW to SWU
fsw2swu("M518x533S1870a489x515") 
// → Unicode equivalent with plane 15/16 characters

// SWU to FSW  
swu2fsw("𝠀𝤛𝤵𝤛𝤵𝤛𝤵")
// → "M518x533S1870a489x515"
```

#### Coordinate Transformations
```javascript
// FSW coordinates to pixel positions
fswCoordToPixel(489, 515, signboxWidth, signboxHeight)

// Pixel positions to FSW coordinates  
pixelToFswCoord(x, y, signboxWidth, signboxHeight)
```

## Unicode Considerations

### Unicode Technical Committee

The Unicode Standard version 8.0 includes a Sutton SignWriting block, but this encoding has significant limitations:

#### Issues with Unicode 8 Implementation
- Incomplete symbol coverage
- Broken rendering mechanisms
- Fictional character assignments
- Limited practical utility

### SignWriting in Unicode (SWU) Alternative

SWU provides a comprehensive alternative:

#### Character Assignments
- **Plane 15**: Symbol outlines/strokes
- **Plane 16**: Symbol fills
- **Structural Markers**: Basic SignWriting structure
- **Number Characters**: Coordinate representation

#### Implementation Benefits
- Complete symbol coverage
- Proper rendering support
- Real-world applicability
- Bidirectional FSW conversion

### Character Encoding Details

#### SWU Character Structure
```
Base Character + Fill Character + Structural Markers

Where:
- Base: U+40001 to U+4F428 (symbol shapes)
- Fill: Plane 16 characters (fill patterns)
- Structure: U+1D800-U+1D9FF (layout markers)
```

## Implementation Examples

### Basic FSW Processing

#### Sign Parsing
```javascript
// Parse FSW string
function parseFSW(fswString) {
  const temporal = fswString.match(/^A([BLMRS]([0-9]{3}x[0-9]{3}))?(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*/);
  const spatial = fswString.match(/[BLMRS][0-9]{3}x[0-9]{3}(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})+/);
  
  return {
    temporal: temporal ? temporal[0] : null,
    spatial: spatial ? spatial[0] : null
  };
}
```

#### Symbol Extraction
```javascript
// Extract symbols from FSW
function extractSymbols(fswString) {
  const symbolPattern = /S[123][0-9a-f]{2}[0-5][0-9a-f]/g;
  return fswString.match(symbolPattern) || [];
}

// Extract coordinates
function extractCoordinates(fswString) {
  const coordPattern = /[0-9]{3}x[0-9]{3}/g;
  return fswString.match(coordPattern) || [];
}
```

### SVG Generation

#### Complete FSW to SVG
```javascript
function fswToSVG(fswString, options = {}) {
  const parsed = parseFSW(fswString);
  const symbols = extractSymbols(parsed.spatial);
  const coords = extractCoordinates(parsed.spatial);
  
  let svg = `<svg viewBox="0 0 ${width} ${height}">`;
  
  symbols.forEach((symbol, index) => {
    const [x, y] = coords[index].split('x').map(Number);
    svg += `<text x="${x}" y="${y}" class="sw-symbol">${symbol}</text>`;
  });
  
  svg += '</svg>';
  return svg;
}
```

### Query Processing

#### Query Matching
```javascript
function matchQuery(fswString, queryString) {
  // Convert query to regex
  const regex = queryToRegex(queryString);
  
  // Test against FSW string
  return regex.test(fswString);
}

function queryToRegex(queryString) {
  if (queryString.startsWith('QS')) {
    // Exact symbol search
    const symbol = queryString.substring(2);
    return new RegExp(symbol);
  } else if (queryString.startsWith('QR')) {
    // Range search
    const [start, end] = queryString.substring(2).split('t');
    return new RegExp(`S[${start.slice(1)}-${end.slice(1)}]`);
  }
  // Additional query types...
}
```

## Standards and Compliance

### Technical Standards

#### IETF Internet Draft
- **Document**: draft-slevinski-formal-signwriting-09
- **Status**: Internet Draft (January 2022)
- **Workgroup**: Sutton-Slevinski Collaboration
- **Distribution**: Unlimited

#### ISO Standards
- **ISO 15924**: Script code "Sgnw" for SignWriting
- **Unicode**: Experimental SWU implementation
- **Web Standards**: CSS and SVG integration

### Implementation Guidelines

#### Character Processing
- Use UTF-8 encoding for all text processing
- Implement proper regular expression handling
- Support bidirectional format conversion
- Maintain coordinate range validation

#### Font Handling
- Provide fallback font mechanisms
- Support multiple platform installations
- Implement proper CSS font loading
- Handle font rendering variations

#### Web Integration
- Ensure URL-safe character usage
- Implement proper MIME type handling
- Support responsive design principles
- Provide accessibility features

### Validation Requirements

#### Format Validation
```javascript
// FSW format validation
function validateFSW(fswString) {
  const fswPattern = /^(A([BLMRS]([0-9]{3}x[0-9]{3}))?(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*)?[BLMRS][0-9]{3}x[0-9]{3}(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})+$/;
  return fswPattern.test(fswString);
}

// Coordinate range validation
function validateCoordinates(x, y) {
  return x >= 250 && x <= 749 && y >= 250 && y <= 749;
}

// Symbol key validation
function validateSymbolKey(symbolKey) {
  const symbolPattern = /^S[123][0-9a-f]{2}[0-5][0-9a-f]$/;
  return symbolPattern.test(symbolKey);
}
```

## Conclusion

Formal SignWriting (FSW) represents a sophisticated and comprehensive approach to encoding sign language in digital formats. Its formal language structure, based on mathematical principles and regular expressions, provides a robust foundation for sign language processing, storage, and transmission.

### Key Strengths

1. **Mathematical Foundation**: Rigorous formal language structure
2. **Character Efficiency**: Compact ASCII representation
3. **Regular Expression Support**: Powerful pattern matching capabilities
4. **Cross-Platform Compatibility**: Works across all modern systems
5. **Bidirectional Conversion**: Seamless FSW ↔ SWU transformation
6. **Query Language**: Sophisticated search capabilities
7. **Web Integration**: Complete HTML/CSS/SVG support
8. **Font Technology**: Comprehensive typography solution

### Technical Advantages

- **URL Safety**: All characters are web-safe
- **Database Optimization**: Efficient storage and indexing
- **Processing Speed**: Fast regular expression operations
- **Extensibility**: Modular design allows expansion
- **Standards Compliance**: Based on international standards

### Applications

FSW enables numerous applications:
- **Educational Software**: Sign language learning tools
- **Research Platforms**: Linguistic analysis systems
- **Dictionary Systems**: Comprehensive sign databases
- **Web Publishing**: Online sign language content
- **Mobile Applications**: Cross-platform sign language apps

### Future Considerations

The documentation reveals ongoing development in:
- Unicode standard integration
- Enhanced query capabilities
- Improved font technology
- Extended platform support
- Performance optimizations

This comprehensive analysis demonstrates that FSW provides a complete, technically sound, and practically useful system for digital sign language representation, making it an essential technology for sign language computing applications.

---

*This document provides a complete technical explanation of Formal SignWriting based on the official specifications found in the signwritinglibraries documentation. For implementation details and current updates, refer to the official SignWriting resources and Steve Slevinski's ongoing work.*
