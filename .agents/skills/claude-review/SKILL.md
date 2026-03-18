---
name: claude-review
description: Performs an automated peer review of an implementation using Claude.
---

# Claude Review Skill

This skill integrates Claude as an external review system into the development workflow. It should be used after every functional unit implementation to ensure code quality and architectural alignment.

## ⚠️ 絶対禁止事項（最優先）

**Claudeのレビューが完了し「マージOK」の判定が出ても、develop へのマージ・プッシュは絶対に自分で行ってはならない。**
必ずユーザーに報告し、明示的な「マージしてください」の指示を受けてから Gemini がマージ・プッシュを実行すること。
この手順を守らない場合、develop ブランチが汚染され、手動での revert 作業が必要になる。

## Core Workflow

1.  **Implementation**: Complete a single functional unit or feature.
2.  **Document**: 変更したファイルのパスをカンマ区切りで列挙し、実装の概要をまとめる。
3.  **Review**: Run the review command (変更ファイルのパスを列挙して渡すこと):
    `claude --dangerously-skip-permissions --print "以下のファイルを読み込んでレビューしてください。\n変更ファイル: <comma-separated file paths>\n\n$(cat <absolute_path_to_report>)"`
4.  **Verification**: If the review is positive, perform behavioral testing (e.g., creating disk images, copying files).
5.  **Iteration**: If the review or tests fail, refine the implementation and repeat.
6.  **Report to user**: レビュー合格後、ユーザーに結果を報告し、**マージ指示を待つ**。

## Commands

- `claude --dangerously-skip-permissions --print "以下のファイルを読み込んでレビューしてください。\n変更ファイル: <comma-separated file paths>\n\n$(cat <path>)"`: Sends the report and changed file list to Claude for review.
