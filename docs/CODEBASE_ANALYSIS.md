SignPuddle 2 Codebase Analysis
Key Files and Their Purpose
signsave.php
Purpose: Allows users to save signs to a puddle (dictionary repository).
Functions:
    • Displays signs with save options
    • Provides buttons for saving to local or remote repositories
    • Links to other sign manipulation features
Dependencies: styleA.php, styleB.php, global.php
Modernization: Convert to SignController with POST endpoint for saving signs and a Svelte SaveSignView component.
signtext.php
Purpose: Server-side component that initializes the SignText editor for creating and editing sign texts (sequences of signs).
Functions:
    • Sets up JavaScript environment variables
    • Initializes the editor with existing sign data
    • Provides interface for manipulating multiple signs in sequence
Dependencies: styleA.php, signtext.js
Modernization: Create a SignTextController in C# and a Svelte SignTextEditor component.
signmaker.php
Purpose: Initializes the SignMaker interface for creating and editing individual signs.
Functions:
    • Sets up JavaScript environment for sign creation
    • Loads icons and symbol palettes
    • Provides interface for symbol manipulation
Dependencies: styleA.php, signmaker.js, keyISWA.js
Modernization: Create a SignEditorController in C# and a Svelte SignMakerEditor component.
msw.php
Purpose: Core library implementing Modern SignWriting encoding and operations.
Functions:
    • isVert(): Tests if text uses lanes (vertical layout)
    • validKey(): Validates symbol keys
    • Includes various conversion functions between sign formats
Dependencies: bsw.php, csw.php, ksw.php, fsw.php
Modernization: Create a C# SignWritingFormatService that implements these format conversions.
database.php
Purpose: Database interface for retrieving symbol information and font data.
Functions:
    • image_txt(): Retrieves text representation of symbols
    • SYM_SIZE class: Manages symbol sizes and dimensions
    • Functions for loading symbol information from database
Dependencies: Database connection, symbol data tables
Modernization: Create a C# SymbolRepository implementing data access patterns.
filesystem.php
Purpose: File system interface for accessing symbol data files.
Functions:
    • loadSymbolGroups(): Loads symbol group information
    • image_svg(), image_png(), image_txt(): Retrieves symbol images in different formats
    • SYM_SIZE class: Similar to database.php but using file storage
Dependencies: File system structure with symbol data files
Modernization: Create a C# FileStorageService with methods for symbol file access.
global.php
Purpose: Core file with global functions, variables and settings.
Functions:
    • displayEntry(): Displays sign entries with formatting
    • displaySWFull(): Shows complete sign information
    • sgnOptions(): Displays sign options
    • Configuration settings and session management
Dependencies: Numerous includes, session variables
Modernization: Split into C# services by functionality (UserService, ConfigurationService, RenderService).
canvas.php
Purpose: Central component for editing and managing signs.
Functions:
    • Handles actions like "Save", "Add", "Edit"
    • Manages sign data including builds, terms, and sequences
    • Processes form submissions for sign editing
Dependencies: Session variables, file system access
Modernization: Create a C# SignEditController with RESTful CRUD endpoints.
JavaScript Files
signmaker.js
Purpose: Client-side functionality for sign creation interface.
Functions:
    • buildControl(): Creates control interface elements
    • symbol(): Creates and manages symbol objects
    • loadSymbols(): Loads symbols from stored data
    • Various symbol manipulation functions (rotate, flip, etc.)
Modernization: Convert to Svelte components with proper state management.
signtext.js
Purpose: Client-side functionality for sign text editing.
Functions:
    • addOutputSign(): Adds signs to the output
    • signDelete(), signReplace(): Manipulates signs in sequence
    • buildOutput(): Generates HTML output for sign texts
Modernization: Convert to Svelte components with stores for state management.
Format Libraries
bsw.php (Binary SignWriting)
Purpose: Handles binary encoding format for SignWriting.
ksw.php (Kartesian SignWriting)
Purpose: Handles coordinate-based SignWriting format.
fsw.php (Formal SignWriting)
Purpose: Handles formal string representation of signs.
csw.php (Character SignWriting)
Purpose: Handles character-based encoding for signs.
Modernization for format libraries: Create a C# SignWritingFormatLibrary with separate classes for each format and converters between them.
Modern Architecture Recommendation
C# API Structure:
    1. Controllers:
        ○ SignController: CRUD operations for signs
        ○ DictionaryController: Dictionary operations
        ○ SymbolController: Symbol retrieval and search
        ○ UserController: User authentication
    2. Services:
        ○ SignService: Business logic for sign operations
        ○ FormatService: Format conversions (BSW, KSW, FSW, CSW)
        ○ RenderService: Generate visual representations
    3. Models:
        ○ Sign: Sign data model
        ○ Symbol: Symbol data model
        ○ Dictionary: Dictionary collection model
    4. Data Access:
        ○ SignRepository: Data access for signs
        ○ SymbolRepository: Data access for symbols
Svelte Frontend Structure:
    1. Components:
        ○ SignEditor.svelte: Interactive sign creation component
        ○ SignText.svelte: Sign text sequence editor
        ○ SymbolPalette.svelte: Symbol selection interface
        ○ DictionaryBrowser.svelte: Browse and search dictionaries
    2. Stores:
        ○ signStore.js: State management for current sign
        ○ symbolStore.js: Symbol palette state
        ○ dictionaryStore.js: Dictionary state
    3. Services:
        ○ api.js: API communication with backend
        ○ signRender.js: Client-side rendering using modern libraries
This architecture would leverage modern web technology like the Sutton SignWriting JavaScript Library (sw10js) and Web Components (sgnw-components) for rendering, while providing a clean separation of concerns between frontend and backend.

There is a folder "SignPuddleNext"with two subfolders in it.  "SignPuddle.API" for the C# API and "signpuddle-web" for the Svelte 3 static website. 

Write all the code for the static website.

