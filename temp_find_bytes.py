import sys
def get_sec(f, tgt_c, tgt_h, tgt_r):
    data = open(f, 'rb').read()
    for i in range(164):
        offset = 0x20 + i*4
        p = int.from_bytes(data[offset:offset+4], 'little')
        if p == 0: continue
        while p < len(data) - 16:
            C, H, R, N = data[p:p+4]
            slen = int.from_bytes(data[p+14:p+16], 'little')
            if slen == 0: break
            if C == tgt_c and H == tgt_h and R == tgt_r:
                return data[p+16:p+16+16]
            p += 16 + slen
            # Only scan this track pointer's sectors (up to 40)
            # Actually, standard D88: each pointer points to a track, which has multiple sectors.
            # A track usually has 10 or 16 sectors. We can just break after a track. 
            # It's safer to just do a linear scan from 0x2b0
    
    # Linear scan
    offset = 0x2b0
    while offset < len(data) - 16:
        C, H, R, N = data[offset:offset+4]
        slen = int.from_bytes(data[offset+14:offset+16], 'little')
        if slen == 0 or slen > 8192: break
        if C == tgt_c and H == tgt_h and R == tgt_r:
            return data[offset+16:offset+16+16]
        offset += 16 + slen
    return None

files = [
    ("XDOS_SYS.D88", "X-DOS System", 1, 0, 1),
    ("XDOS_SYS.D88", "X-DOS System X1", 2, 0, 2),
    ("XDOS_SYS.D88", "SX-BASIC", 3, 0, 8),
    ("XDOS_SYS.D88", "AUTO RUN.BAS", 33, 0, 4), # 1D=42 -> 66 -> C=33 H=0
    ("XDOSUTIL.D88", "X-DOS System", 1, 0, 1),
    ("XDOSUTIL.D88", "SX-BASIC", 2, 0, 2),      # 1D=04 -> 4 -> C=2 H=0
    ("XDOSUTIL.D88", "AUTO RUN.BAS", 3, 0, 4),  # 1D=06 -> 6 -> C=3 H=0
    ("XDOSUTIL.D88", "Overlay module", 3, 0, 6) # 1D=06 -> 6 -> C=3 H=0
]

for d, name, c, h, r in files:
    path = "images/disk_org/x1/" + d
    b = get_sec(path, c, h, r)
    if b:
        print(f"'{name}' in {d}: {b.hex().upper()}")
    else:
        print(f"'{name}' in {d}: NOT FOUND")
