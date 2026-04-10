import struct

def dump_sector_start(d88_path, target_c, target_h, target_r):
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
                first_bytes = data[sector_data_start:sector_data_start+16]
                hex_str = "".join([f"\\x{b:02X}" for b in first_bytes])
                print(f"File at C={c:02X}, H={h:02X}, R={r:02X} starts with: b\"{hex_str}\"")
                return
            current_offset += 16 + sector_size

print("=== XDOS_SYS.D88 ===")
print("X-DOS System X1:")
dump_sector_start("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", 0x02, 0x00, 0x02)

print("SX-BASIC:")
dump_sector_start("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", 0x03, 0x00, 0x08)

print("\n=== XDOSUTIL.D88 ===")
print("Overlay module:")
dump_sector_start("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88", 0x03, 0x00, 0x06)
