import sys
import json
import os

def hex_string(val, bits=8):
    return hex(val).upper().replace("0X", "")

def analyze_window(hex_bytes, start_addr, metadata):
    # Simulated Z80 Disassembly and Hardware Mapping
    # NOTE: This is a simplified draft. A production version would use a full disassembler library.
    
    io_ports = metadata.get("io_ports", {})
    results = []
    i = 0
    while i < len(hex_bytes):
        byte = hex_bytes[i]
        addr = start_addr + i
        line = {"addr": addr, "bytes": [byte], "instr": f"DB {hex_string(byte)}H", "note": ""}
        
        # Simple Opcode Matching Example
        if byte == 0xD3: # OUT (n), A
            if i + 1 < len(hex_bytes):
                port = hex_bytes[i+1]
                port_full = f"00{hex_string(port)}".zfill(4)
                line["bytes"].append(port)
                line["instr"] = f"OUT ({hex_string(port)}H), A"
                line["note"] = io_ports.get(port_full, "Unknown Port")
                i += 1
        elif byte == 0xDB: # IN A, (n)
            if i + 1 < len(hex_bytes):
                port = hex_bytes[i+1]
                port_full = f"00{hex_string(port)}".zfill(4)
                line["bytes"].append(port)
                line["instr"] = f"IN A, ({hex_string(port)}H)"
                line["note"] = io_ports.get(port_full, "Unknown Port")
                i += 1
        elif byte == 0xED: # Multi-byte opcodes
            if i + 1 < len(hex_bytes):
                next_byte = hex_bytes[i+1]
                line["bytes"].append(next_byte)
                if next_byte == 0x79: # OUT (C), A
                    line["instr"] = "OUT (C), A"
                    line["note"] = "Hardware Access via C register"
                elif next_byte == 0x78: # IN A, (C)
                    line["instr"] = "IN A, (C)"
                    line["note"] = "Hardware Access via C register"
                i += 1
        elif byte == 0x11: # LD DE, nn
            if i + 2 < len(hex_bytes):
                low = hex_bytes[i+1]
                high = hex_bytes[i+2]
                val = (high << 8) | low
                line["bytes"].extend([low, high])
                line["instr"] = f"LD DE, {hex_string(val, 16)}H"
                if hex_string(val, 16) in metadata.get("memory_regions", {}):
                    line["note"] = metadata["memory_regions"][hex_string(val, 16)]
                i += 2
        
        results.append(line)
        i += 1
    return results

if __name__ == "__main__":
    # Test script with sample hex input
    hex_input = [0x11, 0x00, 0x40, 0xD3, 0xF8, 0xED, 0x78]
    metadata_path = os.path.join(os.path.dirname(__file__), "x1_metadata.json")
    with open(metadata_path, 'r') as f:
        metadata = json.load(f)
    report = analyze_window(hex_input, 0xC9EA, metadata)
    for r in report:
        print(f"{hex(r['addr']).upper().replace('0X', '')}: {r['instr']} ; {r['note']}")
