import sys

def dump_dir(filename):
    with open(filename, 'rb') as f:
        data = f.read()
    
    # Find directory sector (Track 1, R=2)
    # We can just scan for it
    offset = 0x2b0
    while offset < len(data) - 16:
        C = data[offset]
        H = data[offset+1]
        R = data[offset+2]
        sec_len = int.from_bytes(data[offset+14:offset+16], 'little')
        if sec_len == 0 or sec_len > 8192: break
        
        if C == 1 and H == 0 and R == 2:
            sec_data = data[offset+16:offset+16+sec_len]
            # parse 32-byte entries
            print(f"Directory for {filename}:")
            for i in range(0, 256, 32):
                entry = sec_data[i:i+32]
                if entry[0] == 0: break
                if entry[0] == 0xe5: continue
                name = entry[2:18].decode('ascii', errors='ignore').strip()
                b1d = entry[0x1D]
                b1e = entry[0x1E]
                print(f"  {name:16} | 1D: {b1d:02X} | 1E: {b1e:02X}")
        offset += 16 + sec_len

dump_dir('/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88')
dump_dir('/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88')
