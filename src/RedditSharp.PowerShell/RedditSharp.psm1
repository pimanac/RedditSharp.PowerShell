function Get-ModMatrix {
   [CmdletBinding()]
   Param(
      [Parameter(Mandatory=$true,ValueFromPipeline=$true)]
      [RedditSharp.Things.ModAction[]]$InputObject,
      [Parameter(Mandatory=$false)]
      [ValidateSet('Name','Total')]
      [string]$Sort = 'Name',
      [Parameter(Mandatory=$false)]
      [switch]$Descending
   )
   
   $group = $modlog | Group -Property Action,ModeratorName -AsHashTable -AsString

   $actions = $modlog | Select -Expand Action -Unique | Sort
   $mods = $modlog | Select -ExpandProperty ModeratorName -Unique
   

   $result = @()

   $mods | % {
      $mod = $_
      $template = @{}
      $actions | % { 
         $template[$_] = 0
      }
      $template["Name"] = $_
      $template["Total"] = 0

      $t = 0
      $actions | % { 
         $t += $group["$_, $mod"].Count
         $template[$_] = $group["$_, $mod"].Count

      }

      $template["Total"] = $t
      $result += New-Object PsObject -Property $template
   }

   if ($Descending.IsPresent) {
      $result | Sort -Property $Sort -Descending | Select Name,Total,* -ErrorAction SilentlyContinue
   }
   else {
      $result | Sort -Property $Sort | Select Name,Total,* -ErrorAction SilentlyContinue
   }
}