# SignPuddle 2.0

A modern web application for creating and managing SignWriting dictionaries. SignWriting is a visual notation system for sign languages invented by Valerie Sutton in 1974.

## 🚀 Quick Start

### Backend (C# API)
```bash
cd src/SignPuddle.API
dotnet run
```

### Frontend (Svelte)
```bash
cd src/signpuddle-web
npm install
npm run dev
```

### Testing
```bash
# Run all backend tests
dotnet test src/SignPuddle.API.sln

# Run frontend tests
cd src/signpuddle-web && npm test

# See full testing guide
→ docs/TESTING_STRATEGY_Context.md
```

## 📚 Documentation

### Core Documentation
- **[Testing Strategy](docs/TESTING_STRATEGY_Context.md)** - Complete testing approach and guidelines
- **[Implementation Status](docs/Implementation_Status.md)** - Current development progress
- **[Codebase Analysis](docs/CODEBASE_ANALYSIS.md)** - Detailed architecture analysis
- **[Product Requirements](docs/SignPuddle_PRD_Context.md)** - Product specifications

### Technical Guides
- **[FSW Format](docs/FSW-Context.md)** - Formal SignWriting specification
- **[SPML Import](SPML_IMPORT_SUMMARY.md)** - Data migration documentation
- **[SuttonSignWriting Library](docs/SuttonSignWriting-Context.md)** - Core library integration

### Development Resources
- **[Architecture Summary](docs/CODEBASE_ANALYSIS_Context.md)** - Quick architecture overview
- **[Documentation Summaries](docs/summaries/)** - Condensed technical summaries

## 🧪 Testing & Quality

SignPuddle follows a **no-mocking** testing philosophy for real-world reliability:

- **✅ Unit Tests**: Real implementations with lightweight dependencies
- **✅ E2E Tests**: Complete HTTP endpoints via TestWebApplicationFactory  
- **✅ Integration Tests**: Multi-service workflows with actual data
- **✅ Frontend Tests**: Svelte components with real user interactions

**📖 [Complete Testing Guide →](docs/TESTING_STRATEGY_Context.md)**

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Svelte SPA    │◄──►│   C# Web API    │◄──►│   PostgreSQL    │
│                 │    │                 │    │   + Redis       │
│ - Components    │    │ - Controllers   │    │ - Dictionaries  │
│ - State Mgmt    │    │ - Services      │    │ - Signs         │
│ - PWA Support   │    │ - Repositories  │    │ - Users         │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🎯 Core Features

- **Dictionary Management**: Create and organize SignWriting dictionaries
- **Visual Sign Editor**: Interactive sign creation with symbol palette
- **Text Composition**: Create sequences of signs (SignText)
- **Multi-format Support**: FSW, KSW, BSW, CSW format conversion
- **Search**: Comprehensive search across terms, signs, and symbols

## 📦 Project Structure

```
src/
├── SignPuddle.API/           # C# Web API backend
├── SignPuddle.API.Tests/     # Unit tests for business logic
├── SignPuddle.API.E2ETests/  # End-to-end API tests
├── signpuddle-web/           # Svelte frontend application
│   ├── __tests__/            # Frontend unit tests
│   └── cypress/              # E2E browser tests
docs/                         # Documentation
php-legacy/                   # Original PHP codebase (reference)
```

## 🔄 Migration Status

Migrating from legacy PHP to modern C#/Svelte stack:

- **✅ Backend API** - Core services and endpoints
- **✅ Testing Framework** - Comprehensive test coverage
- **🚧 Frontend Components** - Svelte UI development
- **⏳ Data Migration** - SPML to database conversion
- **⏳ Production Deployment** - Infrastructure setup

## 🤝 Contributing

1. **Review Testing Strategy**: Read [docs/TESTING_STRATEGY_Context.md](docs/TESTING_STRATEGY_Context.md)
2. **Run Tests First**: Ensure all tests pass before changes
3. **Write Tests**: Follow TDD approach with real implementations
4. **No Mocking**: Use actual dependencies for reliable tests

## 📄 License

[License information to be added]

## 🌐 Community

- **SignWriting Community**: [signwriting.org](https://signwriting.org)
- **Original SignPuddle**: Legacy PHP version documentation
- **Valerie Sutton**: Creator of SignWriting notation system

---

**📖 For complete development guidelines and testing procedures:**
→ **[Testing Strategy Guide](docs/TESTING_STRATEGY_Context.md)**
