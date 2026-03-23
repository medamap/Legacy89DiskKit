import sys

def dump_sector_data(filename, target_c, target_r):
    with open(filename, 'rb') as f:
        data = f.read()
    
    offset = 0x2b0
    while offset < len(data) - 16:
        C = data[offset]
        H = data[offset+1]
        R = data[offset+2]
        sec_len = int.from_bytes(data[offset+14:offset+16], 'little')
        if sec_len == 0 or sec_len > 8192: break
        
        if C == target_c and R == target_r:
            sec_data = data[offset+16:offset+16+sec_len]
            # print first 32 bytes
            hex_str = sec_data[:32].hex()
            ascii_str = ''.join(chr(b) if 32 <= b <= 126 else '.' for b in sec_data[:32])
            print(f"[{filename}] C={C:02X} R={R:02X} -> {hex_str} | {ascii_str}")
            return
            
        offset += 16 + sec_len

print("Checking X-DOS System (02 01) vs known placement Track 2, R=2 (02 02)")
dump_sector_data('images/disk_org/x1/XDOS_SYS.D88', 2, 1)
dump_sector_data('images/disk_org/x1/XDOS_SYS.D88', 2, 2)
dump_sector_data('images/disk_org/x1/XDOSUTIL.D88', 2, 1)
dump_sector_data('images/disk_org/x1/XDOSUTIL.D88', 2, 2)

print("\nChecking SX-BASIC XDOS_SYS (06 08) vs XDOSUTIL (04 02)")
dump_sector_data('images/disk_org/x1/XDOS_SYS.D88', 6, 8)
dump_sector_data('images/disk_org/x1/XDOSUTIL.D88', 4, 2)

print("\nChecking AUTO RUN.BAS XDOS_SYS (08 0A) vs XDOSUTIL (06 04)")
dump_sector_data('images/disk_org/x1/XDOS_SYS.D88', 8, 10) # 0A = 10
dump_sector_data('images/disk_org/x1/XDOSUTIL.D88', 6, 4)

print("\nChecking Overlay module XDOS_SYS (09 02) vs XDOSUTIL (06 06)")
dump_sector_data('images/disk_org/x1/XDOS_SYS.D88', 9, 2)
dump_sector_data('images/disk_org/x1/XDOSUTIL.D88', 6, 6)

