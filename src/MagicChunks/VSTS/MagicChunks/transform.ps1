[CmdletBinding(DefaultParameterSetName = 'None')]
param(
    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $sourcePath,

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

if ($transformationType -eq "json" -And [String]::IsNullOrEmpty($transformations))
{
    Write-Error -Message "Inline transformations must be specified if Inline JSON transformation type is enabled!"
    Exit
}

if ($transformationType -eq "file" -And [String]::IsNullOrEmpty($transformationsFile))
{
    Write-Error -Message "Transformation file path must be specified if JSON File transformation type is enabled!"
    Exit
}

Add-Type -Path "$PSScriptRoot\MagicChunks.dll"

Write-Host "Transforming file $($sourcePath)"

try {
    $transforms = New-Object -TypeName MagicChunks.Core.TransformationCollection `

    if ($transformationType -eq "file")
    {
        if (Test-Path $transformationsFile) {
            $transformations = Get-Content $transformationsFile
        }
        else {
            Write-Error -Message "Could not find transformation file at $transformationsFile"
            Exit
        }
    }
    
    foreach($t in ($transformations.Replace("\", "\\") | ConvertFrom-Json).psobject.properties) {
        $transforms.Add($t.name, $t.value)
    }

    if ($targetPathType -eq "source") {
        $target = $sourcePath;
    }
    elseif ($targetPathType -eq "specific") {
        $target = $targetPath;
    }

    [MagicChunks.TransformTask]::Transform(($fileType, $null)[[string]::IsNullOrWhitespace($fileType) -or ($fileType -eq "Auto")], $sourcePath, $target, $transforms)

    Write-Host "File transformed to $($target)"
}
catch {
    Write-Error -Message "File transformation error: $($_.Exception.Message)" -Exception $_.Exception
}
