import sys

def parse_dir_and_check(filename):
    with open(filename, 'rb') as f:
        data = f.read()

    idx = data.find(b"X-DOS System")
    if idx == -1: return
        
    dir_start = idx - 2
    files = []
    for i in range(0, 1024, 32):
        entry = data[dir_start+i:dir_start+i+32]
        if len(entry) < 32: break
        if entry[0] == 0 or entry[0] == 0xFF: continue
        name = entry[2:18].decode('ascii', errors='replace').strip()
        if not name: continue
        v1d = entry[29]
        v1e = entry[30]
        files.append((name, v1d, v1e))
        
    for name, c, r in files:
        offset = 0x2b0
        found_sec = None
        while offset < len(data) - 16:
            C = data[offset]
            H = data[offset+1]
            R = data[offset+2]
            sec_len = int.from_bytes(data[offset+14:offset+16], 'little')
            if sec_len == 0 or sec_len > 8192: break
            if C == c and R == r:
                found_sec = data[offset+16:offset+16+sec_len]
                break
            offset += 16 + sec_len
            
        if found_sec is not None:
            hex_str = found_sec[:8].hex()
            print(f"[{filename}] {name:16} | 1D/1E: {c:02X} {r:02X} | Placement Data: {hex_str}")
        else:
            print(f"[{filename}] {name:16} | 1D/1E: {c:02X} {r:02X} | Placement Data: NOT FOUND")

parse_dir_and_check('images/disk_org/x1/XDOS_SYS.D88')
parse_dir_and_check('images/disk_org/x1/XDOSUTIL.D88')
