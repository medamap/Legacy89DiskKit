---
name: codex-review
description: Performs an automated peer review of an implementation using the external Codex system.
---

# Codex Review Skill

This skill is designed to integrate an external "Codex" review system into the development workflow. It should be used after every functional unit implementation to ensure code quality and architectural alignment.

## Core Workflow

1.  **Implementation**: Complete a single functional unit or feature.
2.  **Document**: Update a walkthrough or create a specific analysis markdown of the implementation.
3.  **Review**: Run the review command:
    `codex exec '次のファイルを読み込んで、あなたの見解を聞かせて <absolute_path_to_artifact> '`
4.  **Verification**: If the review is positive, perform behavioral testing (e.g., creating disk images, copying files).
5.  **Iteration**: If the review or tests fail, refine the implementation and repeat.

## Commands

- `codex exec '次のファイルを読み込んで、あなたの見解を聞かせて <path> '`: Sends the file at `<path>` to Codex for review.
