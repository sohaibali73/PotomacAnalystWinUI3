import json, os

path = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj\x64\Debug\net8.0-windows10.0.19041.0\win-x64\output.json'
with open(path) as f:
    data = json.load(f)

entries = data.get('MSBuildLogEntries', [])
print(f"Total: {len(entries)}")

# Print last 30 entries
print("\n=== Last 30 entries ===")
for e in entries[-30:]:
    msg = str(e.get('Message', ''))[:200]
    fn  = os.path.basename(str(e.get('File', '?')))
    ln  = e.get('LineNumber', 0)
    code = e.get('ErrorCode', '') or e.get('Importance', '')
    print(f"  [{code}] {fn}:{ln} - {msg}")

# Also check if there are any XBF generation failures
print("\n=== All keys in output.json ===")
for k, v in data.items():
    if k not in ('MSBuildLogEntries', 'GeneratedCodeFiles', 'GeneratedXamlFiles',
                 'GeneratedXbfFiles', 'GeneratedXbfV2Files'):
        print(f"  {k}: {str(v)[:200]}")
