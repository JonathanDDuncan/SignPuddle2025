# GitHub Copilot Prompt Guide - SignPuddle2 Migration

This file contains reusable prompts for converting the SignPuddle2 PHP app into a clean C# Web API and static frontend.

---

## 🔁 PHP to C# API Endpoint

**Prompt:**
```csharp
// Convert this PHP route to a C# API controller method:
// File: puddle.php
// Action: get
// Query params: sid (puddle ID), id (entry ID)
```

---

## 📦 Generate Cosmos DB Model for Sign Entry

**Prompt:**
```csharp
// Generate a C# class representing a SignWriting entry stored in Cosmos DB.
// Fields: id (GUID), puddleId (int), words (list of strings), fsw (string), createdAt (DateTime), updatedAt (DateTime)
```

---

## 🔐 Generate Basic API Key Authentication Handler

**Prompt:**
```csharp
// Write a middleware in ASP.NET Core that checks for a valid API key in the request header.
// Valid keys are stored in an in-memory list for now.
```

---

## 📄 XML to Model Deserialization

**Prompt:**
```csharp
// Create a method that parses the following XML structure and maps it to the SignEntry class:
// <entry>
//   <id>123</id>
//   <word>hello</word>
//   <fsw>AS100...</fsw>
// </entry>
```

---

## 📁 Convert File Upload Logic from PHP to C#

**Prompt:**
```csharp
// Create a POST endpoint that accepts an uploaded XML file and parses it into a list of SignEntry objects
```

---

## 🧹 Static Frontend Markdown Prompt

**Prompt:**
```html
<!-- Create a static HTML page that lists all entries from a puddle with search filtering by word -->
```

---

## 📌 Best Practices

- Always add a comment above your code in VS Code using these prompts to nudge Copilot.
- Keep related prompts grouped by domain (e.g., model, controller, parsing, auth).
