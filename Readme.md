# RedditSharp.PowerShell

A powershell wrapper for RedditSharp

### Install Instructions

Download the project, compile it.  

Copy the "Redditsharp" directory to your local modules folder

```powershell
Copy-Item -Recurse -Path RedditSharp -Destination $env:USERPROFILE\Documents\WindowsPowerShell\modules
```

### Using RedditSharp.PowerShell

Examples

```powershell
# get some comments
Import-Module RedditSharp

Start-RedditSession -User "myuser" -Password "password" -ClientId "11111111" -Secret "2222222" -RedirectUri "http://localhost"

$subreddit = Get-Subreddit -Name "example"

$hot = $subreddit.Hot | Select -First 100

# process hot queue
$hot | ForEach-Object {
   $item = $_
   if ($item.Title -contains "something bad") {
      
      # reply as moderator
      New-Comment -Target $item -Body "this is something bad" | Invoke-ModeratorAction -Action Distinguish -DistinguishType Moderator
      
      # remove and flair the post
      Invoke-ModeratorAction -Action Remove -Target $item
      Invoke-ModeratorAction -Action Flair -FlairText "Off Topic" -FlairCss = "offTopicCssClass"

   }
}
```

#### Cmdlets

**Get-Comment**

-Subreddit example
-Id a1s2d3 (or t1_a1s2d3)
-Uri http://www.reddit.com/r/spacepope/foo

---

**New-Comment**

-Target VotableThing (Accepts value from pipeline)

-Body "this is example comment text"

-Distinguish Moderator

---

**Edit-Comment**

-Target Comment (Accepts value from pipeline)

-Body "this is the new body"

---

**Get-Post**

-Subreddit example

-Id 1q2w3e (or t3_1q2w3e)

---

**New-Post**
-Subreddit example (Accepts Value from pipeline)

-Title "this is a post title"

-Content string or Uri 

---

**Edit-Post**

-Target (Accepts value from pipeline)

-Body "new text/markdown of body"



**Get-Subreddit**

-Name example

---

**Start-RedditSession**

-UserName johndoe

-Password 12345

-ClientId aaaaaaaaaa

-Secret bbbbbbbbbbbbbbbbbbbbbbb

-RedirectUri http://localhost

---




Invoke-ModeratorAction -Action -Target ]