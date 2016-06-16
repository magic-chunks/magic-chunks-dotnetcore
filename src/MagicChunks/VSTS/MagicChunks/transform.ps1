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

    [String] [Parameter(Mandatory = $true)] [ValidateNotNullOrEmpty()]
    $transformations
)

[System.Reflection.Assembly]::LoadFrom("$PSScriptRoot\MagicChunks.dll") | Out-Null

Write-Host "Transofrming file $($sourcePath)"

try {
    $transforms = New-Object -TypeName MagicChunks.Core.TransformationCollection `

    
    foreach($t in ($transformations | ConvertFrom-Json).psobject.properties) {
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
