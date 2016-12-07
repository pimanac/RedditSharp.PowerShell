Write-Host "Running Build script"

$module = gci "RedditSharp.dll"

$module.FullName

$dir = "./RedditSharp"
$files = gci @("RedditSharp.PowerShell.dll",
               "RedditSharp.dll",
               "Newtonsoft.Json.dll",
               "HtmlAgilityPack.dll",
               "log4net.dll",
               "RedditSharp.psd1",
               "RedditSharp.PowerShell.dll-Help.xml",
               "RedditSharp.PowerShell.format.ps1xml"
          )

Remove-Item -Path $dir -Recurse -Force

if ( !(Test-Path $dir)) {
    New-Item -ItemType Directory -Path $dir | Out-Null
}

Write-Host "Copying..."

$files | % {
   Write-Host $_.Name
   Copy-Item -Path $_.FullName -Destination $dir | Out-Null
}

Write-Host "Done!"
