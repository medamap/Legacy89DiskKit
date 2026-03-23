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

print("Searching for XDOS_SYS's SX-BASIC (06 08) pattern...")
pattern1 = bytes.fromhex("00197e32d7c2cd3ac221ecc222ddc22a")
find_bytes('images/disk_org/x1/XDOS_SYS.D88', pattern1)
find_bytes('images/disk_org/x1/XDOSUTIL.D88', pattern1)

print("Searching for XDOSUTIL's SX-BASIC (04 02) pattern...")
pattern2 = bytes.fromhex("8deddad1c03e8ecd33ed3a0fc03c320f")
find_bytes('images/disk_org/x1/XDOS_SYS.D88', pattern2)
find_bytes('images/disk_org/x1/XDOSUTIL.D88', pattern2)

print("Searching for XDOS_SYS's Overlay module (09 02) pattern...")
pattern3 = bytes.fromhex("c381c0c31dc0c346c0b0ec11b1ec0610")
find_bytes('images/disk_org/x1/XDOS_SYS.D88', pattern3)
find_bytes('images/disk_org/x1/XDOSUTIL.D88', pattern3)
