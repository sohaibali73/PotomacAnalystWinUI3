import json, os

path = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj\x64\Debug\net8.0-windows10.0.19041.0\win-x64\input.json'
with open(path) as f:
    data = json.load(f)

print("Keys:", list(data.keys()))
print()

for k in ['Pass', 'SourceFileDirectory']:
    if k in data:
        print(f"{k}: {data[k]}")

for section in ['Pages', 'XamlFiles', 'ResourceDictionaries', 'LocalAssembly']:
    v = data.get(section)
    if v:
        if isinstance(v, list):
            print(f"\n{section}: {len(v)} items")
            for item in v[:10]:
                if isinstance(item, dict):
                    src = item.get('Source') or item.get('Path') or item.get('TargetPath') or str(item)[:100]
                    print(f"  {os.path.basename(src) if src else item}")
                else:
                    print(f"  {str(item)[:100]}")
        else:
            print(f"\n{section}: {str(v)[:200]}")
