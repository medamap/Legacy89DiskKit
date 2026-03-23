import struct

def find_string_in_d88(d88_path, search_bytes):
    with open(d88_path, 'rb') as f:
        data = f.read()
    
    # Read header
    header_size = 0x2B0
    track_table = []
    for i in range(164):
        offset = struct.unpack('<I', data[0x10 + i*4:0x14 + i*4])[0]
        if offset > 0:
            track_table.append(offset)
            
    matches = []
    idx = data.find(search_bytes)
    while idx != -1:
        # Find which sector contains this byte
        sector_info = None
        for t_idx, track_offset in enumerate(track_table):
            if track_offset > idx:
                break
            
            # Read sectors in this track
            num_sectors = struct.unpack('<H', data[track_offset+4:track_offset+6])[0]
            current_offset = track_offset
            for s in range(num_sectors):
                c, h, r, n = struct.unpack('<BBBB', data[current_offset:current_offset+4])
                num_sectors_header = struct.unpack('<H', data[current_offset+4:current_offset+6])[0]
                sector_size = (128 << n)
                sector_data_start = current_offset + 16
                sector_data_end = sector_data_start + sector_size
                
                if sector_data_start <= idx < sector_data_end:
                    offset_in_sector = idx - sector_data_start
                    sector_info = f"C={c:02X}, H={h:02X}, R={r:02X} (Track {t_idx}), Offset in sector: {offset_in_sector:04X}"
                    
                    # Print start of sector
                    sector_data = data[sector_data_start:sector_data_start+16]
                    hex_data = " ".join([f"{b:02X}" for b in sector_data])
                    print(f"Match found at file offset {hex_data} ... -> {sector_info}")
                    
                    # Also print the previous sector if we are near the start
                    if offset_in_sector < 0x100:
                        print("Match is near the start of the sector.")
                    break
                current_offset += 16 + sector_size
            if sector_info:
                break
                
        matches.append(sector_info)
        idx = data.find(search_bytes, idx + 1)
        
    return matches

print("SX-BASIC:")
find_string_in_d88("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", b"[ SX-BASIC ]")

print("\nOverlay module:")
find_string_in_d88("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88", b"Overlay modulue")

