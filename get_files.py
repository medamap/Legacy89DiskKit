import sys

def get_files(filename):
    with open(filename, 'rb') as f:
        data = f.read()

    # Find the directory by searching for "X-DOS System"
    idx = data.find(b"X-DOS System")
    if idx == -1:
        return
        
    dir_start = idx - 2
    files = []
    for i in range(0, 1024, 32):
        entry = data[dir_start+i:dir_start+i+32]
        if entry[0] == 0 or entry[0] == 0xFF:
            continue
        name = entry[2:18].decode('ascii', errors='replace').strip()
        if not name:
            continue
        v1d = entry[29]
        v1e = entry[30]
        files.append((name, v1d, v1e, entry))
    return files

f1 = get_files('images/disk_org/x1/XDOS_SYS.D88')
print("XDOS_SYS:")
for name, v1d, v1e, entry in f1:
    print(f"  {name:16} | 1D: {v1d:02X}, 1E: {v1e:02X}")
    
f2 = get_files('images/disk_org/x1/XDOSUTIL.D88')
print("XDOSUTIL:")
for name, v1d, v1e, entry in f2:
    print(f"  {name:16} | 1D: {v1d:02X}, 1E: {v1e:02X}")

def scan_strings(filename, match_strs):
    with open(filename, 'rb') as f:
        data = f.read()
    
    # Simple parse D88 sectors sequentially ignoring track headers
    sectors = []
    for offset in range(0x2b0, len(data), 272): # assuming 256 byte sectors + 16 byte header
        if offset + 16 > len(data): break
        C = data[offset]
        H = data[offset+1]
        R = data[offset+2]
        sec_len = int.from_bytes(data[offset+14:offset+16], 'little')
        if sec_len == 256 and offset + 16 + 256 <= len(data):
            sec_data = data[offset+16:offset+16+256]
            sectors.append({'C': C, 'H': H, 'R': R, 'data': sec_data})
            
    for s in match_strs:
        for sec in sectors:
            if s.encode('ascii') in sec['data']:
                print(f"[{filename}] '{s}' found at C={sec['C']:02X} R={sec['R']:02X} (Hex: {sec['data'][:8].hex()})")
                break

scan_strings('images/disk_org/x1/XDOS_SYS.D88', ['AUTO RUN.BAS', 'SX-BASIC', 'Overlay'])
scan_strings('images/disk_org/x1/XDOSUTIL.D88', ['AUTO RUN.BAS', 'SX-BASIC', 'Overlay'])

