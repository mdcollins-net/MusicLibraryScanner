param (
  [string]$Repo = "mdcollins-net/MusicLibraryScanner",
  [string]$ProjectNumber = "1",        # 👈 Update this with your project number
  [string]$Assignee = "mdcollins-net"  # 👈 Your GitHub username
)

Write-Host "🚀 Creating labels..." -ForegroundColor Cyan

# Labels (ignore errors if they already exist)
gh label create "logging"  --color FF8800 --repo $Repo 2>$null
gh label create "discogs"  --color 0066CC --repo $Repo 2>$null
gh label create "config"   --color 33AA33 --repo $Repo 2>$null
gh label create "parser"   --color AA33AA --repo $Repo 2>$null
gh label create "refactor" --color 999999 --repo $Repo 2>$null
gh label create "planned"  --color 000000 --repo $Repo 2>$null

Write-Host "✅ Labels created." -ForegroundColor Green

Write-Host "🚀 Creating issues..." -ForegroundColor Cyan

function New-Issue {
    param (
        [string]$Title,
        [string]$Body,
        [string[]]$Labels,
        [string]$Milestone
    )

    # Turn labels array into repeated -l flags
    $labelArgs = @()
    foreach ($lbl in $Labels) {
        $labelArgs += @("-l", $lbl)
    }

    # Create the issue
    $issueUrl = gh issue create `
        --title "$Title" `
        --body "$Body" `
        @labelArgs `
        -m "$Milestone" `
        -a "$Assignee" `
        --repo $Repo `
        --json url `
        --jq ".url"

    Write-Host "📌 Created issue: $issueUrl"

    # Add to project board + set status
    gh project item-add $ProjectNumber --url $issueUrl --repo $Repo | Out-Null
    gh project item-edit $ProjectNumber --url $issueUrl --field "Status" --value "Todo" --repo $Repo | Out-Null

    Write-Host "   ↳ Added to Project #$ProjectNumber (Status=Todo, Assignee=$Assignee)"
}

# v0.2
New-Issue -Title "Fix logging: MusicLibraryScanner.log not being created/updated" `
          -Body "Logging currently not writing to the file as expected. Needs investigation in log4net.config and initialization." `
          -Labels @("logging") `
          -Milestone "v0.2"

New-Issue -Title "Add separate log file for summary reports" `
          -Body "Add a dedicated log file (e.g., Summary.log) that only writes the summary reports from ProcessingStats.PrintReport()." `
          -Labels @("logging") `
          -Milestone "v0.2"

New-Issue -Title "Implement Discogs lookup (FindReleaseIdAsync)" `
          -Body "Finish implementation of DiscogsApiClient.FindReleaseIdAsync and integrate into MusicScanner." `
          -Labels @("discogs") `
          -Milestone "v0.2"

New-Issue -Title "Persist DiscogsReleaseId in AlbumRepository" `
          -Body "Ensure DiscogsReleaseId from NFO or API lookup is stored in the database when albums are created/updated." `
          -Labels @("discogs") `
          -Milestone "v0.2"

# v0.3
New-Issue -Title "Add MusicBrainz config blocks" `
          -Body "Add configuration blocks in appsettings.json and appsettings.Development.json for MusicBrainz, similar to Discogs and Last.fm." `
          -Labels @("config","planned") `
          -Milestone "v0.3"

New-Issue -Title "Add AudioDB config blocks" `
          -Body "Add configuration blocks in appsettings.json and appsettings.Development.json for AudioDB, similar to Discogs and Last.fm." `
          -Labels @("config","planned") `
          -Milestone "v0.3"

Write-Host "✅ Issues created, assigned to $Assignee, and added to project with Status=Todo." -ForegroundColor Green
