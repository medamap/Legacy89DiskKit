# Phase 24 First Host Proof Report Template

## Summary

- Host family:
- Bridge repository or location:
- Bridge commit or revision:
- Proof date:
- Operator:

## Selected Integration Mode

- Open mode:
  - `OpenDiskPath`
  - or `OpenDiskImage`
- Exchange mode:
  - plain request and response
  - or notification-aware exchange
- Transport mode:
  - stdio
  - pipe
  - socket
  - other

## Capability Handshake

- `QueryCapabilities` succeeded:
- `ProtocolVersion`:
- `SupportsPathOpen`:
- `SupportsBufferOpen`:
- `SupportsNotificationExchange`:
- `SupportsPlainStdio`:
- `SupportsObservableStdio`:

## D88-Backed Proof

- D88 open succeeded:
- Drive selection succeeded:
- Side selection succeeded:
- Register write flow succeeded:
- `read sector` command succeeded:
- Busy state observed:
- Advance scheduling worked:
- IRQ observed:
- DRQ observed:
- Data register read succeeded:
- Close succeeded:

## Raw Sector-Image Follow-Up

- Raw open attempted:
- Raw open succeeded:
- Same request flow reused:
- Any bridge-side branch was required:

## Timing Notes

- Host timing model used:
- How `Advance` was scheduled:
- Whether `PendingAdvanceMicroseconds` was used:
- Whether notification-driven advance requests were used:
- Any unclear timing semantics:

## Signal Notes

- IRQ handling mode:
  - edge-triggered
  - level-triggered
  - polling-only
- DRQ handling mode:
  - edge-triggered
  - level-triggered
  - polling-only
- Any unclear signal semantics:

## Missing Pieces

- Missing request kinds:
- Missing notifications:
- Missing status semantics:
- Host-specific assumptions that were required:

## Conclusion

- Fixed Phase 23 request set was sufficient:
- Protocol expansion is required:
- If expansion is required, describe the smallest missing piece:

## Attachments

- Request transcript location:
- Response or exchange transcript location:
- Relevant bridge log location:
