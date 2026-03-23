def dump_sector(filename, target_c, target_r):
    with open(filename, 'rb') as f:
        data = f.read()
    
    pointers = []
    for i in range(164):
        offset = 0x20 + i * 4
        ptr = int.from_bytes(data[offset:offset+4], 'little')
        if ptr > 0: pointers.append(ptr)
        
    for p in pointers:
        offset = p
        for _ in range(40):
            if offset + 16 > len(data): break
            C = data[offset]
            H = data[offset+1]
            R = data[offset+2]
            sec_len = int.from_bytes(data[offset+14:offset+16], 'little')
            if C == target_c and R == target_r:
                sec_data = data[offset+16:offset+16+sec_len]
                print(f"[{filename}] C={C:02X} R={R:02X} -> {sec_data[:32].hex()} {sec_data[:32].decode('ascii', 'replace')}")
                return
            offset += 16 + sec_len

print("Checking XDOS_SYS SX-BASIC (06 08)")
dump_sector('images/disk_org/x1/XDOS_SYS.D88', 6, 8)
print("Checking XDOSUTIL SX-BASIC (04 02)")
dump_sector('images/disk_org/x1/XDOSUTIL.D88', 4, 2)
print("Checking XDOS_SYS AUTO RUN.BASNEW (08 0A)")
dump_sector('images/disk_org/x1/XDOS_SYS.D88', 8, 10)
print("Checking XDOSUTIL AUTO RUN.BAS (06 04)")
dump_sector('images/disk_org/x1/XDOSUTIL.D88', 6, 4)
print("Checking XDOS_SYS Overlay module (09 02)")
dump_sector('images/disk_org/x1/XDOS_SYS.D88', 9, 2)
print("Checking XDOSUTIL Overlay module (06 06)")
dump_sector('images/disk_org/x1/XDOSUTIL.D88', 6, 6)
