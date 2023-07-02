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
    git checkout main

    'C3' > file3.txt
    git add file3.txt
    git commit -m "C3"
    
    'C4' > file4.txt
    git add file4.txt
    git commit -m "C4"

    'File1 changed on main' > file1.txt
    git add file1.txt
    git commit -m "File1 changed on main"

    git checkout feature/sample
    'File1 changed on feature/sample' > file1.txt
    git add file1.txt
    git commit -m "File1 changed on feature/sample"

    Start-Sleep -Seconds 1
    git merge-base --fork-point main feature/sample
    git checkout feature/sample
    git rebase main
}


try {
    Invoke-SampleCommands
}
finally {
    Set-Location $PSScriptRoot
}

