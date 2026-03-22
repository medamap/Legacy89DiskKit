import struct
import sys

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

def dump_dir(d88_path, label):
    print(f"=== Directory Dump: {label} ===")
    # Directory is Track 1, Sector 2. 
    # Based on (C*2+H), Track 1 (linear) could be C=0, H=1.
    # Let's try C=0, H=1, R=2 first.
    sector_data = get_sector_data(d88_path, 0, 1, 2)
    if not sector_data:
        print("Sector (0, 1, 2) not found, trying (1, 0, 2)...")
        sector_data = get_sector_data(d88_path, 1, 0, 2)
    
    if not sector_data:
        print("Directory sector not found.")
        return

    for i in range(0, len(sector_data), 32):
        entry = sector_data[i:i+32]
        if entry[0] == 0: # Empty entry
            continue
        filename = entry[2:18].decode('ascii', errors='replace').strip()
        # Indices: 0x1A=26, 0x1B=27, 0x1C=28, 0x1D=29, 0x1E=30
        bytes_1a_1e = entry[26:31].hex(' ').upper()
        pair_1d_1e = entry[29:31].hex(' ').upper()
        print(f"Offset {hex(i):>4}: {filename:<16} | 1A-1E: {bytes_1a_1e} | 1D/1E: {pair_1d_1e}")

dump_dir("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", "XDOS_SYS.D88")
dump_dir("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88", "XDOSUTIL.D88")
