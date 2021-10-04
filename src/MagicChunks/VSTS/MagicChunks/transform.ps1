[CmdletBinding(DefaultParameterSetName = 'None')]
param(
    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $sourcePath,

    [String] [Parameter(Mandatory = $true)]
    $sourcePathRecurse,

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $fileType,

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $targetPathType,

    [String] [Parameter(Mandatory = $false)]
    $targetPath,

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $transformationType,

    [String] [Parameter(Mandatory = $false)] 
    $transformations,
    
    [String] [Parameter(Mandatory = $false)]
    $transformationsFile
)

# Init Magic Chunks

Add-Type -Path "$PSScriptRoot\MagicChunks.dll"

if ($transformationType -eq "json" -And [String]::IsNullOrEmpty($transformations)) {
    Write-Error -Message "Inline transformations must be specified if Inline JSON transformation type is enabled!"
    Exit
}

if ($transformationType -eq "file" -And [String]::IsNullOrEmpty($transformationsFile)) {
    Write-Error -Message "Transformation file path must be specified if JSON File transformation type is enabled!"
    Exit
}


# Parse transformations

try {
    $transforms = New-Object -TypeName MagicChunks.Core.TransformationCollection

    if ($transformationType -eq "file") {
        if (Test-Path $transformationsFile) {
            $transformations = Get-Content $transformationsFile
        }
        else {
            Write-Error -Message "Could not find transformation file at $transformationsFile"
            Exit
        }
    }

    foreach($t in ($transformations | ConvertFrom-Json).psobject.properties) {
        Write-Host "Transformation found: $($t.name): $($t.value)"
        $transforms.Add($t.name, $t.value)
    }
}
catch {
    Write-Error -Message "Transforms parsing error: $($_.Exception.Message)" -Exception $_.Exception
    throw;
}


# Find files to transform

if ([System.Convert]::ToBoolean($sourcePathRecurse)) {
    $files = Get-ChildItem $sourcePath -Recurse
}
else {
    $files = Get-ChildItem $sourcePath
}


# Transform files

foreach ($file in $files) {
    Write-Host "Transforming file $($file)"

    try {
        if ($targetPathType -eq "source") {
            $target = $file;
        }
        elseif ($targetPathType -eq "specific") {
            $target = $targetPath;
        }

        [MagicChunks.TransformTask]::Transform(($fileType, $null)[[string]::IsNullOrWhitespace($fileType) -or ($fileType -eq "Auto")], $file, $target, $transforms)

        Write-Host "File $($file) transformed into $($target)"
    }
    catch {
        Write-Error -Message "File $($file) transformation error: $($_.Exception.Message)" -Exception $_.Exception
    }
}