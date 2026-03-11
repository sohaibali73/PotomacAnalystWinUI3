import urllib.request, zipfile, io, os, shutil

fonts_dir = r"C:\Users\SohaibAli\Documents\PotomacAnalyst\Assets\Fonts"
os.makedirs(fonts_dir, exist_ok=True)

print("Downloading Quicksand from Google Fonts...")
try:
    url = "https://fonts.google.com/download?family=Quicksand"
    req = urllib.request.Request(url, headers={
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
    })
    with urllib.request.urlopen(req, timeout=30) as r:
        data = r.read()
    print(f"Downloaded zip: {len(data)} bytes")

    with zipfile.ZipFile(io.BytesIO(data)) as z:
        names = z.namelist()
        print("Contents:", names)
        for name in names:
            if name.lower().endswith(".ttf") and "static" in name.lower():
                basename = os.path.basename(name)
                out = os.path.join(fonts_dir, basename)
                with z.open(name) as src, open(out, "wb") as dst:
                    shutil.copyfileobj(src, dst)
                print(f"  Extracted: {basename} ({os.path.getsize(out)} bytes)")
except Exception as ex:
    print(f"Error: {ex}")
    # Fallback: try variable font from Google Fonts
    print("Trying variable font fallback...")
    try:
        vurl = "https://fonts.google.com/download?family=Quicksand%3Awght%400..700"
        req2 = urllib.request.Request(vurl, headers={
            "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"
        })
        with urllib.request.urlopen(req2, timeout=30) as r2:
            data2 = r2.read()
        print(f"Downloaded variable zip: {len(data2)} bytes")
        with zipfile.ZipFile(io.BytesIO(data2)) as z2:
            for name in z2.namelist():
                print(" ", name)
    except Exception as ex2:
        print(f"Fallback error: {ex2}")

print("Done.")
import os
print("Fonts dir contents:")
for f in os.listdir(fonts_dir):
    print(f"  {f}: {os.path.getsize(os.path.join(fonts_dir, f))} bytes")
