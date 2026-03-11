$dir = "C:\Users\SohaibAli\Documents\PotomacAnalyst\Assets\Fonts"
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$ua = "Mozilla/4.0 (compatible; MSIE 5.0; Windows NT 5.1)"

$fonts = @(
    @{ Family = "Rajdhani"; Weight = "300"; File = "Rajdhani-Light.ttf" },
    @{ Family = "Rajdhani"; Weight = "500"; File = "Rajdhani-Medium.ttf" },
    @{ Family = "Rajdhani"; Weight = "700"; File = "Rajdhani-Bold.ttf" },
    @{ Family = "Quicksand"; Weight = "300"; File = "Quicksand-Light.ttf" },
    @{ Family = "Quicksand"; Weight = "400"; File = "Quicksand-Regular.ttf" },
    @{ Family = "Quicksand"; Weight = "500"; File = "Quicksand-Medium.ttf" },
    @{ Family = "Quicksand"; Weight = "700"; File = "Quicksand-Bold.ttf" }
)

foreach ($f in $fonts) {
    try {
        $fam = [uri]::EscapeDataString($f.Family)
        $apiUrl = "https://fonts.googleapis.com/css2?family=" + $fam + ":wght@" + $f.Weight
        $css = (Invoke-WebRequest -Uri $apiUrl -UserAgent $ua -UseBasicParsing).Content
        $m = [regex]::Match($css, "url\(([^)]+\.ttf)\)")
        if ($m.Success) {
            $fontUrl = $m.Groups[1].Value.Trim("'").Trim('"')
            $outPath = Join-Path $dir $f.File
            Invoke-WebRequest -Uri $fontUrl -OutFile $outPath -UseBasicParsing
            Write-Host "Downloaded: $($f.File)"
        } else {
            Write-Warning "No TTF URL found for $($f.Family) $($f.Weight)"
        }
    } catch {
        Write-Warning "Error downloading $($f.File): $_"
    }
}

Write-Host ""
Write-Host "Done. Files in: $dir"
Get-ChildItem $dir | Format-Table Name, Length -AutoSize
