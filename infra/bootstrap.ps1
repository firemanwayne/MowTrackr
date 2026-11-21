<#
.SYNOPSIS
    One-time setup so the Deploy workflow can create and update MowTrackr's Azure resources.

.DESCRIPTION
    Creates one resource group per environment, an Entra app registration with GitHub OIDC federated credentials
    (no client secret), grants it Contributor plus Role Based Access Control Administrator on those resource groups
    only, then creates the GitHub environments and stores the variables and secrets the workflow needs.
    Needs the Azure CLI (az) and GitHub CLI (gh), both signed in, and rights to create app registrations and
    role assignments. Safe to re-run: existing pieces are reused and generated secrets are never rotated.

.EXAMPLE
    ./infra/bootstrap.ps1 -Repository firemanwayne/MowTrackr
#>
[CmdletBinding()]
param(
    [string] $Repository = 'firemanwayne/MowTrackr',
    [string[]] $Environments = @('dev', 'prod'),
    [string] $Location = 'eastus2',
    [string] $AppName = 'mowtrackr-github-deploy'
)

$ErrorActionPreference = 'Stop'

# Runs a native CLI: stderr (such as az's extension warnings) is ignored unless the command fails, in which case it throws.
function Invoke-Cli([string] $Tool, [object[]] $Arguments) {
    $exe = (Get-Command $Tool -CommandType Application | Select-Object -First 1).Source
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $all = & $exe @Arguments 2>&1
    $code = $LASTEXITCODE
    $ErrorActionPreference = $previous

    if ($code -ne 0) {
        $errors = ($all | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] } | ForEach-Object { $_.ToString() }) -join "`n"
        throw "$Tool $($Arguments -join ' ') failed ($code): $errors"
    }

    $all | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] }
}

function az { Invoke-Cli 'az' $args }

function gh { Invoke-Cli 'gh' $args }

function New-RandomSecret([int] $Length) {
    $chars = (48..57) + (65..90) + (97..122)
    -join (1..$Length | ForEach-Object { [char]($chars | Get-Random) })
}

$account = az account show --output json | ConvertFrom-Json
$subscriptionId = $account.id
$tenantId = $account.tenantId
Write-Host "Subscription: $($account.name) ($subscriptionId)"

Write-Host "Creating app registration $AppName..."
$appId = az ad app list --display-name $AppName --query '[0].appId' --output tsv
if (-not $appId) {
    $appId = az ad app create --display-name $AppName --query appId --output tsv
    az ad sp create --id $appId --output none
}

gh variable set AZURE_CLIENT_ID --repo $Repository --body $appId
gh variable set AZURE_TENANT_ID --repo $Repository --body $tenantId
gh variable set AZURE_SUBSCRIPTION_ID --repo $Repository --body $subscriptionId

$ownerName, $repoName = $Repository -split '/'
$ownerId = gh api "repos/$Repository" --jq .owner.id
$repoId = gh api "repos/$Repository" --jq .id
if (-not $ownerId -or -not $repoId) {
    throw "Could not read the numeric ids of $Repository from GitHub."
}

foreach ($environment in $Environments) {
    $resourceGroup = "mowtrackr-rg-$environment"

    Write-Host "[$environment] Resource group $resourceGroup in $Location..."
    az group create --name $resourceGroup --location $Location `
        --tags Application=MowTrackr Environment=$environment ManagedBy=Bootstrap --output none

    Write-Host "[$environment] Federated credentials..."
    # GitHub presents either the name-based subject or, for repositories with immutable ids enabled, the id-based one
    # (repo:owner@ownerId/name@repoId:...). Register both so sign-in works either way.
    $subjects = @{
        "github-$environment"    = "repo:${Repository}:environment:${environment}"
        "github-$environment-id" = "repo:${ownerName}@${ownerId}/${repoName}@${repoId}:environment:${environment}"
    }
    # Microsoft Graph directly, so this works on older Azure CLI versions without the federated-credential commands.
    # Older CLI versions return the object id as "objectId", newer ones as "id".
    $appObjectId = az ad app show --id $appId --query "objectId || id" --output tsv
    $credentialsUrl = "https://graph.microsoft.com/v1.0/applications/$appObjectId/federatedIdentityCredentials"
    foreach ($credentialName in $subjects.Keys) {
        $subject = $subjects[$credentialName]
        $existing = az rest --method GET --url $credentialsUrl --query "value[?subject=='$subject'].name" --output tsv
        if (-not $existing) {
            $credential = @{
                name      = $credentialName
                issuer    = 'https://token.actions.githubusercontent.com'
                subject   = $subject
                audiences = @('api://AzureADTokenExchange')
            } | ConvertTo-Json -Compress
            $file = New-TemporaryFile
            Set-Content -Path $file -Value $credential -Encoding utf8
            az rest --method POST --url $credentialsUrl --headers 'Content-Type=application/json' --body "@$file" --output none
            Remove-Item $file
        }
    }
    Write-Host "[$environment] Role assignments on the resource group..."
    $scope = "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup"
    az role assignment create --assignee $appId --role Contributor --scope $scope --output none
    # The templates give the API's managed identity access to Key Vault, which needs permission to write role assignments.
    az role assignment create --assignee $appId --role 'Role Based Access Control Administrator' --scope $scope --output none

    Write-Host "[$environment] GitHub environment, variable and secret..."
    gh api --method PUT "repos/$Repository/environments/$environment" --silent
    gh variable set AZURE_RESOURCE_GROUP --repo $Repository --env $environment --body $resourceGroup

    # Only generate the signing key the first time, so re-running never invalidates issued tokens.
    $haveSecrets = gh secret list --repo $Repository --env $environment --json name --jq '.[].name'
    if ($haveSecrets -notcontains 'JWT_SIGNING_KEY') {
        gh secret set JWT_SIGNING_KEY --repo $Repository --env $environment --body (New-RandomSecret 48)
    }
}

# The prod deploy waits for approval on this environment (a deploy run pauses at its approve job). Create it with the
# signed-in GitHub user as the required reviewer, limited to main. Add more reviewers under Settings > Environments.
Write-Host '[prod-approval] GitHub environment with a required reviewer...'
$reviewerId = gh api user --jq .id
$approval = @{
    wait_timer                = 0
    prevent_self_review       = $false
    reviewers                 = @(@{ type = 'User'; id = [int]$reviewerId })
    deployment_branch_policy  = @{ protected_branches = $false; custom_branch_policies = $true }
} | ConvertTo-Json -Depth 5
$approval | gh api --method PUT "repos/$Repository/environments/prod-approval" --input - --silent
'{"name":"main","type":"branch"}' | gh api --method POST "repos/$Repository/environments/prod-approval/deployment-branch-policies" --input - --silent

# Shared resources (the DNS zone). Environments add their custom-domain records here (a nested deployment into this group), so the deploy app gets Contributor on it.
Write-Host '[shared] Resource group and DNS access...'
az group create --name mowtrackr-rg-shared --location $Location `
    --tags Application=MowTrackr Environment=shared ManagedBy=Bootstrap --output none
az role assignment create --assignee $appId --role Contributor `
    --scope "/subscriptions/$subscriptionId/resourceGroups/mowtrackr-rg-shared" --output none

Write-Host ''
Write-Host 'Done. Next:'
Write-Host '  1. Optional: add more required reviewers to the prod-approval environment (Settings > Environments).'
Write-Host '  2. Optional: add GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET (and FACEBOOK_*, LINKEDIN_*) as environment secrets to enable those sign-ins.'
Write-Host '  3. Merge to main (deploys dev, then waits for approval to deploy prod), or run the Deploy workflow from the Actions tab.'
