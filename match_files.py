import sys

def get_file_content(d88_path, offset, size=16):
    with open(d88_path, 'rb') as f:
        f.seek(offset)
        return f.read(size)

# I can't easily extract files without the file system, but find_file_start found them at offsets:
# XDOS_SYS.D88 offset 0x53b0 -> X-DOS System X1 ?
# XDOS_SYS.D88 offset 0x8950 -> SX-BASIC ?
# XDOSUTIL.D88 offset 0x8530 -> Overlay module ?

# Actually, I know the answers because the pairs map exactly to the 0x1D/0x1E of those files.

print("Done")
