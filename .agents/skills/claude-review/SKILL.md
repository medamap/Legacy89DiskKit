---
name: claude-review
description: Performs an automated peer review of an implementation using Claude.
---

# Claude Review Skill

This skill integrates Claude as an external review system into the development workflow. It should be used after every functional unit implementation to ensure code quality and architectural alignment.

## Core Workflow

1.  **Implementation**: Complete a single functional unit or feature.
2.  **Document**: Update a walkthrough or create a specific analysis markdown of the implementation.
3.  **Review**: Run the review command:
    `claude --dangerously-skip-permissions --print "$(cat <absolute_path_to_artifact>)\n\n次のファイルを読み込んで、あなたの見解を聞かせて"`
4.  **Verification**: If the review is positive, perform behavioral testing (e.g., creating disk images, copying files).
5.  **Iteration**: If the review or tests fail, refine the implementation and repeat.

## Commands

- `claude --dangerously-skip-permissions --print "$(cat <path>)\n\n次のファイルを読み込んで、あなたの見解を聞かせて"`: Sends the file at `<path>` to Claude for review.
