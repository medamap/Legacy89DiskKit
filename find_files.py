import sys

def parse_d88(filename):
    with open(filename, 'rb') as f:
        data = f.read()

    pointers = []
    for i in range(164):
        offset = 0x20 + i * 4
        ptr = int.from_bytes(data[offset:offset+4], 'little')
        if ptr > 0:
            pointers.append(ptr)
            
    sectors = []
    for p in sorted(list(set(pointers))):
        offset = p
        while True:
            if offset + 16 > len(data):
                break
            C = data[offset]
            H = data[offset+1]
            R = data[offset+2]
            data_len = int.from_bytes(data[offset+14:offset+16], 'little')
            if data_len == 0 or data_len > 8192:
                break
            sec_data = data[offset+16:offset+16+data_len]
            sectors.append({'C': C, 'H': H, 'R': R, 'data': sec_data, 'offset': offset})
            offset += 16 + data_len
            if offset >= len(data) or (len(sectors) % 40 == 0): # just a rough break
                break
            # D88 track ends when we hit the next pointer
            # We sorted the pointers, so if we hit or exceed the next pointer, we break.
            next_ptrs = [x for x in sorted(list(set(pointers))) if x > p]
            if next_ptrs and offset >= next_ptrs[0]:
                break

    return sectors

sectors_sys = parse_d88('images/disk_org/x1/XDOS_SYS.D88')

def find_file_start(sectors, search_str):
    for s in sectors:
        if search_str in s['data']:
            print(f"Found '{search_str}' at C={s['C']}, H={s['H']}, R={s['R']}")
            return (s['C'], s['R'])
    return None

print("XDOS_SYS:")
for i in range(15):
    s = sectors_sys[i]
    #print(f"C={s['C']}, H={s['H']}, R={s['R']}, preview={s['data'][:16].hex()}")

# Let's just print the first 16 bytes of C=2..10 to identify file boundaries
for s in sectors_sys:
    if s['C'] >= 2 and s['C'] <= 10:
        print(f"C={s['C']:02X} R={s['R']:02X} : {s['data'][:16].hex()} {s['data'][:16].decode('ascii', 'replace')}")

