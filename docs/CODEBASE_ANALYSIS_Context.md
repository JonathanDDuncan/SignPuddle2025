# SignPuddle 2 Architecture Summary

## Overview
SignPuddle 2 is a web application for creating and managing SignWriting dictionaries. SignWriting is a visual notation system for sign languages invented by Valerie Sutton in 1974.

**Core Features:**
- Dictionary creation and management
- Visual sign editor (SignMaker)
- Text composition with sign sequences (SignText)
- Multi-format SignWriting support (FSW, KSW, BSW, CSW)
- Search across terms, signs, and symbols

## Current Architecture (Legacy PHP)

### Technology Stack
- **Backend**: PHP 5.x/7.x, Apache, minimal SQLite
- **Frontend**: Custom JavaScript, DynAPI (deprecated), jQuery-style DOM manipulation
- **Storage**: File-based XML (SPML format), session files
- **Dependencies**: ImageMagick, ISWA symbol data

### Key Files & Components
- `global.php` (1,560 lines): Central config, routing, utilities
- `canvas.php` (421 lines): Main sign editing interface
- Search modules: `searchword.php`, `searchsign.php`, `searchsymbol.php`
- Format libraries: `fsw.php`, `ksw.php`, `bsw.php`, `csw.php`, `msw.php`
- JavaScript editors: `signmaker.js` (712 lines), `signtext.js`

### Data Models

#### SPML Dictionary Format (XML)
```xml
<spml root="..." type="sgn" puddle="4">
  <entry id="1" cdt="1311183542" mdt="1311183729" usr="Val">
    <term>FSW_NOTATION_HERE</term>
    <term><![CDATA[text term]]></term>
    <text><![CDATA[definition]]></text>
    <video><![CDATA[embedded html]]></video>
    <src><![CDATA[source]]></src>
  </entry>
</spml>
```

#### SignWriting Formats
- **FSW**: `M525x535S10000492x467S26500508x534` (Primary storage)
- **KSW**: Coordinate-based for layout calculations
- **BSW**: Binary encoding for compression
- **CSW**: Character-based encoding

## Critical Issues

### Architecture Problems
1. **Session-heavy state**: Extensive `$_SESSION` usage, memory intensive
2. **Monolithic files**: Mixed concerns, tight coupling
3. **File-based storage**: No ACID compliance, concurrent access issues
4. **No caching**: XML parsed on every request

### Security Vulnerabilities
1. **No input validation**: Direct user input inclusion
2. **XSS vulnerabilities**: Unescaped output throughout
3. **Backdoor mechanism**: `$_REQUEST['backdoor']` in `styleA.php`
4. **Session management**: Unencrypted file storage, no timeouts

### Performance Bottlenecks
1. **XML processing**: Full dictionary parsing per request
2. **Memory usage**: Large session objects, scales poorly
3. **No indexing**: Linear search through files
4. **Capacity limits**: ~10-20 concurrent users, ~1K-5K entries

## Modern Architecture Recommendation

### Technology Stack
- **Frontend**: Svelte 4+ SPA with TypeScript, Vite build
- **Backend**: C# ASP.NET Core 8+ Web API
- **Database**: PostgreSQL + Redis cache
- **Infrastructure**: Containerized deployment

### Architecture Pattern
```
Svelte SPA ←→ C# Web API ←→ PostgreSQL/Redis
```

### Database Schema (Proposed)
```sql
Dictionaries: Id, Name, Language, OwnerId, Created
Signs: Id, DictionaryId, Fsw, Gloss, Created
Users: Id, Username, Email, SecurityLevel
Symbols: Key, Category, Group, Name, SvgPath
```

## Migration Strategy

### Phase 1 (Months 1-3): Foundation
- Git setup, CI/CD, testing framework
- Complete data model analysis
- Security audit and standards
- Database schema design

### Phase 2 (Months 4-6): Backend API
- REST API development with C#
- Database implementation
- Format conversion services
- Data migration scripts

### Phase 3 (Months 7-9): Frontend
- Svelte component architecture
- State management implementation
- PWA capabilities
- Mobile-responsive design

### Phase 4 (Months 10-11): Integration
- Comprehensive testing
- Performance optimization
- Data migration execution
- Deployment procedures

### Phase 5 (Month 12): Go-Live
- Production deployment
- User training and documentation
- Monitoring and support setup

## Risk Assessment

### High Risk
- **Data migration complexity**: SPML→DB conversion
- **Security vulnerabilities**: Current system has multiple issues
- **User adoption**: Interface changes may face resistance

### Medium Risk
- **Performance regression**: Must maintain/improve speed
- **Browser compatibility**: Modern features may limit access
- **Search functionality**: Complex requirements with large datasets

## Key References

### Core Business Logic Files
- `canvas.php`: Entry CRUD operations, sign editing
- `signmaker.js`: Visual sign creation interface
- `fsw.php`, `ksw.php`: Format conversion libraries
- `searchword.php`, `searchsign.php`: Search implementations

### Data Format Specifications
- **[FSW Format Guide](FSW-Context.md)** - Complete FSW specification and patterns
- **[SuttonSignWriting Library](SuttonSignWriting-Context.md)** - Core library integration guide
- SPML DTD: `http://www.signpuddle.net/spml_1.6.dtd`
- ISWA symbol data: `iswa.sql3` database
- Session structure: `$_SESSION['SGN']`, `$_SESSION['UI']`

### Security Concerns
- Input validation needed in all `$_REQUEST` usage
- XSS fixes required in output generation
- Authentication system in `global.php` lines 800-900
- Backdoor removal needed in `styleA.php`

## Development Priorities

### Immediate (0-3 months)
1. Security hardening: Input validation, XSS fixes
2. Basic monitoring and error handling
3. XML caching implementation

### Short-term (3-6 months)
1. Database backend for new data
2. Modern JavaScript library migration
3. Improved user experience flows

### Long-term (6+ months)
1. Complete technology stack modernization
2. Comprehensive testing framework
3. Production deployment with monitoring

---

*This summary references the full analysis in `CODEBASE_ANALYSIS_Context.md` for detailed technical specifications, code examples, and implementation guidance.*
