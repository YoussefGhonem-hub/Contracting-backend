$ErrorActionPreference = 'Stop'

function Invoke-CurlJson {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Token = "",
        [string]$JsonBody = ""
    )

    $args = @('-k', '-s', '-X', $Method, $Url, '-H', 'Accept: application/json')
    if ($Token) { $args += @('-H', "Authorization: Bearer $Token") }
    if ($JsonBody) {
        $args += @('-H', 'Content-Type: application/json', '-d', $JsonBody)
    }
    $args += @('-w', 'HTTPSTATUS:%{http_code}')

    $raw = & curl.exe @args
    $txt = [string]$raw
    $status = [int]($txt -replace '(?s).*HTTPSTATUS:', '')
    $body = $txt -replace 'HTTPSTATUS:\d+$', ''

    [pscustomobject]@{ Status = $status; Body = $body; Url = $Url; Method = $Method }
}

function Invoke-CurlForm {
    param(
        [string]$Url,
        [string]$Token,
        [string[]]$FormPairs
    )

    $args = @('-k', '-s', '-X', 'POST', $Url, '-H', "Authorization: Bearer $Token", '-H', 'Accept: application/json')
    foreach ($pair in $FormPairs) {
        $args += @('-F', $pair)
    }
    $args += @('-w', 'HTTPSTATUS:%{http_code}')

    $raw = & curl.exe @args
    $txt = [string]$raw
    $status = [int]($txt -replace '(?s).*HTTPSTATUS:', '')
    $body = $txt -replace 'HTTPSTATUS:\d+$', ''

    [pscustomobject]@{ Status = $status; Body = $body; Url = $Url; Method = 'POST' }
}

function Try-Login {
    param([string]$Base, [string]$UserNameOrEmail, [string]$Password)

    $payload = @{ userNameOrEmail = $UserNameOrEmail; password = $Password; rememberMe = $false } | ConvertTo-Json -Compress
    $res = Invoke-CurlJson -Method 'POST' -Url "$Base/api/auth/login" -JsonBody $payload

    $token = $null
    if ($res.Status -eq 200) {
        try {
            $obj = $res.Body | ConvertFrom-Json
            $token = $obj.accessToken
            if (-not $token) { $token = $obj.AccessToken }
        }
        catch {}
    }

    [pscustomobject]@{ Status = $res.Status; Token = $token; Response = $res }
}

    function New-LocalJwt {
        param([string]$Role)

        Add-Type -Path (Join-Path $PSScriptRoot '..\src\Contracting.API\bin\Debug\net8.0\Microsoft.IdentityModel.Abstractions.dll') -ErrorAction SilentlyContinue
        Add-Type -Path (Join-Path $PSScriptRoot '..\src\Contracting.API\bin\Debug\net8.0\Microsoft.IdentityModel.Tokens.dll') -ErrorAction SilentlyContinue
        Add-Type -Path (Join-Path $PSScriptRoot '..\src\Contracting.API\bin\Debug\net8.0\System.IdentityModel.Tokens.Jwt.dll') -ErrorAction SilentlyContinue

        $secret = 'THIS IS YOUR VERY LONG SECRET KEY CHANGE IT'
        $keyBytes = [System.Text.Encoding]::UTF8.GetBytes($secret)
        $securityKey = New-Object Microsoft.IdentityModel.Tokens.SymmetricSecurityKey -ArgumentList (, $keyBytes)
        $creds = New-Object Microsoft.IdentityModel.Tokens.SigningCredentials -ArgumentList $securityKey,([Microsoft.IdentityModel.Tokens.SecurityAlgorithms]::HmacSha256)

        $uid = [guid]::NewGuid().ToString()
        $identity = New-Object System.Security.Claims.ClaimsIdentity
        $identity.AddClaim([System.Security.Claims.Claim]::new('Id', $uid))
        $identity.AddClaim([System.Security.Claims.Claim]::new('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier', $uid))
        $identity.AddClaim([System.Security.Claims.Claim]::new('email', "autotest-$Role@local"))
        $identity.AddClaim([System.Security.Claims.Claim]::new('http://schemas.microsoft.com/ws/2008/06/identity/claims/role', $Role))

        $desc = New-Object Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        $desc.Subject = $identity
        $desc.Expires = [datetime]::UtcNow.AddHours(2)
        $desc.Issuer = 'ContractingApi'
        $desc.Audience = 'ContractingApiUsers'
        $desc.SigningCredentials = $creds

        $handler = New-Object System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler
        $handler.WriteToken($handler.CreateToken($desc))
    }

function Add-Result {
    param([System.Collections.Generic.List[object]]$List, [int]$Order, [string]$Name, [int]$Status, [string]$Url, [string]$Note = '')
    $List.Add([pscustomobject]@{ Order = $Order; Name = $Name; Status = $Status; Url = $Url; Note = $Note }) | Out-Null
}

$results = [System.Collections.Generic.List[object]]::new()
$order = 1

$bases = @('https://localhost:7241', 'http://localhost:5241')
$base = $null
foreach ($b in $bases) {
    $probe = Invoke-CurlJson -Method 'GET' -Url "$b/swagger/index.html"
    if ($probe.Status -gt 0) { $base = $b; break }
}

if (-not $base) {
    Write-Output 'NO_SERVER'
    exit 1
}

# Login users
$super = Try-Login -Base $base -UserNameOrEmail 'superadmin@shop.com' -Password 'SuperAdmin@123'
Add-Result -List $results -Order $order -Name 'Login.SuperAdmin' -Status $super.Status -Url "$base/api/auth/login"
$order++

$accounts = Try-Login -Base $base -UserNameOrEmail 'accounts@shop.com' -Password 'Accounts@123'
if (-not $accounts.Token) {
    $accounts = Try-Login -Base $base -UserNameOrEmail 'admin@shop.com' -Password 'Admin@123'
    Add-Result -List $results -Order $order -Name 'Login.AccountsOrAdmin' -Status $accounts.Status -Url "$base/api/auth/login" -Note 'Fallback to admin if accounts user not seeded'
}
else {
    Add-Result -List $results -Order $order -Name 'Login.AccountsOrAdmin' -Status $accounts.Status -Url "$base/api/auth/login"
}
$order++

$client = Try-Login -Base $base -UserNameOrEmail 'client@shop.com' -Password 'Client@123'
Add-Result -List $results -Order $order -Name 'Login.Client' -Status $client.Status -Url "$base/api/auth/login"
$order++

if (-not $super.Token) {
    $super = [pscustomobject]@{ Status = -1; Token = (New-LocalJwt -Role 'SuperAdmin'); Response = $null }
    Add-Result -List $results -Order $order -Name 'LoginFallback.SuperAdminJwt' -Status 200 -Url 'LOCAL-JWT' -Note 'Used local signed JWT fallback because login failed'
    $order++
}

if (-not $accounts.Token) {
    $accounts = [pscustomobject]@{ Status = -1; Token = (New-LocalJwt -Role 'Accounts'); Response = $null }
    Add-Result -List $results -Order $order -Name 'LoginFallback.AccountsJwt' -Status 200 -Url 'LOCAL-JWT' -Note 'Used local signed JWT fallback because login failed'
    $order++
}

if (-not $client.Token) {
    $client = [pscustomobject]@{ Status = -1; Token = (New-LocalJwt -Role 'Client'); Response = $null }
    Add-Result -List $results -Order $order -Name 'LoginFallback.ClientJwt' -Status 200 -Url 'LOCAL-JWT' -Note 'Used local signed JWT fallback because login failed'
    $order++
}

# Prepare project context
$getProjects = Invoke-CurlJson -Method 'GET' -Url "$base/api/Project?pageIndex=1&pageSize=25" -Token $super.Token
Add-Result -List $results -Order $order -Name 'Project.GetAll' -Status $getProjects.Status -Url $getProjects.Url
$order++

$projectId = $null
if ($getProjects.Status -eq 200) {
    try {
        $p = $getProjects.Body | ConvertFrom-Json
        if ($p.items -and $p.items.Count -gt 0) { $projectId = $p.items[0].id }
        if (-not $projectId -and $p.Items -and $p.Items.Count -gt 0) { $projectId = $p.Items[0].id }
    }
    catch {}
}

if (-not $projectId) {
    $unique = (Get-Date -Format 'yyyyMMddHHmmss')
    $branchPayload = @{
        nameEn = "Auto Branch $unique"
        nameAr = "Auto Branch $unique"
        address = 'Automated test address'
        location = 'Automated test location'
        departments = @()
    } | ConvertTo-Json -Compress

    $createBranch = Invoke-CurlJson -Method 'POST' -Url "$base/api/branch" -Token $super.Token -JsonBody $branchPayload
    Add-Result -List $results -Order $order -Name 'ProjectSetup.CreateBranch' -Status $createBranch.Status -Url $createBranch.Url
    $order++

    $branchId = $null
    if ($createBranch.Status -in @(200,201)) {
        try { $branchId = ($createBranch.Body | ConvertFrom-Json).id } catch {}
    }

    if ($branchId) {
        $createProject = Invoke-CurlForm -Url "$base/api/project" -Token $super.Token -FormPairs @(
            "nameEn=Auto Project $unique",
            "nameAr=Auto Project $unique",
            'location=Automated test location',
            "code=AUTO-$unique",
            'area=120',
            'projectStatus=Active',
            "branchId=$branchId"
        )
        Add-Result -List $results -Order $order -Name 'ProjectSetup.CreateProject' -Status $createProject.Status -Url $createProject.Url
        $order++
    }

    $getProjects2 = Invoke-CurlJson -Method 'GET' -Url "$base/api/Project?pageIndex=1&pageSize=25" -Token $super.Token
    Add-Result -List $results -Order $order -Name 'Project.GetAll(AfterCreate)' -Status $getProjects2.Status -Url $getProjects2.Url
    $order++

    if ($getProjects2.Status -eq 200) {
        try {
            $p2 = $getProjects2.Body | ConvertFrom-Json
            if ($p2.items -and $p2.items.Count -gt 0) { $projectId = $p2.items[0].id }
            if (-not $projectId -and $p2.Items -and $p2.Items.Count -gt 0) { $projectId = $p2.Items[0].id }
        }
        catch {}
    }
}

if ($projectId) {
    $getProjectById = Invoke-CurlJson -Method 'GET' -Url "$base/api/Project/$projectId" -Token $super.Token
    Add-Result -List $results -Order $order -Name 'Project.GetById' -Status $getProjectById.Status -Url $getProjectById.Url
    $order++
}
else {
    Add-Result -List $results -Order $order -Name 'Project.GetById' -Status -1 -Url 'SKIPPED' -Note 'No project available'
    $order++
}

# Create test files
$tmpDir = Join-Path $PSScriptRoot 'artifacts'
if (-not (Test-Path $tmpDir)) { New-Item -ItemType Directory -Path $tmpDir | Out-Null }
$txtFile = Join-Path $tmpDir 'dummy.txt'
$pdfFile = Join-Path $tmpDir 'dummy.pdf'
Set-Content -Path $txtFile -Value "API ordered test $(Get-Date -Format o)"
Set-Content -Path $pdfFile -Value 'fake-pdf-content'

$createdInvoiceId = $null
$createdVoId = $null

if ($projectId) {
    # 1) Create monthly report
    $r = Invoke-CurlForm -Url "$base/api/backoffice/client-content/monthly-reports" -Token $super.Token -FormPairs @(
        "projectId=$projectId",
        'month=6',
        'year=2026',
        'title=Automated Monthly Report',
        'workProgress=Automated progress note',
        "attachments=@$txtFile"
    )
    Add-Result -List $results -Order $order -Name 'Content.CreateMonthlyReport' -Status $r.Status -Url $r.Url
    $order++

    # 2) Create variation order
    $r = Invoke-CurlForm -Url "$base/api/backoffice/client-content/variation-orders" -Token $super.Token -FormPairs @(
        "projectId=$projectId",
        'title=Automated VO',
        'description=Automated variation for API test',
        'cost=1000',
        'issueDate=2026-06-02T10:00:00+02:00',
        'dueDate=2026-06-15T10:00:00+02:00',
        "attachments=@$txtFile"
    )
    if ($r.Status -eq 200) {
        try { $createdVoId = ($r.Body | ConvertFrom-Json).id } catch {}
    }
    Add-Result -List $results -Order $order -Name 'Content.CreateVariationOrder' -Status $r.Status -Url $r.Url
    $order++

    # 3) Upload drawing
    $r = Invoke-CurlForm -Url "$base/api/backoffice/client-content/drawings" -Token $super.Token -FormPairs @(
        "projectId=$projectId",
        'type=TwoD',
        'title=Automated Drawing',
        "file=@$pdfFile"
    )
    Add-Result -List $results -Order $order -Name 'Content.UploadDrawing' -Status $r.Status -Url $r.Url
    $order++

    # 4) Upload tender document
    $r = Invoke-CurlForm -Url "$base/api/backoffice/client-content/tender-documents" -Token $super.Token -FormPairs @(
        "projectId=$projectId",
        'title=Automated Tender Doc',
        "file=@$pdfFile"
    )
    Add-Result -List $results -Order $order -Name 'Content.UploadTender' -Status $r.Status -Url $r.Url
    $order++

    # 5) Upload schedule
    $r = Invoke-CurlForm -Url "$base/api/backoffice/client-content/schedules" -Token $super.Token -FormPairs @(
        "projectId=$projectId",
        'title=Automated Schedule',
        'version=v1',
        "file=@$pdfFile"
    )
    Add-Result -List $results -Order $order -Name 'Content.UploadSchedule' -Status $r.Status -Url $r.Url
    $order++

    # 6) Create invoice (accounts/admin)
    if ($accounts.Token) {
        $invoicePayload = @{ 
            projectId = $projectId
            title = 'Automated Invoice'
            totalValue = 5000
            paidAmount = 0
            notes = 'Created by automated ordered test'
            issueDate = '2026-06-02T10:00:00+02:00'
            dueDate = '2026-06-30T10:00:00+02:00'
        } | ConvertTo-Json -Compress

        $r = Invoke-CurlJson -Method 'POST' -Url "$base/api/backoffice/client-content/invoices" -Token $accounts.Token -JsonBody $invoicePayload
        if ($r.Status -eq 200) {
            try { $createdInvoiceId = ($r.Body | ConvertFrom-Json).id } catch {}
        }
        Add-Result -List $results -Order $order -Name 'Content.CreateInvoice' -Status $r.Status -Url $r.Url
        $order++

        if ($createdInvoiceId) {
            $payPayload = @{ paidAmount = 2500; notes = 'Partial payment from ordered test' } | ConvertTo-Json -Compress
            $r2 = Invoke-CurlJson -Method 'PUT' -Url "$base/api/backoffice/client-content/invoices/$createdInvoiceId/payment" -Token $accounts.Token -JsonBody $payPayload
            Add-Result -List $results -Order $order -Name 'Content.UpdateInvoicePayment' -Status $r2.Status -Url $r2.Url
            $order++
        }
    }
    else {
        Add-Result -List $results -Order $order -Name 'Content.CreateInvoice' -Status -1 -Url 'SKIPPED' -Note 'No accounts/admin token'
        $order++
    }
}

# Client reads (get-all then get-by-id where available)
if ($client.Token) {
    $cp = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects" -Token $client.Token
    Add-Result -List $results -Order $order -Name 'Client.GetProjects' -Status $cp.Status -Url $cp.Url
    $order++

    $clientProjectId = $null
    if ($cp.Status -eq 200) {
        try {
            $arr = $cp.Body | ConvertFrom-Json
            if ($arr.Count -gt 0) { $clientProjectId = $arr[0].id }
        }
        catch {}
    }

    if ($clientProjectId) {
        $sr = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects/$clientProjectId/site-reports" -Token $client.Token
        Add-Result -List $results -Order $order -Name 'Client.GetSiteReports' -Status $sr.Status -Url $sr.Url
        $order++

        $reportId = $null
        if ($sr.Status -eq 200) {
            try {
                $arr = $sr.Body | ConvertFrom-Json
                if ($arr.Count -gt 0) { $reportId = $arr[0].id }
            }
            catch {}
        }

        if ($reportId) {
            $r = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/site-reports/$reportId" -Token $client.Token
            Add-Result -List $results -Order $order -Name 'Client.GetSiteReportById' -Status $r.Status -Url $r.Url
            $order++
        }

        $r = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects/$clientProjectId/invoices" -Token $client.Token
        Add-Result -List $results -Order $order -Name 'Client.GetInvoices' -Status $r.Status -Url $r.Url
        $order++

        $r = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects/$clientProjectId/invoices/financial-summary" -Token $client.Token
        Add-Result -List $results -Order $order -Name 'Client.GetFinancialSummary' -Status $r.Status -Url $r.Url
        $order++

        $r = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects/$clientProjectId/tender-documents" -Token $client.Token
        Add-Result -List $results -Order $order -Name 'Client.GetTenderDocuments' -Status $r.Status -Url $r.Url
        $order++

        $r = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects/$clientProjectId/variation-orders" -Token $client.Token
        Add-Result -List $results -Order $order -Name 'Client.GetVariationOrders' -Status $r.Status -Url $r.Url
        $order++

        $voId = $null
        if ($r.Status -eq 200) {
            try {
                $vo = ($r.Body | ConvertFrom-Json).variationOrders
                if ($vo.Count -gt 0) { $voId = $vo[0].id }
            }
            catch {}
        }

        if ($voId) {
            $r2 = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/variation-orders/$voId" -Token $client.Token
            Add-Result -List $results -Order $order -Name 'Client.GetVariationOrderById' -Status $r2.Status -Url $r2.Url
            $order++
        }

        $r = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects/$clientProjectId/schedule" -Token $client.Token
        Add-Result -List $results -Order $order -Name 'Client.GetSchedule' -Status $r.Status -Url $r.Url
        $order++

        $r = Invoke-CurlJson -Method 'GET' -Url "$base/api/client/projects/$clientProjectId/drawings?type=TwoD" -Token $client.Token
        Add-Result -List $results -Order $order -Name 'Client.GetDrawings' -Status $r.Status -Url $r.Url
        $order++
    }
    else {
        Add-Result -List $results -Order $order -Name 'Client.ProjectBasedGets' -Status -1 -Url 'SKIPPED' -Note 'Client has no assigned project'
        $order++
    }

    # Change-password negative-path check
    $cpayload = @{ currentPassword = 'wrong-current'; newPassword = 'Client@123'; confirmPassword = 'Client@123' } | ConvertTo-Json -Compress
    $r = Invoke-CurlJson -Method 'POST' -Url "$base/api/auth/change-password" -Token $client.Token -JsonBody $cpayload
    Add-Result -List $results -Order $order -Name 'Auth.ChangePassword(Negative)' -Status $r.Status -Url $r.Url
    $order++
}

Write-Output ("BASE=" + $base)
$results | Sort-Object Order | Format-Table -AutoSize | Out-String | Write-Output

$has500 = ($results | Where-Object { $_.Status -ge 500 }).Count -gt 0
$hasAuthFailures = ($results | Where-Object { $_.Name -like 'Login.*' -and $_.Status -ne 200 }).Count -gt 0
Write-Output ("HAS_500=" + $has500)
Write-Output ("HAS_LOGIN_FAILURES=" + $hasAuthFailures)
