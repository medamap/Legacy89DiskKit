# Common Use Cases

## Purpose

This document groups common user workflows without repeating low-level implementation detail.

## Inspect a Disk Image

Use this when you want to:

- detect the filesystem
- list files
- inspect disk size and free space
- inspect boot metadata

Typical commands:

- `list <image>`
- `boot show <image>`
- `layout export <image>`

## Create Blank Media

Use this when you want an unformatted disk image container.

Typical command:

- `disk create <image> --disk-type 2d|2dd|2hd`

## Format a Disk

Use this when you want to initialize an existing image with a specific filesystem.

Typical command:

- `disk format <image> --file-system hu-basic|n88-basic|msx-dos|xdos`

## Inject a Host File

Use this when you want to add one host file into an existing disk image.

Typical command:

- `inject <image> <host-file>`

## Copy Files Between Images

Use this when you want logical file transfer instead of raw physical duplication.

Typical command family:

- `file ...`

## Inspect or Transfer Boot Information

Use this when you want to inspect boot metadata or move boot-related payloads without doing a full physical disk copy.

Typical command family:

- `boot ...`

## Preserve Directory Order

Use this when directory entry order matters for a target workflow.

Typical command family:

- `layout export`
- `layout validate`
- `layout apply`

## Clone a Bootable X-DOS Disk

Use this when you need a bootable logical clone of a known X-DOS source image.

Current public guidance:

1. create a destination image with explicit X-DOS formatting
2. transfer boot information
3. copy files

## Notes

- A logical copy is not the same as a full physical disk copy.
- Boot transfer, file transfer, and physical duplication should be treated as separate workflows.
- When exact physical behavior matters, validate with emulator or real-host testing.
