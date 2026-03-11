import json, os

path = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj\x64\Debug\net8.0-windows10.0.19041.0\win-x64\input.json'
with open(path) as f:
    data = json.load(f)

print(f"IsPass1: {data.get('IsPass1')}")
print(f"SavedStateFile: {data.get('SavedStateFile')}")

pages = data.get('XamlPages', [])
print(f"\nXamlPages: {len(pages)} items")
for item in pages:
    if isinstance(item, dict):
        src = item.get('SourcePath') or item.get('Source') or str(item)[:120]
        print(f"  {os.path.basename(src)}")
    else:
        print(f"  {item}")

apps = data.get('XamlApplications', [])
print(f"\nXamlApplications: {len(apps)} items")
for item in apps:
    if isinstance(item, dict):
        src = item.get('SourcePath') or item.get('Source') or str(item)[:120]
        print(f"  {os.path.basename(src)}")

# Check SavedStateFile exists
ssf = data.get('SavedStateFile', '')
if ssf:
    print(f"\nSavedStateFile exists: {os.path.exists(ssf)}")
