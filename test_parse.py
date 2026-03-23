import sys

def parse_d88(filename):
    with open(filename, 'rb') as f:
        data = f.read()

    pointers = []
    for i in range(164):
        offset = 0x20 + i * 4
        ptr = int.from_bytes(data[offset:offset+4], 'little')
        if ptr > 0:
            pointers.append(ptr)
    
    sectors = []
    for p in sorted(list(set(pointers))):
        offset = p
        # Read sectors until next pointer
        while True:
            if offset + 16 > len(data):
                break
            C = data[offset]
            H = data[offset+1]
            R = data[offset+2]
            N = data[offset+3]
            num_sec = int.from_bytes(data[offset+4:offset+6], 'little')
            data_len = int.from_bytes(data[offset+14:offset+16], 'little')
            
            if data_len == 0 or data_len > 8192:
                break
                
            sec_data = data[offset+16:offset+16+data_len]
            sectors.append({'C': C, 'H': H, 'R': R, 'offset': offset, 'data': sec_data})
            offset += 16 + data_len
            if offset >= len(data) or data_len == 0:
                break
            # break if we hit another track pointer, but D88 doesn't always have tight packed sectors.
            # a heuristic is 40 sectors max or next pointer.
            # since we sorted pointers, we can stop if offset >= next pointer
            # actually just reading sequentially is fine for D88 if we know track boundaries.
            break # let's just use the known robust d88 parser logic
            
    return sectors

def read_d88(filename):
    with open(filename, 'rb') as f:
        data = f.read()

    pointers = []
    for i in range(164):
        offset = 0x20 + i * 4
        ptr = int.from_bytes(data[offset:offset+4], 'little')
        if ptr > 0:
            pointers.append(ptr)
            
    sectors = []
    for p in pointers:
        offset = p
        for _ in range(40):
            if offset + 16 > len(data):
                break
            C = data[offset]
            H = data[offset+1]
            R = data[offset+2]
            N = data[offset+3]
            data_len = int.from_bytes(data[offset+14:offset+16], 'little')
            if data_len == 0 or data_len > 8192:
                break
            sec_data = data[offset+16:offset+16+data_len]
            sectors.append({'C': C, 'H': H, 'R': R, 'offset': offset, 'data': sec_data})
            offset += 16 + data_len
    return sectors

def analyze(filename):
    print(f"--- {filename} ---")
    sectors = read_d88(filename)
    
    dir_data = b''
    for s in sectors:
        if s['C'] == 1 and s['R'] >= 2 and s['R'] <= 9: # Track 1, Side 0/1, R 2-9? Usually just Side 0?
            # actually we don't know if it's interleaved. Let's just collect all Track 1 R>=2.
            if s['H'] == 0: # typical 2D
                dir_data += s['data']
            
    files = []
    for i in range(0, min(len(dir_data), 2048), 32):
        entry = dir_data[i:i+32]
        if entry[0] == 0 or entry[0] == 0xFF:
            continue
        name = entry[2:18].decode('ascii', errors='replace').strip()
        if not name:
            continue
        v1d = entry[29]
        v1e = entry[30]
        files.append((name, v1d, v1e, entry))
        print(f"File: '{name}', 1D: {v1d:02X}, 1E: {v1e:02X}")
        
    return sectors, files

s_sys, f_sys = analyze('images/disk_org/x1/XDOS_SYS.D88')
s_uti, f_uti = analyze('images/disk_org/x1/XDOSUTIL.D88')
