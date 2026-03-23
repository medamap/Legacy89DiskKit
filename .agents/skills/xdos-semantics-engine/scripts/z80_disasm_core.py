import json
import os

class Z80Disassembler:
    def __init__(self, metadata_path):
        with open(metadata_path, 'r') as f:
            self.metadata = json.load(f)
        self.io_ports = self.metadata.get("io_ports", {})
        self.memory_regions = self.metadata.get("memory_regions", {})
        self.fdc_commands = self.metadata.get("fdc_commands", [])

    def decode_fdc(self, val):
        val_bin = bin(val)[2:].zfill(8)
        for cmd in self.fdc_commands:
            pattern = cmd["pattern"]
            match = True
            for b_idx in range(8):
                if pattern[b_idx] in ('0', '1') and pattern[b_idx] != val_bin[b_idx]:
                    match = False
                    break
            if match:
                return f"{cmd['name']} (Code: {hex(val).upper().replace('0X', '')}H)"
        return None

    def disassemble(self, hex_bytes, start_addr):
        results = []
        i = 0
        while i < len(hex_bytes):
            addr = start_addr + i
            b = hex_bytes[i]
            instr = f"DB {hex(b).upper().replace('0X', '')}H"
            note = ""
            consumed = 1

            if b == 0x11: # LD DE, nn
                if i + 2 < len(hex_bytes):
                    val = (hex_bytes[i+2] << 8) | hex_bytes[i+1]
                    instr = f"LD DE, {hex(val).upper().replace('0X', '')}H"
                    note = self.memory_regions.get(hex(val).upper().replace("0X", ""), "")
                    consumed = 3
            elif b == 0x21: # LD HL, nn
                if i + 2 < len(hex_bytes):
                    val = (hex_bytes[i+2] << 8) | hex_bytes[i+1]
                    instr = f"LD HL, {hex(val).upper().replace('0X', '')}H"
                    note = self.memory_regions.get(hex(val).upper().replace("0X", ""), "")
                    consumed = 3
            elif b == 0x01: # LD BC, nn
                if i + 2 < len(hex_bytes):
                    val = (hex_bytes[i+2] << 8) | hex_bytes[i+1]
                    instr = f"LD BC, {hex(val).upper().replace('0X', '')}H"
                    consumed = 3
            elif b == 0xD3: # OUT (n), A
                if i + 1 < len(hex_bytes):
                    port = hex_bytes[i+1]
                    port_str = hex(port).upper().replace("0X", "").zfill(2)
                    port_full = f"00{port_str}"
                    instr = f"OUT ({port_str}H), A"
                    note = self.io_ports.get(port_full, "")
                    if port_full in ("0FF8", "0FE8"):
                        # Peek ahead or recall previous register state would be needed for full FDC decode
                        # but we can note the port role.
                        note += " [FDC Command]"
                    consumed = 2
            elif b == 0xDB: # IN A, (n)
                if i + 1 < len(hex_bytes):
                    port = hex_bytes[i+1]
                    port_str = hex(port).upper().replace("0X", "").zfill(2)
                    port_full = f"00{port_str}"
                    instr = f"IN A, ({port_str}H)"
                    note = self.io_ports.get(port_full, "")
                    consumed = 2
            elif b == 0xED:
                if i + 1 < len(hex_bytes):
                    b2 = hex_bytes[i+1]
                    if b2 == 0x79: # OUT (C), A
                        instr = "OUT (C), A"
                        note = "I/O via C"
                    elif b2 == 0x78: # IN A, (C)
                        instr = "IN A, (C)"
                        note = "I/O via C"
                    elif b2 == 0x61: # OUT (C), H (Observed in 0xC9EA)
                        instr = "OUT (C), H"
                        note = "I/O via C (Data transfer?)"
                    consumed = 2
            elif b == 0xC9:
                instr = "RET"
                consumed = 1
            elif b == 0xCD: # CALL nn
                 if i + 2 < len(hex_bytes):
                    val = (hex_bytes[i+2] << 8) | hex_bytes[i+1]
                    instr = f"CALL {hex(val).upper().replace('0X', '')}H"
                    consumed = 3
            elif b == 0xC2: # JP NZ, nn
                 if i + 2 < len(hex_bytes):
                    val = (hex_bytes[i+2] << 8) | hex_bytes[i+1]
                    instr = f"JP NZ, {hex(val).upper().replace('0X', '')}H"
                    consumed = 3
            elif b == 0x10: # DJNZ n
                 if i + 1 < len(hex_bytes):
                    rel = hex_bytes[i+1]
                    if rel > 127: rel -= 256
                    instr = f"DJNZ {rel} (Target: {hex(addr + 2 + rel).upper().replace('0X', '')}H)"
                    consumed = 2

            results.append({"addr": addr, "instr": instr, "note": note, "bytes": hex_bytes[i:i+consumed]})
            i += consumed
        return results

if __name__ == "__main__":
    import sys
    # Example usage for 0xC9EA window
    c9ea_bytes = [0x11, 0x00, 0x40, 0x44, 0x4C, 0xED, 0x61, 0x03, 0x1B, 0x7B, 0xB2, 0xC2, 0xBA, 0xCA, 0xED, 0x78, 0xFB, 0xE1, 0xD1, 0xC1, 0xF1, 0xC9]
    meta_path = "/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/Legacy89DiskKit/.agents/skills/xdos-semantics-engine/scripts/x1_metadata.json"
    dis = Z80Disassembler(meta_path)
    lines = dis.disassemble(c9ea_bytes, 0xC9EA)
    print("| Addr | Instruction | Bytes | Hardware Note |")
    print("| :--- | :--- | :--- | :--- |")
    for l in lines:
        byte_str = " ".join([hex(x).upper().replace("0X", "").zfill(2) for x in l["bytes"]])
        print(f"| {hex(l['addr']).upper().replace('0X', '')}H | {l['instr']} | {byte_str} | {l['note']} |")
