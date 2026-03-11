import os

base = r'C:\Users\SohaibAli\Documents\PotomacAnalyst'
files_to_fix = [
    r'MainWindow.xaml',
    r'Package.appxmanifest',
    r'Views\Auth\LoginPage.xaml',
    r'Views\Auth\ForgotPasswordPage.xaml',
    r'Views\Pages\AflGeneratorPage.xaml',
]

for rel in files_to_fix:
    path = os.path.join(base, rel)
    with open(path, 'rb') as f:
        data = f.read()

    original_len = len(data)

    # Remove UTF-8 BOM (EF BB BF)
    if data[:3] == b'\xef\xbb\xbf':
        data = data[3:]
        print(f'Removed UTF-8 BOM from: {rel}')

    # Remove any other non-XML garbage at the start
    # Strip bytes until we find '<'
    i = 0
    while i < len(data) and data[i:i+1] != b'<':
        i += 1
    if i > 0:
        print(f'Stripped {i} garbage byte(s) from start of: {rel} (was: {data[:i+5]!r})')
        data = data[i:]

    # Write back without BOM, UTF-8 encoding
    with open(path, 'wb') as f:
        f.write(data)
    print(f'Saved {rel}: length {original_len} -> {len(data)}, starts with {data[:30]!r}')

print('\nAll files processed.')

# Also scan all other XAML files for BOM
print('\nScanning all XAML files for remaining issues...')
for root, dirs, files in os.walk(base):
    dirs[:] = [d for d in dirs if d not in {'obj','bin','.git','Assets'}]
    for fn in files:
        if fn.endswith('.xaml'):
            p = os.path.join(root, fn)
            with open(p, 'rb') as f:
                h = f.read(8)
            if h[:3] == b'\xef\xbb\xbf' or h[:1] != b'<':
                print(f'  ISSUE: {os.path.relpath(p, base)} starts with {h[:8]!r}')
print('Scan complete.')
