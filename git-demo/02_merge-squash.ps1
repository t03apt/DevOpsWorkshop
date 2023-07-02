$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/common.ps1"

Function Invoke-SampleCommands(){  
    'C1' > file1.txt
    git add file1.txt
    git commit -m "C1"
    
    'C2' > file2.txt
    git add file2.txt
    git commit -m "C2"
    
    git checkout -b feature/sample

    'C3' > file3.txt
    git add file3.txt
    git commit -m "C3"
    
    'C4' > file4.txt
    git add file4.txt
    git commit -m "C4"

    Start-Sleep -Seconds 1
    git checkout main
    git merge --squash feature/sample
    git commit -m "Squashed commits"
}


try {
    Invoke-SampleCommands
}
finally {
    Set-Location $PSScriptRoot
}

