Function Format-MagicChunks() {
    <#
    .SYNOPSIS
    Transforms source document with specified transformations
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$True, ValueFromPipeline=$True)]
        [System.IO.FileInfo]$path,

        [Parameter()]
        [string]$target,

        [Parameter(Mandatory=$True)]
        [hashtable]$transformations,

        [Parameter()]
        [string]$type
    )
    PROCESS {
        [System.Reflection.Assembly]::LoadFrom("$PSScriptRoot\MagicChunks.dll") | Out-Null

        Write-Host "Transofrming file $($path)"

        try {
            $transforms = New-Object -TypeName MagicChunks.Core.TransformationCollection `

            foreach($t in $transformations.GetEnumerator()) {
                $transforms.Add($t.Key, $t.Value)
            }

            [MagicChunks.TransformTask]::Transform($type, $path, ($target, $path)[[string]::IsNullOrWhiteSpace($target)], $transforms)

            Write-Host "File transformed to $(($target, $path)[[string]::IsNullOrWhiteSpace($target)])"
        }
        catch {
            Write-Error -Message "File transformation error: $($_.Exception.Message)" -Exception $_.Exception
        }
	}
}