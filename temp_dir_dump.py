import struct
import sys

def dump_dir(d88_path):
    print(f"=== {d88_path} ===")
    with open(d88_path, 'rb') as f:
        data = f.read()
    
    # Directory is usually at Track 1, Sector 2
    # Let's just find the directory by looking for known names like "X-DOS System"
    # Wait, the boot_notes say: Track 1, R=2 is Record 11.
    # Entry 1 at offset 0x1650 in XDOS_SYS.D88
    
    base_offset = 0x1650
    for i in range(16):
        entry = data[base_offset + i*32 : base_offset + (i+1)*32]
        if entry[0] == 0: # empty or deleted? wait, usually 0x00 or 0xE5 is deleted, but let's just print
            continue
        filename = entry[2:18].decode('ascii', errors='ignore').strip()
        if not filename:
            continue
        val_1d = entry[0x1D]
        val_1e = entry[0x1E]
        print(f"File: {filename:16s} | 0x1D: {val_1d:02X} | 0x1E: {val_1e:02X}")

dump_dir("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOS_SYS.D88")
dump_dir("/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/images/disk_org/x1/XDOSUTIL.D88")
