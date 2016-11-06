[CmdletBinding(DefaultParameterSetName = 'None')]
param(
    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $sourcePath,

    [bool]
    $sourcePathRecurse,

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $fileType,

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $targetPathType,

    [String] [Parameter(Mandatory = $false)]
    $targetPath,

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $transformationType,

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $transformations
)

# Init Magic Chunks

Add-Type -Path "$PSScriptRoot\MagicChunks.dll"


# Parse transforms

try {
    $transforms = New-Object -TypeName MagicChunks.Core.TransformationCollection `

    foreach($t in ($transformations.Replace("\", "\\") | ConvertFrom-Json).psobject.properties) {
        $transforms.Add($t.name, $t.value)
    }
}
catch {
    Write-Error -Message "Transforms parsing error: $($_.Exception.Message)" -Exception $_.Exception
    throw;
}


# Find files to transform

if ($sourcePathRecurse) {
    $files = Get-ChildItem $sourcePath -Recurse
}
else {
    $files = Get-ChildItem $sourcePath
}

foreach ($file in $files) {

    # Transform file

    Write-Host "Transforming file $($file)"

    try {
        if ($targetPathType -eq "source") {
            $target = $file;
        }
        elseif ($targetPathType -eq "specific") {
            $target = $targetPath;
        }

        [MagicChunks.TransformTask]::Transform(($fileType, $null)[[string]::IsNullOrWhitespace($fileType) -or ($fileType -eq "Auto")], $sourcePath, $target, $transforms)

        Write-Host "File transformed to $($target)"
    }
    catch {
        Write-Error -Message "File $($file) transformation error: $($_.Exception.Message)" -Exception $_.Exception
    }
}