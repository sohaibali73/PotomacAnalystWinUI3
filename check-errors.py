import json, sys

path = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj\x64\Debug\net8.0-windows10.0.19041.0\win-x64\output.json'
with open(path) as f:
    data = json.load(f)

entries = data.get('MSBuildLogEntries', [])
print(f"Total log entries: {len(entries)}")
for e in entries:
    msg = str(e.get('Message', ''))
    code = e.get('ErrorCode', '')
    if code or 'error' in msg.lower() or 'unknown' in msg.lower() or 'duplication' in msg.lower():
        f = e.get('File', '?')
        ln = e.get('LineNumber', 0)
        print(f"  [{code}] {f}:{ln} - {msg[:200]}")
