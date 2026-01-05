<!-- markdownlint-disable-file -->
# Validation Signature Editing Locks Plan

## Scope

- Enforce validation signature checks whenever motor files are opened.
- Lock editing of signed motor, drive/voltage, and curve data in MotorEditor while leaving unsigned data editable.
- Prepare code paths so future UI for adding/removing validation signatures can plug into the same lock state.

## Tasks

- [x] T1: Evaluate validation signatures on motor load and capture lock state for motor, drives, and curves.
- [ ] T2: Enforce editing locks in UI/viewmodels based on signature validity for motor properties, drives/voltages, and curve series.
- [ ] T3: Add or update tests covering validation signature detection and lock behavior.
- [ ] T4: Update tracking changes file after each task and mark tasks complete.
