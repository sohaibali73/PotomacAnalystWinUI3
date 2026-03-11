import json, os

path = r'C:\Users\SohaibAli\Documents\PotomacAnalyst\obj\x64\Debug\net8.0-windows10.0.19041.0\win-x64\input.json'
with open(path) as f:
    data = json.load(f)

# Check reference assemblies
refs = data.get('ReferenceAssemblies', [])
print(f"ReferenceAssemblies: {len(refs)}")
missing = []
for r in refs:
    p = r if isinstance(r, str) else r.get('Path', str(r))
    exists = os.path.exists(p)
    if not exists:
        missing.append(p)
        print(f"  MISSING: {os.path.basename(p)}")
print(f"Missing refs: {len(missing)}")

# LocalAssembly
la = data.get('LocalAssembly', '')
if la:
    p = la if isinstance(la, str) else la.get('Path', str(la))
    print(f"\nLocalAssembly: {os.path.basename(str(p))} - exists: {os.path.exists(str(p))}")

# Check SavedStateFile path issue
ssf = data.get('SavedStateFile', '')
print(f"\nSavedStateFile: '{ssf}'")
print(f"Double backslash in path: {'\\\\\\\\' in ssf or ssf.count(chr(92)+chr(92)) > 0}")

# GenXbfPath
gxp = data.get('GenXbfPath', '')
print(f"\nGenXbfPath: '{gxp}'")
print(f"GenXbfPath exists: {os.path.exists(gxp) if gxp else 'N/A'}")
