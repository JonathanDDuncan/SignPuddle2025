# FSW (Formal SignWriting) - AI Context Document

*Created: June 4, 2025*  
*Purpose: AI context for understanding existing SignWriting code and writing new related functionality*

## Quick Reference for AI Development

### Essential Concepts Summary

**FSW** is a formal language encoding system for SignWriting that represents signs as character strings using ASCII characters: `ABLMRS0123456789xabcdef`

**Key Principle**: Every sign = **Time** (temporal prefix) + **Space** (spatial signbox)

### Core Data Structures

#### 1. FSW Sign Format
```
[TemporalPrefix][SpatialSignbox]

Examples:
AS14c20S27106M518x529S14c20481x471S27106503x489
                 ↑temporal    ↑spatial signbox
```

#### 2. Symbol Structure
```
S[Base][Fill][Rotation]

S1870a = S + 187 + 0 + a
         ↑   ↑    ↑   ↑
         |   |    |   rotation (0-f)
         |   |    fill (0-5)
         |   base symbol (123xxx)
         symbol marker
```

#### 3. Coordinate System
```
[Width]x[Height] → 518x533 (signbox dimensions)
[X]x[Y]         → 489x515 (symbol position)

Valid range: 250-749 (500 possible values)
```

## Common Code Patterns

### 1. Regular Expression Patterns

#### Basic Validation
```javascript
// FSW symbol pattern
const FSW_SYMBOL = /S[123][0-9a-f]{2}[0-5][0-9a-f]/;

// FSW coordinate pattern  
const FSW_COORD = /[0-9]{3}x[0-9]{3}/;

// Complete FSW sign pattern
const FSW_SIGN = /(A([BLMRS]([0-9]{3}x[0-9]{3}))?(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*)?([BLMRS][0-9]{3}x[0-9]{3}(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})+)/;
```

#### Parsing Components
```javascript
// Extract symbols
const symbols = fswString.match(/S[123][0-9a-f]{2}[0-5][0-9a-f]/g);

// Extract coordinates
const coords = fswString.match(/[0-9]{3}x[0-9]{3}/g);

// Extract temporal prefix
const temporal = fswString.match(/^A([BLMRS]([0-9]{3}x[0-9]{3}))?(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*/);

// Extract spatial signbox
const spatial = fswString.match(/[BLMRS][0-9]{3}x[0-9]{3}(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})+/);
```

### 2. Format Conversion Patterns

#### FSW ↔ SWU Conversion
```javascript
// FSW to SWU conversion pattern
function fsw2swu(fswText) {
  // Character mapping: ASCII → Unicode
  // S → U+40001+ range
  // Numbers → Unicode equivalents
  // Structure preserved
}

// SWU to FSW conversion pattern  
function swu2fsw(swuText) {
  // Reverse mapping: Unicode → ASCII
  // Maintains semantic equivalence
}
```

#### Query Generation
```javascript
// FSW to query patterns
"AS14c20S27106M518x529..." → "QT" // temporal query
"M518x529S14c20481x471..." → "Q"  // spatial query
"S14c20"                   → "QS14c20" // symbol query
```

### 3. Common Validation Functions

```javascript
// Coordinate validation
function isValidCoord(x, y) {
  return x >= 250 && x <= 749 && y >= 250 && y <= 749;
}

// Symbol validation
function isValidSymbol(symbol) {
  return /^S[123][0-9a-f]{2}[0-5][0-9a-f]$/.test(symbol);
}

// FSW format validation
function isValidFSW(fswString) {
  return FSW_SIGN.test(fswString);
}
```

## Symbol System Reference

### Symbol Categories (ISWA 2010)
```
Total: 37,811 symbols organized in categories:

Category 01: Hand - S100-S204  (handshapes)
Category 02: Movement - S205-S2f6 (movement arrows)  
Category 03: Dynamics - S2f7-S2fe (dynamic indicators)
Category 04: Head - S300-S36c (facial expressions)
Category 05: Body - S36d-S376 (body positions)
Category 06: Limbs - S377-S37e (arm/leg positions)
Category 07: Location - S37f-S386 (spatial markers)
Category 08: Punctuation - S387-S38b (text flow)
```

### Symbol Modifiers
```javascript
// Fill modifiers (6th character)
'0': outline only
'1': solid fill  
'2': thick outline
'3': thick solid
'4': dotted outline
'5': dotted solid

// Rotation modifiers (7th character)  
'0'-'7': standard rotations (0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°)
'8'-'f': mirrored rotations (flipped versions)
```

## Common Data Processing Operations

### 1. Sign Parsing
```javascript
function parseFSW(fswString) {
  return {
    temporal: extractTemporal(fswString),
    spatial: extractSpatial(fswString),
    symbols: extractSymbols(fswString),
    coordinates: extractCoordinates(fswString),
    signbox: extractSignbox(fswString)
  };
}
```

### 2. Symbol Extraction
```javascript
function extractSymbols(fswString) {
  return fswString.match(/S[123][0-9a-f]{2}[0-5][0-9a-f]/g) || [];
}

function extractCoordinates(fswString) {
  return fswString.match(/[0-9]{3}x[0-9]{3}/g) || [];
}
```

### 3. Signbox Processing
```javascript
function extractSignbox(fswString) {
  const match = fswString.match(/[BLMRS]([0-9]{3}x[0-9]{3})/);
  if (match) {
    const [lane, dimensions] = [match[0][0], match[1]];
    const [width, height] = dimensions.split('x').map(Number);
    return { lane, width, height, dimensions };
  }
  return null;
}
```

## Query Language Patterns

### Query Types
```javascript
// Exact symbol search
"QS1870a"           // Find signs containing symbol S1870a

// Symbol range search
"QRS100tS200"       // Find symbols in range S100 to S200

// Temporal sequence search  
"QAS1870aS18701"    // Find signs with temporal sequence

// Combined queries
"QTAS1870aS20500"   // Temporal + specific symbols
```

### Query Processing
```javascript
function processQuery(queryString) {
  if (queryString.startsWith('QS')) {
    return exactSymbolSearch(queryString.substring(2));
  } else if (queryString.startsWith('QR')) {
    return rangeSearch(queryString.substring(2));
  } else if (queryString.startsWith('QA')) {
    return temporalSearch(queryString.substring(2));
  } else if (queryString.startsWith('QT')) {
    return temporalOnlySearch();
  }
  return generalSearch();
}
```

## Technology Integration Patterns

### 1. Font Loading
```css
@font-face {
  font-family: 'SuttonSignWritingLine';
  src: url('SuttonSignWritingLine.ttf');
}

@font-face {
  font-family: 'SuttonSignWritingFill';  
  src: url('SuttonSignWritingFill.ttf');
}
```

### 2. SVG Generation
```javascript
function fswToSVG(fswString, options = {}) {
  const parsed = parseFSW(fswString);
  const { width = 500, height = 500 } = options;
  
  let svg = `<svg viewBox="0 0 ${width} ${height}" xmlns="http://www.w3.org/2000/svg">`;
  
  // Add symbols with positioning
  parsed.symbols.forEach((symbol, i) => {
    const coord = parsed.coordinates[i];
    const [x, y] = coord.split('x').map(Number);
    svg += `<text x="${x}" y="${y}" class="sw-symbol">${symbol}</text>`;
  });
  
  svg += '</svg>';
  return svg;
}
```

### 3. Web Component Patterns
```javascript
class SignWritingDisplay extends HTMLElement {
  static get observedAttributes() { 
    return ['fsw', 'size', 'styling']; 
  }
  
  attributeChangedCallback(name, oldValue, newValue) {
    if (name === 'fsw') {
      this.renderSign(newValue);
    }
  }
  
  renderSign(fswString) {
    // Parse FSW and render
    const parsed = parseFSW(fswString);
    this.innerHTML = this.generateHTML(parsed);
  }
}
```

## Error Handling Patterns

### Common Validation Checks
```javascript
function validateFSWInput(input) {
  const errors = [];
  
  // Check basic format
  if (!FSW_SIGN.test(input)) {
    errors.push('Invalid FSW format');
  }
  
  // Check coordinate ranges
  const coords = extractCoordinates(input);
  coords.forEach(coord => {
    const [x, y] = coord.split('x').map(Number);
    if (!isValidCoord(x, y)) {
      errors.push(`Invalid coordinate: ${coord}`);
    }
  });
  
  // Check symbol validity
  const symbols = extractSymbols(input);
  symbols.forEach(symbol => {
    if (!isValidSymbol(symbol)) {
      errors.push(`Invalid symbol: ${symbol}`);
    }
  });
  
  return errors;
}
```

## Performance Considerations

### 1. Regex Optimization
```javascript
// Pre-compile frequently used patterns
const COMPILED_PATTERNS = {
  symbol: /S[123][0-9a-f]{2}[0-5][0-9a-f]/g,
  coord: /[0-9]{3}x[0-9]{3}/g,
  sign: /(A([BLMRS]([0-9]{3}x[0-9]{3}))?(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*)?([BLMRS][0-9]{3}x[0-9]{3}(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})+)/
};
```

### 2. Caching Strategies
```javascript
// Cache parsed results
const parseCache = new Map();

function cachedParseFSW(fswString) {
  if (parseCache.has(fswString)) {
    return parseCache.get(fswString);
  }
  
  const result = parseFSW(fswString);
  parseCache.set(fswString, result);
  return result;
}
```

## Testing Patterns

### Sample Data for Testing
```javascript
const TEST_SIGNS = {
  simple: "M518x533S1870a489x515",
  complex: "AS14c20S27106M518x529S14c20481x471S27106503x489",
  temporal: "AS1870aS18701S20500",
  spatial: "B450x400S1870a225x375S18701275x350"
};

const TEST_SYMBOLS = ["S100a0", "S1870a", "S27106", "S20500"];
const TEST_COORDS = ["250x250", "500x500", "749x749"];
```

### Common Test Cases
```javascript
describe('FSW Processing', () => {
  test('should parse valid FSW strings', () => {
    const result = parseFSW(TEST_SIGNS.simple);
    expect(result.symbols).toHaveLength(1);
    expect(result.coordinates).toHaveLength(1);
  });
  
  test('should validate symbol format', () => {
    expect(isValidSymbol('S1870a')).toBe(true);
    expect(isValidSymbol('X1870a')).toBe(false);
  });
  
  test('should handle coordinate ranges', () => {
    expect(isValidCoord(500, 500)).toBe(true);
    expect(isValidCoord(100, 100)).toBe(false);
  });
});
```

## Integration with Existing Libraries

### 1. SuttonSignWriting.js Integration
```javascript
// If using the existing SuttonSignWriting library
import { fsw } from './SuttonSignWriting.js';

// Common operations
const isValid = fsw.parse(fswString).valid;
const symbols = fsw.symbols(fswString);  
const query = fsw.query('QS1870a');
```

### 2. Database Storage Patterns
```sql
-- FSW storage in databases
CREATE TABLE signs (
  id INTEGER PRIMARY KEY,
  fsw_string TEXT NOT NULL,
  symbols TEXT[], -- extracted symbols
  temporal_part TEXT, -- temporal prefix if any
  spatial_part TEXT NOT NULL, -- spatial signbox
  width INTEGER, -- signbox width
  height INTEGER -- signbox height
);

-- Indexing for performance
CREATE INDEX idx_signs_symbols ON signs USING GIN(symbols);
CREATE INDEX idx_signs_fsw ON signs(fsw_string);
```

## AI Development Guidelines

### When Working with FSW Code:

1. **Always validate input** using established patterns
2. **Use pre-compiled regex** for performance
3. **Handle coordinate ranges** (250-749) explicitly
4. **Consider both FSW and SWU formats** for compatibility
5. **Cache parsing results** for repeated operations
6. **Test with edge cases** (minimum/maximum coordinates, all symbol types)

### Key Files to Reference:
- `signwritinglibraries/documentation/Formal SignWriting.html` - Complete specification
- `docs/FSW-Explanation.md` - Technical explanation
- `SuttonSignWriting.js` - Reference implementation patterns

### Common Mistakes to Avoid:
- Using invalid coordinate ranges (outside 250-749)
- Not handling temporal prefix as optional
- Forgetting symbol modifier validation (fill 0-5, rotation 0-f)
- Mixing FSW and SWU character sets
- Not escaping regex special characters in queries

---

*This context document provides essential patterns and references for AI assistants working on SignWriting-related code. Always refer to the complete specifications in the documentation folder for authoritative information.*
