---
name: codex-consultation
description: Consults the external Codex system for pre-implementation advice and notes.
---

# Codex Consultation Skill

This skill is used *before* implementing a feature. It explains the intended approach to Codex and asks for advice, pitfalls, and suggestions based on the user's requirements.

## Core Workflow

1.  **Preparation**: Create or update a "Consultation Report" (markdown) that describes:
    - The specific goals of the next implementation phase.
    - The intended technical approach (classes, interfaces, logic).
    - The user's specific requirements and constraints.
2.  **Consultation**: Run the consultation command:
    `codex exec 'これからこれをこういうふうに実装するので、アドバイスや注意点をください、ユーザーの要求はこれです： <absolute_path_to_consultation_report>'`
3.  **Refinement**: Review Codex's feedback. If there are valid concerns or better suggestions, update the implementation plan or approach accordingly.
4.  **Proceed**: Move to EXECUTION mode once the approach is refined.

## Commands

- `codex exec 'これからこれをこういうふうに実装するので、アドバイスや注意点をください、ユーザーの要求はこれです： <path>'`: Sends the consultation report at `<path>` to Codex for advice.
