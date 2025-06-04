# Script to update method names in test files to match the repository interface
$filesPath = "c:\Code\SignWriting\SignPuddle\src\SignPuddle.API.Tests"

# Define method name mappings
$methodMappings = @{
    "GetByIdAsync" = "GetSpmlDocumentByIdAsync"
    "GetAllAsync" = "GetAllSpmlDocumentsAsync"
    "GetByTypeAsync" = "GetSpmlDocumentsByTypeAsync"
    "GetByPuddleIdAsync" = "GetSpmlDocumentsByPuddleIdAsync"
    "GetByOwnerAsync" = "GetSpmlDocumentsByOwnerAsync"
    "UpdateAsync" = "UpdateSpmlDocumentAsync"
    "DeleteAsync" = "DeleteSpmlDocumentAsync"
    "ExportAsXmlAsync" = "ExportSpmlDocumentAsXmlAsync"
    "GetStatsAsync" = "GetSpmlDocumentStatsAsync"
}

# Get all .cs files in the test project
$files = Get-ChildItem -Path $filesPath -Filter *.cs -Recurse

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    $modified = $false
    
    # Replace each method name
    foreach ($mapping in $methodMappings.GetEnumerator()) {
        $oldName = $mapping.Key
        $newName = $mapping.Value
        
        if ($content -match $oldName) {
            $content = $content -replace "await\s+_repository\.$oldName\(", "await _repository.$newName("
            $content = $content -replace "public\s+async\s+Task\s+\w+$oldName", "public async Task ``${1}$newName"
            $modified = $true
        }
    }
    
    # Add System.Threading.Tasks if missing
    if ($content -notmatch "using System.Threading.Tasks;") {
        $content = $content -replace "using System;", "using System;`r`nusing System.Threading.Tasks;"
        $modified = $true
    }
    
    # Save the file if modified
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Updated $($file.Name)"
    }
}

Write-Host "Method name update completed."
