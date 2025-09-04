try {
    $response = Invoke-WebRequest -Uri "http://localhost:5168/health-monitoring" -UseBasicParsing
    Write-Host "Status: $($response.StatusCode)"
    Write-Host "Content: $($response.Content)"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
