# SuttonSignWriting Library - AI Context Document

## Purpose
This document provides essential context for AI assistants working on the SuttonSignWriting library. It serves as a comprehensive reference for understanding the codebase structure, key concepts, and development patterns to enable effective code assistance and modifications.

## Quick Reference

### Core Library Overview
- **Main Library**: `SuttonSignWriting.js` (2,850 lines) - Core JavaScript library
- **Styling**: `SuttonSignWriting.css` - Font loading and CSS definitions
- **Documentation**: `index.html`, `guide.html`, `draft.html`
- **Character Browser**: `symbols.js`, `messages.js` - Symbol browsing and internationalization
- **Testing**: `components/tests/` - Comprehensive test suite (935+ tests)

### SignWriting Formats Supported
1. **FSW (Formal SignWriting)** - Primary format with explicit positioning
2. **SWU (SignWriting in Unicode)** - Unicode-based representation
3. **KSW (Kolumbus SignWriting)** - Legacy format support
4. **BSW (Binary SignWriting)** - Compact binary format
5. **CSW (Compact SignWriting)** - Space-efficient text format

## Key Development Concepts

### 1. SignWriting Symbols
- **Symbol ID Format**: Base-16 numbers (e.g., S100, S38b)
- **Symbol Categories**: Hand shapes, movements, facial expressions, body positions
- **Symbol Variations**: Each base symbol has multiple variations (fills, rotations)
- **ISWA 2010**: International SignWriting Alphabet standard used

### 2. Coordinate System
- **Lane Format**: Signs positioned in vertical lanes
- **Positioning**: X,Y coordinates for symbol placement
- **Anchor Points**: Symbols have specific anchor points for connections
- **Canvas**: Virtual canvas for sign layout and rendering

### 3. Font System Architecture
```
SuttonSignWritingLine.ttf    - Outline symbols
SuttonSignWritingFill.ttf    - Filled symbols  
SuttonSignWritingOneD.ttf    - One-dimensional symbols
SuttonSignWritingNull.ttf    - Null/placeholder symbols
```

## Code Structure Patterns

### 1. Main Library Functions (SuttonSignWriting.js)

#### Format Conversion Functions
```javascript
// FSW parsing and validation
function fsw2swu(fswText)      // Convert FSW to SWU
function swu2fsw(swuText)      // Convert SWU to FSW
function fsw2query(fswText)    // Convert FSW to search query
function fsw2tokens(fswText)   // Tokenize FSW string

// Symbol processing
function symbol2canvas(symbol) // Render symbol to canvas
function symbols2svg(symbols)  // Convert symbols to SVG
function parseSymbol(symText)  // Parse symbol string
```

#### Canvas and Rendering
```javascript
function swu2png(swuText, options)    // Render SWU to PNG
function fsw2svg(fswText, options)    // Render FSW to SVG
function sign2canvas(signText)        // Render sign to canvas
function columns2svg(columnsText)     // Multi-column layout
```

#### Search and Query
```javascript
function query2fsw(queryText)         // Convert query to FSW
function query2regex(queryText)       // Convert query to regex
function searchRange(min, max)        // Search symbol ranges
```

### 2. Testing Patterns (components/tests/test.ssw.js)

#### Test Structure
```javascript
describe('Format Conversion', function() {
  it('should convert FSW to SWU', function() {
    const fsw = 'M518x529S14c20481x471S27106503x489';
    const expected = '𝠀𝠃𝣰𝣩𝤚𝤦𝦍';
    expect(ssw.fsw2swu(fsw)).to.equal(expected);
  });
});
```

#### Common Test Categories
- Format conversion (FSW ↔ SWU ↔ KSW ↔ BSW ↔ CSW)
- Symbol validation and parsing
- Canvas rendering and SVG generation
- Search query processing
- Font loading and glyph rendering

### 3. Character Browser Patterns

#### Symbol Organization (symbols.js)
```javascript
const symbolCategories = {
  'hand': { range: [0x100, 0x204], name: 'Hand Shapes' },
  'movement': { range: [0x205, 0x2f6], name: 'Movement' },
  'dynamics': { range: [0x2f7, 0x2fe], name: 'Dynamics' },
  // ...additional categories
};
```

#### Internationalization (messages.js)
```javascript
const messages = {
  'en': { 'symbol': 'Symbol', 'search': 'Search' },
  'es': { 'symbol': 'Símbolo', 'search': 'Buscar' },
  // ...additional languages
};
```

## Development Guidelines

### 1. Adding New Format Support
1. Create parsing functions: `format2fsw()` and `fsw2format()`
2. Add validation: `formatRegex` and `formatPattern`
3. Include in conversion matrix
4. Add comprehensive tests
5. Update documentation

### 2. Symbol Processing
- Always validate symbol IDs against ISWA 2010 ranges
- Handle symbol variations (base symbol + variation)
- Consider coordinate transformations for positioning
- Test with various font combinations

### 3. Canvas Rendering
- Use consistent coordinate system
- Handle font loading asynchronously
- Implement proper error handling for missing symbols
- Support multiple output formats (PNG, SVG, Canvas)

### 4. Testing Requirements
- Test all format combinations (FSW ↔ SWU ↔ KSW ↔ BSW ↔ CSW)
- Include edge cases and malformed input
- Verify visual output with known good examples
- Test cross-browser compatibility

## Common Development Tasks

### Adding a New Symbol Category
1. Update symbol ranges in `symbols.js`
2. Add category to character browser interface
3. Include category tests in test suite
4. Update font files if new glyphs needed
5. Document category in guide

### Performance Optimization
- Cache font loading results
- Optimize symbol lookup tables
- Use efficient string parsing
- Implement lazy loading for large symbol sets

### Browser Compatibility
- Test font loading across browsers
- Handle Canvas API differences
- Ensure SVG rendering consistency
- Support touch interfaces for mobile

## Error Handling Patterns

### Input Validation
```javascript
function validateFSW(fswText) {
  if (!fswText || typeof fswText !== 'string') {
    throw new Error('Invalid FSW input: must be non-empty string');
  }
  if (!fswRegex.test(fswText)) {
    throw new Error('Invalid FSW format: ' + fswText);
  }
  return true;
}
```

### Font Loading
```javascript
function loadFont(fontName) {
  return new Promise((resolve, reject) => {
    const font = new FontFace(fontName, `url(${fontPath})`);
    font.load().then(resolve).catch(reject);
  });
}
```

## Integration Points

### With SignPuddle
- Dictionary integration via format conversion
- Search functionality using query patterns
- Sign rendering for display
- Export capabilities to various formats

### With External Systems
- Unicode support for text processing
- SVG output for web display
- PNG generation for documents
- JSON serialization for APIs

## Debugging and Troubleshooting

### Common Issues
1. **Font Loading Failures**: Check font paths and CORS policies
2. **Symbol Rendering**: Verify symbol IDs against ISWA ranges
3. **Format Conversion**: Test with minimal valid examples
4. **Canvas Issues**: Check browser Canvas API support

### Debug Tools
- Browser developer tools for Canvas inspection
- SVG validators for output verification
- Font inspection tools for glyph analysis
- Network tools for font loading monitoring

## Future Development Considerations

### Modernization Opportunities
- Convert to ES6+ modules
- Add TypeScript definitions
- Implement build system (Webpack/Rollup)
- Add automated visual regression testing

### API Improvements
- Consistent promise-based APIs
- Better error messages with context
- Performance monitoring hooks
- Plugin architecture for extensions

### Standards Compliance
- Keep ISWA 2010 compatibility
- Support emerging Unicode standards
- Maintain backward compatibility
- Follow web accessibility guidelines

## Key Files for Common Tasks

### Format Development
- `SuttonSignWriting.js` lines 1-500: Core parsing functions
- `SuttonSignWriting.js` lines 1500-2000: Format conversion matrix
- `components/tests/test.ssw.js`: Format conversion tests

### Symbol Management
- `symbols.js`: Symbol definitions and categories
- `SuttonSignWriting.js` lines 500-1000: Symbol processing
- `assets/`: Font files and symbol resources

### Rendering System
- `SuttonSignWriting.js` lines 2000-2500: Canvas and SVG rendering
- `SuttonSignWriting.css`: Font loading and styling
- `components/tests/`: Visual output tests

### Documentation
- `index.html`: Main documentation and examples
- `guide.html`: User guide and tutorials
- `draft.html`: Technical specifications

This context document should enable effective AI assistance for any development tasks on the SuttonSignWriting library. Refer to the Architecture document for deeper technical analysis and system design details.
