<!-- markdownlint-disable-file -->
# Validation Signature Editing Locks Details

## T1: Evaluate signatures on load

- After a motor file is loaded or a new motor is created, verify motor, drive, and curve validation signatures using the existing `DataIntegrityService`.
- Store lock state per motor, drive, and curve without mutating the underlying model so that future signature add/remove UI can toggle state cleanly.
- Surface computed lock state through view model properties that can be reused by future UI.

## T2: Enforce locks in UI and viewmodels

- Prevent edits to motor properties (including units) when a valid motor signature is present.
- Prevent edits to selected drive and its voltages when that drive has a valid signature; allow adding other drives that are unsigned.
- Prevent edits (rename, delete, data changes) to curve series with valid signatures; ensure data grids and commands respect the lock.
- Keep user feedback minimal but clear (status messaging) while avoiding destructive changes to signatures.

## T3: Tests

- Add targeted view model tests that cover signature verification and editing lock behavior for motor, drive/voltage, and curve series paths.
- Prefer unit-level tests that reuse existing factories/mocks and real `DataIntegrityService` for realistic signatures.
- Ensure tests assert that data does not change when a locked section is edited and that unlocked data remains editable.
