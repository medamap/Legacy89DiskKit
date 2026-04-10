import struct

def get_sector_data(d88_path, target_c, target_h, target_r):
    with open(d88_path, 'rb') as f:
        data = f.read()
    
    track_table = []
    for i in range(164):
        offset = struct.unpack('<I', data[0x10 + i*4:0x14 + i*4])[0]
        if offset > 0:
            track_table.append(offset)
            
    for track_offset in track_table:
        current_offset = track_offset
        try:
            num_sectors = struct.unpack('<H', data[current_offset+4:current_offset+6])[0]
        except struct.error:
            continue
            
        for s in range(num_sectors):
            c, h, r, n = struct.unpack('<BBBB', data[current_offset:current_offset+4])
            sector_size = (128 << n)
            sector_data_start = current_offset + 16
            
            if c == target_c and h == target_h and r == target_r:
                return data[sector_data_start:sector_data_start + sector_size]
            
            current_offset += 16 + sector_size
    return None

def dump_fam(d88_path, disk_name):
    print(f"=== FAM Dump: {disk_name} ===")
    # Track 2, R=1 maps to C=1, H=0, R=1
    fam_data = get_sector_data(d88_path, 1, 0, 1)
    if fam_data:
        for i in range(0, len(fam_data), 16):
            print(f"{hex(i):>4}: {fam_data[i:i+16].hex(' ').upper()}")
    print("")

dump_fam("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", "XDOS_SYS.D88")
dump_fam("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88", "XDOSUTIL.D88")
