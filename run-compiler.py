import subprocess, os, json

exe = r'C:\Users\SohaibAli\.nuget\packages\microsoft.windowsappsdk.winui\1.8.260204000\tools\net472\XamlCompiler.exe'
inp = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj\x64\Debug\net8.0-windows10.0.19041.0\win-x64\input.json'
out = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj\x64\Debug\net8.0-windows10.0.19041.0\win-x64\output2.json'

os.chdir(r'C:\Users\SohaibAli\Documents\PotomacAnalyst')

r = subprocess.run([exe, inp, out], capture_output=True, text=True, timeout=120,
                   cwd=r'C:\Users\SohaibAli\Documents\PotomacAnalyst')
print(f"RC: {r.returncode}")
print(f"STDOUT len: {len(r.stdout)}")
print(f"STDERR len: {len(r.stderr)}")
if r.stdout: print("STDOUT:", r.stdout[:2000])
if r.stderr: print("STDERR:", r.stderr[:2000])

if os.path.exists(out):
    with open(out) as f: data = json.load(f)
    entries = data.get('MSBuildLogEntries', [])
    print(f"\nOutput entries: {len(entries)}")
    for e in entries:
        msg = str(e.get('Message', ''))
        code = e.get('ErrorCode', '')
        if code or 'error' in msg.lower() or 'unknown' in msg.lower() or 'failed' in msg.lower():
            print(f"  [{code}] {e.get('File','')}:{e.get('LineNumber',0)} - {msg[:250]}")
else:
    print("output2.json was NOT created")
