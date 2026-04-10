# X1 I/O Port Reference

This reference was provided as external machine information for Sharp X1 family hardware.
It is stored here as supporting material for X-DOS kernel reverse engineering, especially
when interpreting FDC, DMA, memory-bank, and ROM-mapping related code paths.

Use this file as a hardware-side lookup aid only. It does not by itself prove how X-DOS
uses a given port in the kernel. Kernel usage still requires byte-level evidence.

| Port Address | Description | Input Effect | Output Effect | Models | Notes |
| --- | --- | --- | --- | --- | --- |
| 0700H | YM2151 (FM sound) address port | - | Select YM2151 register number | Z | Standard FM sound on turboZ |
| 0701H | YM2151 (FM sound) data port | Read FM status | Write data to selected register | Z | - |
| 0704H-0707H | CTC (for FM sound) ch0-3 | Read timer/counter or status | Configure timer/counter mode and constants | Z | 0704H also used as latch for FM software checks |
| 0800H | Color image board control | - | Set digitizer resolution and split modes | X1/turbo | CZ-8BV1/2 |
| 0801H | Color image board image data | Read digitized image data | - | X1/turbo | Transfer to G-RAM etc. |
| 0A00H | Stereo board control | - | Control shutter/page selection per VSync | All | CZ-8BR1 |
| 0A04H-0A07H | CTC (stereo board) ch0-3 | Read timer/counter or status | Configure timer/counter | All | - |
| 0B00H | Main/bank memory switch | Read current setting (Z etc.) | Select main memory/bank memory and bank number | turbo/Z | Extended memory-space switch |
| 0C**H | RS-232C card (CZ-8RS) | Read control/status or receive data | Write control or transmit data | All | Address varies by DIP |
| 0D00H-0D03H | External RAM board (EMM) | 0D03H reads EMM data | 0D00H-0D02H set addr, 0D03H writes data | All | Internal address auto-increments |
| 0E00H-0E03H | External ROM (BASIC ROM etc.) | 0E03H reads ROM data | 0E00H-0E02H set address | All | No auto-increment |
| 0E80H-0E82H | Kanji ROM / EPROM | 0E80H/0E81H read pattern halves | 0E80H/0E81H set JIS/ROM addr, 0E82H start read | All | Requires 3us+ wait |
| 0FD0H-0FD3H | Hard disk (SCSI) | Read SCSI status/data | Write SCSI control/data | All | 10MB HDD etc. |
| 0FE8H-0FEFH | 8-inch floppy disk | Read FDC status/data/drive info | Write FDC command/data and drive control | turbo/Z | 8-inch variant |
| 0FF8H-0FFFH | 5-inch floppy disk (FDC etc.) | 0FF8H FDC status, 0FF9H-0FFBH track/sector/data, 0FFDH-0FFFH drive status | 0FF8H FDC command, 0FF9H-0FFBH track/sector/data, 0FFCH drive/side/motor | All | MB8877A/8876 based |
| 10**H, 11**H, 12**H | Graphic palette (Blue, Red, Green) | Read palette values (Z multicolor only) | Set display color codes | All | More detailed on Z |
| 13**H | Priority setting | - | Set text/graphics/background priority | All | Default 00H |
| 14**H-17**H | CG ROM / PCG RAM | 14**H reads CG ROM, 15**H-17**H read PCG B/R/G | 15**H-17**H write PCG B/R/G | All | turbo/Z has faster mode |
| 1800H-1801H | CRTC | - | 1800H selects register, 1801H writes data | All | HD46505-SP |
| 1900H | Sub-CPU (80C49) comm port | Read 1-byte data from sub-CPU | Send command/data to sub-CPU | All | Handshake via 1A01H |
| 1A00H-1A03H | Main-side 8255 PPI | 1A01H reads VBLANK, HSYNC, printer BUSY, sub-CPU flags | 1A00H printer data, 1A02H printer strobe/PCG/cassette, 1A03H PPI mode | All | Core peripheral control |
| 1B00H | PSG data | Read selected PSG register | Write selected PSG register | All | Also joystick etc. |
| 1C00H | PSG register select | - | Select PSG register number | All | Must precede 1B00H |
| 1D**H | IPL ROM enable (ON) | - | Map IPL ROM to 0000H-7FFFH | turbo/Z | Any output value |
| 1E**H | IPL ROM disable (OFF) | - | Restore main RAM at 0000H-7FFFH | turbo/Z | Any output value |
| 1F80H | DMA (Z80 DMA) | Read status/internal registers | Write commands, addr, size, mode | turbo/Z | Used for fast transfers |
| 1F90H-1F93H | SIO (Z80 SIO/0) | Read status/receive data | Write control/transmit data | turbo/Z | Channel B often serial mouse |
| 1FA0H-1FA3H | CTC (Z80 CTC) | Read counter values etc. | Configure mode, constants, vector | turbo/Z | For SIO clocks/timers |
| 1FB0H | Z mode select | Read current state | Set monitor mode, multicolor, capture, superimpose | Z | Main Z-only mode port |
| 1FB9H-1FBFH | Z text palette | Read color setting values | Set text RGB intensity | Z | Z-only extended palette |
| 1FC0H | Z priority select | Read current priority | Set bank/text/graphics priority | Z | - |
| 1FC1H | Z image capture position correction | Read current correction | Set dot-level capture adjustment | Z | - |
| 1FC2H | Z mosaic/quantization capture | Read current state | Set mosaic size and quantization level | Z | - |
| 1FC3H | Z chroma key | Read current chroma key | Set transparent/removed color | Z | - |
| 1FC4H | Z scroll | Read current scroll/output state | Set scroll amount and CRT output cut | Z | - |
| 1FC5H | Z text palette mode | Read current palette mode | Control text palette read mode ON/OFF | Z | - |
| 1FD0H | Screen management | Read current screen management state (Z only) | Control 200/400 lines, bank select, PCG fast mode | turbo/Z | - |
| 1FF0H | Start port | Read DIP/front switch state | - | Z | Boot device and resolution |
| 2000H-27FFH | Text attribute VRAM | Read attribute bytes | Write text attribute bytes | All | One byte per character |
| 3000H-37FFH | Text VRAM | Read character codes | Write character codes | All | Mapping varies by mode |
| 3800H-3FFFH | Kanji VRAM | Read kanji info | Write kanji ROM/page/underline/half info | turbo/Z | Paired with text VRAM |
| 4000H-FFFFH | Graphics RAM (G-RAM) | Read pixel data | Write pixel data | All | B/R/G planes or simultaneous access |
