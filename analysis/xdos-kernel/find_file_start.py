import struct
import sys

def scan_d88(d88_path, search_str, is_hex=False):
    with open(d88_path, 'rb') as f:
        data = f.read()
    
    if is_hex:
        search_bytes = bytes.fromhex(search_str.replace(" ", ""))
    else:
        search_bytes = search_str.encode('ascii', errors='ignore')
        
    idx = data.find(search_bytes)
    
    if idx == -1:
        print(f"Not found: {search_str}")
        return
        
    print(f"Found '{search_str}' at file offset {hex(idx)}")
    
    track_table = []
    for i in range(164):
        offset = struct.unpack('<I', data[0x10 + i*4:0x14 + i*4])[0]
        if offset > 0:
            track_table.append(offset)
            
    # Iterate through all tracks and sectors to find which one contains 'idx'
    for t_idx, track_offset in enumerate(track_table):
        current_offset = track_offset
        try:
            num_sectors = struct.unpack('<H', data[current_offset+4:current_offset+6])[0]
        except struct.error:
            continue
            
        for s in range(num_sectors):
            c, h, r, n = struct.unpack('<BBBB', data[current_offset:current_offset+4])
            sector_size = (128 << n)
            sector_data_start = current_offset + 16
            sector_data_end = sector_data_start + sector_size
            
            if sector_data_start <= idx < sector_data_end:
                offset_in_sector = idx - sector_data_start
                print(f"  Located in C={c:02X}, H={h:02X}, R={r:02X} (Track index {t_idx})")
                print(f"  Offset in sector: {hex(offset_in_sector)}")
                
                # Calculate candidate observed placement pair
                pair_c = (c * 2) + h
                pair_r = r
                print(f"  Candidate Observed Placement Pair: ({pair_c:02X}, {pair_r:02X})")
                return
            current_offset += 16 + sector_size

print("=== XDOS_SYS.D88 ===")
scan_d88("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", "04 03 08 05 01 0A 06 01 02 00 00 00 00 00 00 00", is_hex=True)
scan_d88("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", "06 09 02 07 01 0A 08 01 09 00 00 00 00 00 00 00", is_hex=True)

print("\n=== XDOSUTIL.D88 ===")
scan_d88("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88", "06 07 04 07 01 0A 08 01 08 45 01 01 00 00 00 00", is_hex=True)
