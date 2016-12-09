Write-Host "Running postbuild script"

$dir = "./RedditSharp"
$helpDir = "./RedditSharp/en-US"
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

New-Item -ItemType Directory -Path $dir | Out-Null
New-Item -ItemType Directory -Path $helpDir | Out-Null

$files | % {
   Write-Host $_.Name
   if ($_.Name.EndsWith(".dll-Help.xml")) {
      Copy-Item -Path $_.FullName -Destination $helpDir | Out-Null
   }
   else {
      Copy-Item -Path $_.FullName -Destination $dir | Out-Null
   }
}

if ($PSVersionTable.PSVersion.Major -ge 5) {
   Remove-Item "./RedditSharp.PowerShell.zip" -Force
   Compress-Archive -Path "./RedditSharp" -DestinationPath "./RedditSharp.PowerShell.zip"
}

Write-Host "Done!"
