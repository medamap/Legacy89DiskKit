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

## ⚠️ レビュー対象の定義（重要）

**Claudeがレビューするのは、フィーチャーブランチのHEAD（コミット済みの状態）のみ。**

- レビュー依頼前に、すべての変更を必ずフィーチャーブランチにコミットすること
- ローカルの未コミット変更はレビュー対象に含まれない
- `git status` でクリーンな状態（変更なし）であることを確認してからレビュー依頼すること
- 「修正しました」と報告しても、コミットされていなければ修正は存在しないものとして扱われる

## Core Workflow

1.  **Implementation**: Complete a single functional unit or feature. **必ずフィーチャーブランチ（`codex/vX-XX-xxx`）上で作業すること。develop への直接コミットは禁止。**
2.  **Commit**: すべての変更をフィーチャーブランチにコミットする。`git status` がクリーンであることを確認すること。
3.  **Document**: 変更したファイルのパスをカンマ区切りで列挙し、実装の概要をまとめる。
4.  **Review**: Run the review command (変更ファイルのパスを列挙して渡すこと):
    `claude --dangerously-skip-permissions --print "以下のファイルを読み込んでレビューしてください。\n変更ファイル: <comma-separated file paths>\n\n$(cat <absolute_path_to_report>)"`
5.  **Verification**: If the review is positive, perform behavioral testing (e.g., creating disk images, copying files).
6.  **Iteration**: If the review or tests fail, refine the implementation, commit again, and repeat.
7.  **Report to user**: レビュー合格後、ユーザーに結果を報告し、**マージ指示を待つ**。

## Commands

- `claude --dangerously-skip-permissions --print "以下のファイルを読み込んでレビューしてください。\n変更ファイル: <comma-separated file paths>\n\n$(cat <path>)"`: Sends the report and changed file list to Claude for review.
