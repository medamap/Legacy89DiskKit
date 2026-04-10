import struct
import sys
import os

def read_d88_track_sector(data, c_target, h_target, r_target):
    track_table = []
    for i in range(164):
        offset = struct.unpack('<I', data[0x10 + i*4:0x14 + i*4])[0]
        if offset > 0:
            track_table.append(offset)
            
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
            
            if c == c_target and h == h_target and r == r_target:
                return sector_data_start, data[sector_data_start:sector_data_end]
                
            current_offset += 16 + sector_size
    return None, None

def read_directory(data):
    # Directory is at Track 1, R=2 and following sectors on Track 1 Side 0 maybe?
    # Actually, X1 physical mapping:
    # C=1, H=0, R=2
    entries = []
    
    # Read sectors 2 to 16 on C=1, H=0
    for r in range(2, 17):
        offset, sector_data = read_d88_track_sector(data, 1, 0, r)
        if not sector_data:
            continue
            
        for i in range(0, len(sector_data), 32):
            entry_data = sector_data[i:i+32]
            if entry_data[0] == 0x00 or entry_data[0] == 0xE5:
                continue # deleted or empty
                
            name_bytes = entry_data[2:18]
            name = name_bytes.decode('ascii', errors='ignore').strip()
            if not name:
                continue
                
            entries.append({
                'name': name,
                'offset': offset + i,
                'data': entry_data
            })
    return entries

def dump_file_info(d88_path, target_files):
    with open(d88_path, 'rb') as f:
        data = f.read()
        
    entries = read_directory(data)
    
    # FAM is at Track 2, R=1 -> C=2, H=0, R=1
    fam_offset, fam_data = read_d88_track_sector(data, 2, 0, 1)
    
    print(f"=== {os.path.basename(d88_path)} ===")
    print(f"FAM Sector Offset: {hex(fam_offset) if fam_offset else 'Not Found'}\n")
    
    for target in target_files:
        found = False
        for entry in entries:
            if target.lower() in entry['name'].lower() or target == '*':
                found = True
                print(f"File: {entry['name']}")
                print(f"  Directory Entry Base Offset: {hex(entry['offset'])}")
                
                b1a = entry['data'][0x1A]
                b1b = entry['data'][0x1B]
                b1c = entry['data'][0x1C]
                b1d = entry['data'][0x1D]
                b1e = entry['data'][0x1E]
                
                print(f"  Directory Bytes 0x1A-0x1E: {b1a:02X} {b1b:02X} {b1c:02X} {b1d:02X} {b1e:02X}")
                print(f"  0x1D/0x1E pair: ({b1d:02X}, {b1e:02X})")
                
                # First observed placement based on the pair
                # From notes: C * 2 + H = b1d, R = b1e
                c = b1d // 2
                h = b1d % 2
                r = b1e
                print(f"  First observed placement pair: ({b1d:02X}, {b1e:02X}) -> C={c:02X}, H={h:02X}, R={r:02X}")
                
                # Plausible FAM byte window
                # Let's just dump a few bytes around the offset equal to b1d
                # Or simply dump the whole FAM or a 16 byte window starting at b1d
                if fam_data:
                    fam_idx = b1d
                    window_start = max(0, fam_idx - 4)
                    window_end = min(len(fam_data), fam_idx + 12)
                    fam_window = fam_data[window_start:window_end]
                    hex_str = " ".join([f"{b:02X}" for b in fam_window])
                    print(f"  Raw FAM-area window (offset {hex(fam_offset + window_start)} to {hex(fam_offset + window_end - 1)}): {hex_str}")
                else:
                    print("  FAM data not found")
                print("")
        if not found and target != '*':
            print(f"File '{target}' not found in directory.\n")

if __name__ == '__main__':
    targets = ['SX-BASIC', 'Overlay', 'X-DOS System X1']
    dump_file_info("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", targets)
    dump_file_info("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88", targets)
