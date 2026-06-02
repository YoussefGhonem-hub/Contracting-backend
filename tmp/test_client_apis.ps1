$ErrorActionPreference = 'Stop'

$token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkM2YyMDM4ZS1lYjQ1LTRjNDgtY2FhMi0wOGRlYzA3OTE5NjUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImQzZjIwMzhlLWViNDUtNGM0OC1jYWEyLTA4ZGVjMDc5MTk2NSIsIklkIjoiZDNmMjAzOGUtZWI0NS00YzQ4LWNhYTItMDhkZWMwNzkxOTY1IiwiZW1haWwiOiJjbGllbnRAc2hvcC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGllbnRAc2hvcC5jb20iLCJuYW1lIjoiRGVmYXVsdCBDbGllbnQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRGVmYXVsdCBDbGllbnQiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJjbGllbnRAc2hvcC5jb20iLCJqdGkiOiIzNTNiMGI2YTRkZGY0NzdhYjk3Y2Q5Zjc1NWRiYjI3NyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsInJvbGVzIjoiW1wiQ2xpZW50XCJdIiwiZGVwYXJ0bWVudEhhdmVUZWFtTGVhZE9yTm90IjoiRmFsc2UiLCJuYmYiOjE3ODAzODU4ODcsImV4cCI6MTc4MDM5MzA4NywiaXNzIjoiQ29udHJhY3RpbmdBcGkiLCJhdWQiOiJDb250cmFjdGluZ0FwaVVzZXJzIn0.Z7VlLHDx1QZb97LU0yWzpxTaZIX1tkBsZffmoOGSsAA'

function Invoke-Api {
  param([string]$Method,[string]$Url,[string]$Body=$null)

  $headers = @('-H',"Authorization: Bearer $token",'-H','Accept: application/json')
  if ($Body) { $headers += @('-H','Content-Type: application/json') }

  $args = @('-k','-s','-X',$Method,$Url) + $headers
  if ($Body) { $args += @('-d',$Body) }
  $args += @('-w','HTTPSTATUS:%{http_code}')

  $raw = & curl.exe @args
  $txt = [string]$raw
  $status = [int]($txt -replace '(?s).*HTTPSTATUS:','')
  $resp = $txt -replace 'HTTPSTATUS:\d+$',''

  return [pscustomobject]@{ Method=$Method; Url=$Url; Status=$status; Body=$resp }
}

$bases = @('https://localhost:7241','http://localhost:5241')
$base = $null
foreach($b in $bases){
  $probe = Invoke-Api -Method 'GET' -Url "$b/api/client/projects"
  if($probe.Status -ne 0){ $base=$b; break }
}

if(-not $base){
  Write-Output 'NO_SERVER'
  exit 1
}

$results = @()

$projects = Invoke-Api 'GET' "$base/api/client/projects"
$results += [pscustomobject]@{Name='1.GetProjects';Status=$projects.Status;Url=$projects.Url}

$projectId = $null
if($projects.Status -eq 200){
  try {
    $arr = $projects.Body | ConvertFrom-Json
    if($arr.Count -gt 0){ $projectId = $arr[0].id }
  } catch {}
}

$reportId=$null
$voId=$null
$pendingVoId=$null

if($projectId){
  $siteReports = Invoke-Api 'GET' "$base/api/client/projects/$projectId/site-reports"
  $results += [pscustomobject]@{Name='2.GetSiteReports';Status=$siteReports.Status;Url=$siteReports.Url}

  if($siteReports.Status -eq 200){
    try {
      $sr = $siteReports.Body|ConvertFrom-Json
      if($sr.Count -gt 0){$reportId=$sr[0].id}
    } catch {}
  }

  if($reportId){
    $r3=Invoke-Api 'GET' "$base/api/client/site-reports/$reportId"
  } else {
    $r3=[pscustomobject]@{Status=-1;Url='SKIPPED(no reportId)'}
  }
  $results += [pscustomobject]@{Name='3.GetSiteReportDetail';Status=$r3.Status;Url=$r3.Url}

  $r4=Invoke-Api 'GET' "$base/api/client/projects/$projectId/invoices"
  $results += [pscustomobject]@{Name='4.GetInvoices';Status=$r4.Status;Url=$r4.Url}

  $r5=Invoke-Api 'GET' "$base/api/client/projects/$projectId/invoices/financial-summary"
  $results += [pscustomobject]@{Name='5.GetFinancialSummary';Status=$r5.Status;Url=$r5.Url}

  $r6=Invoke-Api 'GET' "$base/api/client/projects/$projectId/tender-documents"
  $results += [pscustomobject]@{Name='6.GetTenderDocuments';Status=$r6.Status;Url=$r6.Url}

  $r7=Invoke-Api 'GET' "$base/api/client/projects/$projectId/variation-orders"
  $results += [pscustomobject]@{Name='7.GetVariationOrders';Status=$r7.Status;Url=$r7.Url}

  if($r7.Status -eq 200){
    try {
      $vo = ($r7.Body|ConvertFrom-Json).variationOrders
      if($vo.Count -gt 0){ $voId=$vo[0].id }
      $pending = $vo | Where-Object { $_.status -eq 'Pending' } | Select-Object -First 1
      if($pending){ $pendingVoId = $pending.id }
    } catch {}
  }

  if($voId){
    $r8=Invoke-Api 'GET' "$base/api/client/variation-orders/$voId"
  } else {
    $r8=[pscustomobject]@{Status=-1;Url='SKIPPED(no voId)'}
  }
  $results += [pscustomobject]@{Name='8.GetVariationOrderDetail';Status=$r8.Status;Url=$r8.Url}

  if($pendingVoId){
    $r9=Invoke-Api 'POST' "$base/api/client/variation-orders/$pendingVoId/approve"
  } elseif($voId) {
    $r9=Invoke-Api 'POST' "$base/api/client/variation-orders/$voId/approve"
  } else {
    $r9=[pscustomobject]@{Status=-1;Url='SKIPPED(no voId)'}
  }
  $results += [pscustomobject]@{Name='9.ApproveVariationOrder';Status=$r9.Status;Url=$r9.Url}

  $r7b=Invoke-Api 'GET' "$base/api/client/projects/$projectId/variation-orders"
  $pending2=$null
  if($r7b.Status -eq 200){
    try {
      $pending2 = (($r7b.Body|ConvertFrom-Json).variationOrders | Where-Object { $_.status -eq 'Pending' } | Select-Object -First 1).id
    } catch {}
  }

  if($pending2){
    $r10=Invoke-Api 'POST' "$base/api/client/variation-orders/$pending2/reject" '{"rejectionReason":"Automated smoke test"}'
  } elseif($voId){
    $r10=Invoke-Api 'POST' "$base/api/client/variation-orders/$voId/reject" '{"rejectionReason":"Automated smoke test"}'
  } else {
    $r10=[pscustomobject]@{Status=-1;Url='SKIPPED(no voId)'}
  }
  $results += [pscustomobject]@{Name='10.RejectVariationOrder';Status=$r10.Status;Url=$r10.Url}

  $r11=Invoke-Api 'GET' "$base/api/client/projects/$projectId/schedule"
  $results += [pscustomobject]@{Name='11.GetSchedule';Status=$r11.Status;Url=$r11.Url}

  $r12=Invoke-Api 'GET' "$base/api/client/projects/$projectId/drawings?type=TwoD"
  $results += [pscustomobject]@{Name='12.GetDrawings';Status=$r12.Status;Url=$r12.Url}
}
else {
  $results += [pscustomobject]@{Name='2-12.ProjectBased';Status=-1;Url='SKIPPED(no projectId)'}

  # Fallback: verify project-based endpoints still return controlled errors (not 500)
  $dummyProjectId = '00000000-0000-0000-0000-000000000001'
  $dummyReportId = '00000000-0000-0000-0000-000000000002'
  $dummyVoId = '00000000-0000-0000-0000-000000000003'

  $results += [pscustomobject]@{Name='2.GetSiteReports(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/projects/$dummyProjectId/site-reports").Status;Url="$base/api/client/projects/$dummyProjectId/site-reports"}
  $results += [pscustomobject]@{Name='3.GetSiteReportDetail(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/site-reports/$dummyReportId").Status;Url="$base/api/client/site-reports/$dummyReportId"}
  $results += [pscustomobject]@{Name='4.GetInvoices(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/projects/$dummyProjectId/invoices").Status;Url="$base/api/client/projects/$dummyProjectId/invoices"}
  $results += [pscustomobject]@{Name='5.GetFinancialSummary(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/projects/$dummyProjectId/invoices/financial-summary").Status;Url="$base/api/client/projects/$dummyProjectId/invoices/financial-summary"}
  $results += [pscustomobject]@{Name='6.GetTenderDocuments(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/projects/$dummyProjectId/tender-documents").Status;Url="$base/api/client/projects/$dummyProjectId/tender-documents"}
  $results += [pscustomobject]@{Name='7.GetVariationOrders(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/projects/$dummyProjectId/variation-orders").Status;Url="$base/api/client/projects/$dummyProjectId/variation-orders"}
  $results += [pscustomobject]@{Name='8.GetVariationOrderDetail(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/variation-orders/$dummyVoId").Status;Url="$base/api/client/variation-orders/$dummyVoId"}
  $results += [pscustomobject]@{Name='9.ApproveVariationOrder(dummy)';Status=(Invoke-Api 'POST' "$base/api/client/variation-orders/$dummyVoId/approve").Status;Url="$base/api/client/variation-orders/$dummyVoId/approve"}
  $results += [pscustomobject]@{Name='10.RejectVariationOrder(dummy)';Status=(Invoke-Api 'POST' "$base/api/client/variation-orders/$dummyVoId/reject" '{"rejectionReason":"Automated smoke test"}').Status;Url="$base/api/client/variation-orders/$dummyVoId/reject"}
  $results += [pscustomobject]@{Name='11.GetSchedule(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/projects/$dummyProjectId/schedule").Status;Url="$base/api/client/projects/$dummyProjectId/schedule"}
  $results += [pscustomobject]@{Name='12.GetDrawings(dummy)';Status=(Invoke-Api 'GET' "$base/api/client/projects/$dummyProjectId/drawings?type=TwoD").Status;Url="$base/api/client/projects/$dummyProjectId/drawings?type=TwoD"}
}

$r13=Invoke-Api 'POST' "$base/api/auth/change-password" '{"currentPassword":"wrong-current","newPassword":"Client@123","confirmPassword":"Client@123"}'
$results += [pscustomobject]@{Name='13.ChangePassword';Status=$r13.Status;Url=$r13.Url}

Write-Output ("BASE=" + $base)
$results | Format-Table -AutoSize | Out-String | Write-Output
$has500 = ($results | Where-Object { $_.Status -ge 500 }).Count -gt 0
Write-Output ("HAS_500=" + $has500)
