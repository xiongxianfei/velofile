# Install, Rollback, And Uninstall

VeloFile V1 is distributed as an MSIX package that installs side by side with Windows File Explorer. It does not replace Explorer, does not register as the desktop shell, and does not take ownership of system file associations.

## Install

Install the signed MSIX from the published release source documented in `docs/release/stable-update-channel.md`. After install, launch VeloFile from the Start menu or the package launch surface and confirm File Explorer still opens normally.

## Update

Install the newer signed MSIX from the same stable update channel. Session, settings, favorites, and recent-location data are versioned; newer builds must either migrate, ignore unknown fields, or fall back safely without blocking launch.

## Rollback

Uninstalling VeloFile is the rollback path. If a release must be rolled back, uninstall the current MSIX and install the earlier signed MSIX from the published release source.

Explorer remains available before, during, and after rollback. The system file associations remain owned by Windows and user defaults, not VeloFile.

## Uninstall

Use Windows Settings, Start menu uninstall, or the standard MSIX removal path. Uninstall removes the VeloFile package. It does not repair Explorer because VeloFile does not replace Explorer, and it does not repair file associations because VeloFile does not modify global file associations.
