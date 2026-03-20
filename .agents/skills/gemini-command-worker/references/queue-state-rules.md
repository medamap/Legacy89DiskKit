# Queue State Rules

- `command_waiting`: untouched instruction
- `command_processing`: actively being worked
- `command_processed`: execution finished
- `report_waiting`: report submitted and awaiting Codex review

Move the instruction into `command_processing` before starting work.  
Move it into `command_processed` only after the report has been written.
