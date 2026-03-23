import sys

def parse_d88(filename):
    with open(filename, 'rb') as f:
        data = f.read()

    # Read Track Pointers
    pointers = []
    for i in range(164):
        offset = 0x20 + i * 4
        ptr = int.from_bytes(data[offset:offset+4], 'little')
        pointers.append(ptr)
    
    sectors = []
    for p in pointers:
        if p == 0 or p >= len(data):
            continue
        # read sectors for this track until next pointer or end
        # a track has multiple sectors
        offset = p
        # just read max 40 sectors
        for _ in range(40):
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
            sectors.append({'C': C, 'H': H, 'R': R, 'data': sec_data, 'offset': offset})
            offset += 16 + data_len
            if offset >= len(data) or (len(sectors) > 0 and num_sec == 0):
                pass
                
    return sectors

def analyze(filename):
    print(f"--- {filename} ---")
    sectors = parse_d88(filename)
    
    dir_sectors = [s for s in sectors if s['C'] == 1 and s['R'] == 2] # Track 1, Sector 2?
    if not dir_sectors:
        dir_sectors = [s for s in sectors if s['C'] == 1 and s['H'] == 0 and s['R'] == 2]
        if not dir_sectors:
            print("Dir sector not found")
            return
            
    # X-DOS dir is track 1, sector 2 to 10 maybe. Let's just find "X-DOS System" in any sector to be sure.
    dir_data = b''
    for s in sectors:
        if s['C'] == 1 and s['R'] >= 2:
            dir_data += s['data']
    
    # parse 32-byte entries
    for i in range(0, len(dir_data), 32):
        entry = dir_data[i:i+32]
        if entry[0] == 0 or entry[0] == 0xFF:
            continue
        name = entry[2:18].decode('ascii', errors='replace').strip()
        if not name:
            continue
        pair_1d_1e = entry[29:31]
        
        # let's find this file's data in the disk. 
        # Usually file data starts with something we can identify?
        # Maybe we can search for the first sector where the file's data seems to be.
        # But wait, what is the 'observed placement pair' on the image? 
        # If it's a known file like SX-BASIC, we can find its string in the disk.
        
        # For SX-BASIC, maybe there's a string "SX-BASIC" or similar inside it?
        
        print(f"File: {name}, 1D/1E: {pair_1d_1e[0]:02X} {pair_1d_1e[1]:02X}")

analyze('images/disk_org/x1/XDOS_SYS.D88')
analyze('images/disk_org/x1/XDOSUTIL.D88')

