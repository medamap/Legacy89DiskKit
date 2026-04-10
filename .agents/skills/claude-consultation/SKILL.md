---
name: claude-consultation
description: Consults Claude for pre-implementation advice and notes.
---

# Claude Consultation Skill

This skill is used *before* implementing a feature. It explains the intended approach to Claude and asks for advice, pitfalls, and suggestions based on the user's requirements.

## Core Workflow

1.  **Preparation**: Create or update a "Consultation Report" (markdown) that describes:
    - The specific goals of the next implementation phase.
    - The intended technical approach (classes, interfaces, logic).
    - The user's specific requirements and constraints.
2.  **Consultation**: Run the consultation command:
    `claude --dangerously-skip-permissions --print "$(cat <absolute_path_to_consultation_report>)\n\nこれからこれをこういうふうに実装するので、アドバイスや注意点をください。"`
3.  **Refinement**: Review Claude's feedback. If there are valid concerns or better suggestions, update the implementation plan or approach accordingly.
4.  **Proceed**: Move to EXECUTION mode once the approach is refined.

## Commands

- `claude --dangerously-skip-permissions --print "$(cat <path>)\n\nこれからこれをこういうふうに実装するので、アドバイスや注意点をください。"`: Sends the consultation report at `<path>` to Claude for advice.
