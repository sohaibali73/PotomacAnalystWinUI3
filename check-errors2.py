import json, os, glob

# Find the most recent output.json
base = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj'
files = glob.glob(base + r'\**\output.json', recursive=True)
for f in files:
    print(f"=== {f} (size {os.path.getsize(f)}) ===")
    try:
        with open(f) as fp:
            data = json.load(fp)
        entries = data.get('MSBuildLogEntries', [])
        print(f"Entries: {len(entries)}")
        for e in entries:
            msg = str(e.get('Message', ''))
            code = e.get('ErrorCode', '')
            importance = e.get('Importance', '')
            if code or 'error' in msg.lower() or 'unknown' in msg.lower() or 'duplication' in msg.lower() or 'quote' in msg.lower():
                fn = e.get('File', '?')
                ln = e.get('LineNumber', 0)
                print(f"  [{code or importance}] {os.path.basename(fn)}:{ln} - {msg[:300]}")
    except Exception as ex:
        print(f"  ERROR reading: {ex}")
