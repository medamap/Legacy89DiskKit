import sys

def find_bytes(filename, pattern):
    with open(filename, 'rb') as f:
        data = f.read()
    
    offset = 0x2b0
    while offset < len(data) - 16:
        C = data[offset]
        H = data[offset+1]
        R = data[offset+2]
        sec_len = int.from_bytes(data[offset+14:offset+16], 'little')
        if sec_len == 0 or sec_len > 8192: break
        
        sec_data = data[offset+16:offset+16+sec_len]
        if pattern in sec_data:
            print(f"[{filename}] Found pattern at C={C:02X} R={R:02X} (Hex: {C:02X} {R:02X})")
            
        offset += 16 + sec_len

pattern = bytes.fromhex("c982b582c483760d2020202020202020")
find_bytes('images/disk_org/x1/XDOSUTIL.D88', pattern)
