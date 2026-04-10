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

def get_file_bytes(d88_path, pair_c_h, pair_r):
    # pair_c_h is C*2+H
    c = pair_c_h // 2
    h = pair_c_h % 2
    r = pair_r
    return get_sector_data(d88_path, c, h, r)

def collect_evidence(d88_path, disk_name, filenames):
    print(f"--- Evidence for {disk_name} ---")
    
    # FAM is at Track 2, R=1. (C*2+H = 2, so C=1, H=0)
    fam_data = get_sector_data(d88_path, 1, 0, 1)
    if fam_data:
        print(f"FAM Area (Track 2, R=1) Length: {len(fam_data)}")
    
    # Directory is Track 1, R=2. (C*2+H = 1, so C=0, H=1)
    dir_data = get_sector_data(d88_path, 0, 1, 2)
    if not dir_data:
        dir_data = get_sector_data(d88_path, 1, 0, 2)
        
    for target_filename in filenames:
        found = False
        for i in range(0, len(dir_data), 32):
            entry = dir_data[i:i+32]
            filename = entry[2:18].decode('ascii', errors='replace').strip()
            if filename == target_filename:
                found = True
                entry_offset = i
                # 0x1A through 0x1E (26 to 31)
                dir_bytes = entry[26:31].hex(' ').upper()
                p1d = entry[29]
                p1e = entry[30]
                pair_1d_1e = f"({p1d:02X}, {p1e:02X})"
                
                # Payload at (p1d, p1e)
                payload_first_sector = get_file_bytes(d88_path, p1d, p1e)
                payload_hex = payload_first_sector[:16].hex(' ').upper() if payload_first_sector else "NOT FOUND"
                
                # Plausible FAM window
                # If 1D/1E is (C*2+H, R), maybe the FAM index is related.
                # Let's show a window around p1d.
                # If p1d is a logical track index, FAM[p1d] might be relevant.
                fam_window = ""
                if fam_data and p1d < len(fam_data):
                    start_win = max(0, p1d - 4)
                    end_win = min(len(fam_data), p1d + 12)
                    fam_window = fam_data[start_win:end_win].hex(' ').upper()
                    fam_offsets = f"0x{start_win:02X}-0x{end_win-1:02X}"
                
                print(f"File: {filename}")
                print(f"  Dir Entry Offset: {hex(entry_offset)}")
                print(f"  Dir Bytes 0x1A-0x1E: {dir_bytes}")
                print(f"  0x1D/0x1E Pair: {pair_1d_1e}")
                print(f"  First Sector Bytes (first 16): {payload_hex}")
                print(f"  FAM Window ({fam_offsets}): {fam_window}")
                print("")
                break
        if not found:
            print(f"File {target_filename} not found on {disk_name}\n")

sys_files = ["X-DOS System", "SX-BASIC", "Overlay module", "XEDIT"]
util_files = ["X-DOS System", "SX-BASIC", "Overlay module", "AUTO RUN.BAS"]

collect_evidence("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88", "XDOS_SYS.D88", sys_files)
collect_evidence("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88", "XDOSUTIL.D88", util_files)
