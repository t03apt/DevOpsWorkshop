$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"

Function Invoke-SampleCommands(){  
    '' > First.txt
    git add First.txt
    "First" > First.txt
    git add First.txt
    mkdir Folder1
    "First" > Folder1/First.txt
    git add Folder1/First.txt
    git commit -m "First commit"
    
    "Changed" > First.txt
    "Second" > Second.txt
    git add First.txt Second.txt
    git commit -m "Second Commit"
    
    git tag -a 1.0 -m "Tag 1.0"

    git checkout -b mybranch
    Get-ChildItem "./.git/refs" –Recurse | Where-Object { !$_.PSIsContainer } | ForEach-Object { $_.FullName }
}

Function Get-DecompressedGitObject{
    Param(
        $infile
        )

    $result = '';
    $inputStream = New-Object System.IO.FileStream $inFile, ([IO.FileMode]::Open), ([IO.FileAccess]::Read), ([IO.FileShare]::Read)
    $inputStream.ReadByte() | Out-Null
    $inputStream.ReadByte() | Out-Null

    $deflateStream = New-Object System.IO.Compression.DeflateStream $inputStream, ([IO.Compression.CompressionMode]::Decompress)
    $streamReader = New-Object System.IO.StreamReader($deflateStream)
    
    while ($line = $streamReader.ReadLine()) {  
        $result += $line
    }

    $deflateStream.Close()
    $inputStream.Close()
    $streamReader.Close()

    return $result
}

try {
    Invoke-SampleCommands
    Get-ChildItem "./.git/objects" –Recurse | 
    Where-Object { !$_.PSIsContainer } | 
    ForEach-Object {
        Write-Host "`r`n"
        Write-Host $_.FullName
        $result = Get-DecompressedGitObject $_
        Write-Host $result
    }
}
finally {
    Set-Location $PSScriptRoot
}

