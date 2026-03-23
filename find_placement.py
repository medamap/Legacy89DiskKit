import sys

def find_string_in_d88(filename, search_str):
    with open(filename, 'rb') as f:
        data = f.read()

    # Linear scan of the entire file, ignoring the 164 track pointers.
    # Sector headers are 16 bytes.
    # Structure: C H R N NumSecs(2) ... DataLen(2)
    # We start at 0x2B0 (after track pointers)
    
    offset = 0x2b0
    found = False
    while offset < len(data) - 16:
        C = data[offset]
        H = data[offset+1]
        R = data[offset+2]
        sec_len = int.from_bytes(data[offset+14:offset+16], 'little')
        
        if sec_len == 0 or sec_len > 8192:
            # likely lost sync, advance by 1 to resync? no, d88 is packed
            # but if it fails, maybe just scan raw data?
            break
            
        sec_data = data[offset+16:offset+16+sec_len]
        
        if search_str.encode('ascii') in sec_data:
            print(f"[{filename}] Found '{search_str}' at placement C={C:02X}, H={H:02X}, R={R:02X} (Hex: {C:02X} {R:02X})")
            found = True
            # keep searching to see if it spans multiple or if we want the first
            break
            
        offset += 16 + sec_len
        
    if not found:
        print(f"[{filename}] '{search_str}' NOT FOUND")

find_string_in_d88('images/disk_org/x1/XDOS_SYS.D88', 'SX-BASIC')
find_string_in_d88('images/disk_org/x1/XDOSUTIL.D88', 'SX-BASIC')
find_string_in_d88('images/disk_org/x1/XDOS_SYS.D88', 'AUTO RUN.BAS')
find_string_in_d88('images/disk_org/x1/XDOSUTIL.D88', 'AUTO RUN.BAS')
find_string_in_d88('images/disk_org/x1/XDOS_SYS.D88', 'Overlay module')
find_string_in_d88('images/disk_org/x1/XDOSUTIL.D88', 'Overlay module')

