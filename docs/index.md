[Quick Start](QuickStart.md) | [User Guide](UserGuide.md) | [API documentation](api/index.md)

## Documentation

- [Quick Start](QuickStart.md)
- [User Guide](UserGuide.md)
- [API documentation](api/index.md)

## Curve data point counts

Motor curve data is stored as a percent axis, RPM axis, and one or more torque series.

- Supported point counts: 0–101 points per series.
- Standard curves: 101 points (0%..100% in 1% increments).
- Coarse curves: fewer points are allowed.
- Overspeed: percent values above 100% are allowed in the file format, but the Motor Editor does not currently provide UI to author overspeed axes (JSON editing only for now).

When a series has 0 points, clients can fall back to rated continuous/peak torque values for visualization.
