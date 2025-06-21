## Technology Context Files 📚
**IMPORTANT**:  You can use Context7 MCP for technology-specific guidance, patterns, and best practices, always reference:

### Quick Technology Reference 

**Rule**: Check relevant context files BEFORE implementing any technology-specific features!

### SignPuddle Database Technology Stack 🗄️
- **Database ORM**: Entity Framework Core 9.0.5
- **Primary Database**: Azure CosmosDB (NoSQL for SPML documents)
- **Relational Database**: SQL Server (via EF Core 8.0.5)
- **Testing Database**: In-Memory Database (EF Core InMemory provider)
- **API Framework**: ASP.NET Core 9.0 Web API
- **Authentication**: JWT Bearer Authentication (9.0.5)
- **Test Framework**: xUnit 2.9.0 with 89+ comprehensive tests
- **Architecture**: Repository pattern with dependency injection

---

# AI-Assisted Vibe Coding Development Guide
---

## 🎯 Core Mission

You are an AI coding assistant following **vibe coding principles** with **micro-unit development**. Build high-quality software through natural language communication, focusing on atomic functions (max 15 lines) that are independently testable and composable.

---

## 📜 The TFCDC Framework

**T - Thinking** (4 Levels):
- **Logical**: What are we building?
- **Analytical**: How does it work?
- **Computational**: How do we fit logic into configurations?
- **Procedural**: What strategies optimize performance?

**F - Frameworks**: Always specify exact tech stack
**C - Checkpoints**: Use version control at every micro-function
**D - Debugging**: Systematic isolation to specific micro-functions
**C - Context**: Provide maximum context for all requests

---

## 🏗️ 3-Phase Planning (Before Any Code)

### Phase 1: Architecture & Features
Create comprehensive PRD with:
- MVP user flow
- Feature breakdown with tech requirements
- System architecture diagram
- Clarifying questions

### Phase 2: Screen Design & States
Define 3-5 states per screen:
- Loading, error, empty, populated, interactive states
- Motion choreography and responsive feedback

### Phase 3: Technical Specifications
- File system structure
- API endpoints and database schema
- Micro-function breakdown per feature
- Security considerations and edge cases

---

## 🔬 Micro-Unit Development Protocol

### Hard Rules
- **Maximum 15 lines of logic per function**
- **One responsibility per function**
- **Unit test required before integration**
- **Validation checkpoint after each function**

### Implementation Sequence
1. **Atomic Breakdown**: List micro-functions needed (no code)
2. **User Confirmation**: Get approval for breakdown
3. **Single Micro-Function**: Implement one function + tests
4. **Validation**: Show tests passing with real data
5. **Composition**: Combine tested functions (max 5 lines coordination)
6. **Integration Testing**: Realistic end-to-end scenarios

---

## 💬 6-Component Prompt Framework

Every request must include:

**1. ROLE**: Expert in [specific technology] following micro-unit principles
**2. TASK**: Specific objective (implement one micro-function, debug specific issue, etc.)
**3. INPUT**: Exact specifications, error messages, or requirements
**4. OUTPUT**: Detailed format (function + tests, composition, debugging analysis)
**5. CONSTRAINTS**: What NOT to do (don't write multiple functions, don't skip tests, etc.)
**6. CAPABILITIES**: Available tools, project context, existing micro-function library

---

## 🧪 Testing Strategy

### Micro-Function Tests (Required)
- **3 minimum test cases**: Happy path, edge case, error condition
- **Isolation testing**: No external dependencies
- **Real data demonstration**: Show function working with actual inputs

### Composition Tests
- **Integration scenarios**: Realistic user workflows
- **Error propagation**: Verify errors handled across function boundaries
- **Performance validation**: Ensure composed functions meet requirements

---

## 🚨 Debugging Protocol

When issues arise:
1. **Isolate**: Identify specific failing micro-function
2. **Test Independence**: Verify function works alone
3. **Minimal Fix**: Modify only the problematic function
4. **Validate**: Test fix in isolation then integration
5. **Document**: Record solution for future reference

---

## 📋 Essential Prompt Templates

### Project Initialization
```
ROLE: Senior [TECH_STACK] developer expert in micro-unit architecture
TASK: Create PRD and technical architecture for [PROJECT_CONCEPT]
INPUT: [Brief app description, target users, key features]
OUTPUT: Complete PRD with micro-function breakdown and milestone plan
CONSTRAINTS: Design for MVP first, plan for 10 to 500k+ user scalability
CAPABILITIES: Modern frameworks, scalable architecture, AI-assisted patterns
```

### Micro-Function Implementation
```
ROLE: [LANGUAGE] expert following micro-unit development
TASK: Implement ONLY [FUNCTION_NAME] micro-function
INPUT: [Single responsibility description, parameters, expected output]
OUTPUT: Function (max 15 lines) + unit tests + real data demo
CONSTRAINTS: One function only, no integration, must pass all tests
CAPABILITIES: Project patterns, testing framework, existing micro-functions

VALIDATION REQUIRED:
✅ Function compiles and passes tests
✅ Works with real test data  
✅ Single responsibility confirmed
✅ Wait for approval before next function
```

### Composition Function
```
ROLE: Integration expert for micro-function composition
TASK: Combine [FUNCTION_A] + [FUNCTION_B] + [FUNCTION_C]
INPUT: Previously tested micro-functions
OUTPUT: Composition (max 5 lines) + integration tests + realistic demo
CONSTRAINTS: Coordination only, no new business logic
CAPABILITIES: All tested micro-functions, integration testing tools
```

### Debugging Session
```
ROLE: Debugging specialist for micro-function architectures
TASK: Fix issue in [SPECIFIC_COMPONENT]
INPUT: Error message: [FULL_ERROR], current vs expected behavior
OUTPUT: Root cause analysis + minimal fix + validation
CONSTRAINTS: Fix only failing micro-function, maintain existing functionality
CAPABILITIES: Complete codebase, debugging tools, version control history
```

### SignPuddle Database-Specific Templates

#### SPML Import Function
```
ROLE: Entity Framework Core expert specializing in SPML data migration
TASK: Implement [SPML_FUNCTION_NAME] for legacy data import
INPUT: SPML XML structure, target entity mapping, validation requirements
OUTPUT: Micro-function (max 15 lines) + xUnit tests + real SPML data demo
CONSTRAINTS: Preserve 100% data integrity, handle malformed XML gracefully
CAPABILITIES: SpmlImportService, SpmlRepository, CosmosDB integration

VALIDATION REQUIRED:
✅ SPML XML parsing works with test files
✅ Data integrity preserved (timestamps, user attribution)
✅ Error handling for invalid/incomplete data
✅ Performance acceptable for large files
```

#### Repository Method Implementation
```
ROLE: Repository pattern expert for SignPuddle database architecture
TASK: Implement [REPOSITORY_METHOD] in [ENTITY]Repository
INPUT: Entity type, query requirements, expected return type
OUTPUT: Repository method (max 15 lines) + integration tests + real data
CONSTRAINTS: Follow existing repository patterns, async/await required
CAPABILITIES: ApplicationDbContext, existing repository interfaces, EF Core

VALIDATION REQUIRED:  
✅ Method follows repository pattern consistently
✅ Async operations with proper error handling
✅ Integration tests with real database
✅ Null reference safety with C# 12 nullable types
```

#### Database Entity Enhancement
```
ROLE: Entity Framework Core domain modeling expert
TASK: Enhance [ENTITY_NAME] entity with [NEW_PROPERTY/RELATIONSHIP]
INPUT: Entity requirements, relationship type, validation rules
OUTPUT: Entity update + migration + tests + relationship validation
CONSTRAINTS: Maintain existing relationships, follow naming conventions
CAPABILITIES: EF Core 9.0.5, SQL Server, CosmosDB, existing entity relationships

VALIDATION REQUIRED:
✅ Entity relationships properly configured
✅ Database migration generated and tested
✅ Existing data compatibility maintained
✅ Repository methods updated accordingly
```

#### Performance Optimization Function
```
ROLE: Database performance optimization specialist for Entity Framework
TASK: Optimize [QUERY/OPERATION] performance in [COMPONENT]
INPUT: Current implementation, performance requirements, bottleneck analysis
OUTPUT: Optimized function + performance tests + benchmark comparison
CONSTRAINTS: Maintain functionality, improve specific performance metric
CAPABILITIES: EF Core query optimization, async patterns, performance testing

VALIDATION REQUIRED:
✅ Performance improvement measurable and documented
✅ Functionality unchanged (all existing tests pass)
✅ Memory usage within acceptable limits
✅ Concurrent operation support maintained
```

---

## ✅ Quality Gates

### Per Micro-Function
- [ ] Single responsibility (describable in one sentence)
- [ ] Under 15 lines of logic
- [ ] 3+ passing unit tests
- [ ] Works in isolation
- [ ] Clear, intention-revealing name

### Per Composition
- [ ] Coordinates without adding business logic
- [ ] Integration tests pass
- [ ] Error handling across boundaries
- [ ] Performance meets requirements

### Per Feature
- [ ] All micro-functions tested
- [ ] End-to-end scenarios work
- [ ] Meets PRD requirements
- [ ] Maintainable and debuggable

### SignPuddle Database Quality Gates

#### Per Database Entity
- [ ] Follows Entity Framework conventions (PascalCase, proper relationships)
- [ ] Nullable reference types properly configured (C# 12)
- [ ] Primary key and foreign key relationships defined
- [ ] Repository interface and implementation created
- [ ] Unit tests for all CRUD operations with real data

#### Per Repository Method
- [ ] Async/await pattern consistently applied
- [ ] Proper null checking and argument validation
- [ ] Returns appropriate types (Task<T>, Task<List<T>>, etc.)
- [ ] Error handling with meaningful exception messages
- [ ] Integration tests with ApplicationDbContext

#### Per SPML Operation
- [ ] Preserves original data integrity (timestamps, user attribution)
- [ ] Handles malformed or incomplete XML gracefully
- [ ] Performance acceptable for production SPML file sizes
- [ ] Export functionality maintains backward compatibility
- [ ] Comprehensive error logging for debugging

#### Per API Controller Method
- [ ] Proper dependency injection of repositories/services
- [ ] HTTP status codes correctly applied (200, 400, 404, 500)
- [ ] Request/response models properly defined
- [ ] Authentication/authorization requirements enforced
- [ ] Swagger documentation complete and accurate

---

## 🚀 Session Workflow

### Pre-Development Checklist
- [ ] 95% confidence in requirements
- [ ] Framework/tech stack specified
- [ ] **Context files reviewed** for relevant technologies
- [ ] Version control ready
- [ ] PRD and architecture available
- [ ] Testing strategy confirmed

### Development Session
1. **Context Loading**: Project state, existing micro-functions, coding standards, **relevant technology context files**
2. **Atomic Planning**: Break feature into micro-functions (no code)
3. **Sequential Implementation**: One micro-function at a time with validation
4. **Progressive Composition**: Build up through tested combinations
5. **Integration Validation**: End-to-end testing with realistic scenarios

### Session Template
```  
VIBE CODING SESSION: SignPuddle Database Development
User: JonathanDDuncan
Date: 2025-06-21
Mode: [Planning/Implementation/Debugging]

TFCDC CHECK:
✅ Thinking: Four levels applied
✅ Frameworks: EF Core 9.0.5, ASP.NET Core 9.0, CosmosDB, SQL Server
✅ Checkpoints: Version control ready, 89+ tests passing
✅ Debugging: Systematic approach prepared  
✅ Context: SignPuddle database architecture loaded

CURRENT STATE:
- Database Architecture: ✅ Complete (5 entities, repositories, dual DB)
- SPML Integration: ✅ Complete (import/export, legacy compatibility)
- Testing Framework: ✅ Complete (89+ tests, real data validation)
- API Integration: ✅ Complete (8 controllers, JWT auth, Swagger)

OBJECTIVE: [SPECIFIC_DATABASE_GOAL]
APPROACH: Micro-unit development with EF Core best practices

Ready to proceed with atomic breakdown and sequential implementation.
```

---

## 🗄️ SignPuddle Database Context

### Current Architecture Status
The SignPuddle database implementation is **production-ready** with comprehensive testing and modern architecture patterns. The system successfully migrates from legacy PHP/XML storage to Entity Framework Core while maintaining 100% backward compatibility.

### Key Success Metrics
- **89+ Tests Passing**: Comprehensive coverage across all layers
- **Dual Database Support**: SQL Server for relational data, CosmosDB for documents  
- **Performance Validated**: Sub-2-second operations with concurrent support
- **Legacy Compatibility**: 100% SPML import/export functionality
- **Security Implemented**: JWT authentication with proper validation

### Next Development Areas
1. **Caching Layer**: Redis implementation for frequently accessed data
2. **Advanced Search**: Full-text search capabilities
3. **Monitoring**: Performance metrics and usage analytics
4. **Backup Strategy**: Automated backup and recovery procedures

---

**Key Principle**: Always confirm atomic breakdown before writing any code. Implement one micro-function at a time with complete testing. Build features through composition of verified atomic units while leveraging AI capabilities for exponential development velocity.

## Overview

The SignPuddle 2.0 database modernization project focuses on migrating from legacy PHP/XML file-based storage to a modern Entity Framework Core architecture. This implementation maintains 100% backward compatibility with SPML (SignPuddle Markup Language) while providing scalable, testable database operations.

## Requirements Summary

### Core Database Requirements
- **Legacy Migration**: Complete SPML import/export functionality with data preservation
- **Multi-Database Support**: CosmosDB for document storage, SQL Server for relational data
- **Testing Excellence**: No-mocking approach with real database operations (89+ tests)
- **Performance**: Sub-2-second async operations with concurrent import support
- **Security**: JWT authentication with proper data validation and SQL injection prevention

### Key Entities Implemented
- **User**: Authentication and profile management
- **Dictionary**: SignWriting dictionary/puddle management  
- **Sign**: Individual sign storage with FSW notation
- **Symbol**: SignWriting symbol metadata and rendering
- **SpmlDocument**: Legacy SPML document storage in CosmosDB

# Implementation Guide

## Project Overview

### Current Implementation Status
- ✅ **Database Architecture**: Complete EF Core setup with dual database support
- ✅ **Repository Pattern**: Full CRUD operations for all entities
- ✅ **SPML Import System**: XML-to-database conversion with validation
- ✅ **API Integration**: 8 controllers with comprehensive endpoint coverage
- ✅ **Testing Framework**: 89+ tests across units, integration, performance, and error handling
- ✅ **Legacy Compatibility**: 100% SPML format preservation and export capability

### Project Structure
```
src/SignPuddle.API/
├── Controllers/         # 8 API controllers with full CRUD operations
├── Data/               # ApplicationDbContext and repositories
├── Models/             # 5 core entities with proper relationships
├── Services/           # Business logic and format conversion services
└── Program.cs          # DI configuration and startup

src/SignPuddle.API.Tests/
├── Controllers/        # API endpoint testing
├── Services/           # Business logic unit tests
├── Data/Repositories/  # Repository integration tests
├── Performance/        # Scalability and performance validation
├── Integration/        # End-to-end workflow tests
└── ErrorHandling/      # Comprehensive error scenario coverage
```

## Implementation Tasks

### Priority 1: Database Foundation ✅ COMPLETE
- [x] Entity Framework Core setup with dual database support
- [x] Repository pattern implementation with dependency injection
- [x] All 5 core entities with proper relationships
- [x] Database context configuration for both SQL Server and CosmosDB
- [x] Migration support and schema versioning

### Priority 2: SPML Legacy Integration ✅ COMPLETE  
- [x] SPML XML parsing with validation
- [x] Legacy data import with user attribution preservation
- [x] Export functionality for backward compatibility
- [x] Performance optimization for large SPML files
- [x] Error handling for malformed or incomplete data

### Priority 3: Testing Excellence ✅ COMPLETE
- [x] Comprehensive test suite (89+ tests) with real data
- [x] No-mocking approach using actual database operations
- [x] Performance testing with concurrent operation validation
- [x] Error handling tests for all edge cases
- [x] Integration tests for complete workflows

### Priority 4: API Integration ✅ COMPLETE
- [x] 8 controllers with proper dependency injection
- [x] JWT authentication and authorization
- [x] Swagger/OpenAPI documentation
- [x] Health check endpoints
- [x] Global exception handling middleware

## Technical Notes

### Database Design Patterns
- **Repository Pattern**: Abstracts data access with proper interfaces
- **Unit of Work**: EF Core DbContext provides transaction management
- **Dependency Injection**: All repositories and services properly injected
- **Async/Await**: Non-blocking database operations throughout

### Testing Philosophy
- **Real Database Operations**: No mocking for authentic validation
- **Comprehensive Coverage**: Units, integration, performance, error handling
- **Real Data Testing**: Uses actual SPML files from legacy system
- **Performance Validation**: Concurrent operations and memory management

### SPML Integration Strategy
- **Incremental Import**: File-at-a-time processing with validation
- **Data Preservation**: 100% backward compatibility maintained
- **Metadata Handling**: Custom metadata storage and retrieval
- **Export Capability**: XML generation for legacy system compatibility

### Performance Considerations
- **Async Operations**: All database calls use async/await patterns
- **Connection Pooling**: EF Core manages connection efficiency
- **Query Optimization**: Proper indexing and LINQ query patterns
- **Memory Management**: Bulk operations validated for memory usage

### Security Implementation
- **JWT Authentication**: Token-based API security
- **Input Validation**: Comprehensive parameter validation
- **SQL Injection Prevention**: Parameterized queries throughout
- **Owner-Based Access**: Resource ownership validation

---

