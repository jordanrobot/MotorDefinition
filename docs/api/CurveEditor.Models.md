#### [MotorDefinition](index.md 'index')

## CurveEditor\.Models Namespace

| Classes | |
| :--- | :--- |
| [CurveSeries](CurveEditor.Models.CurveSeries.md 'CurveEditor\.Models\.CurveSeries') | Represents a named series of motor torque/speed data points\. Each series represents a specific operating condition \(e\.g\., "Peak" or "Continuous"\)\. |
| [DataPoint](CurveEditor.Models.DataPoint.md 'CurveEditor\.Models\.DataPoint') | Represents a single point on a motor torque curve\. |
| [DriveConfiguration](CurveEditor.Models.DriveConfiguration.md 'CurveEditor\.Models\.DriveConfiguration') | Represents a servo drive configuration for a motor\. Contains voltage\-specific configurations and their associated curve series\. |
| [MotorDefinition](CurveEditor.Models.MotorDefinition.md 'CurveEditor\.Models\.MotorDefinition') | Represents a complete motor definition including all properties, drive configurations, and metadata\. Structure: Motor → Drive\(s\) → Voltage\(s\) → CurveSeries |
| [MotorMetadata](CurveEditor.Models.MotorMetadata.md 'CurveEditor\.Models\.MotorMetadata') | Contains metadata about the motor definition file\. |
| [UnitSettings](CurveEditor.Models.UnitSettings.md 'CurveEditor\.Models\.UnitSettings') | Specifies the units used for various motor properties\. |
| [VoltageConfiguration](CurveEditor.Models.VoltageConfiguration.md 'CurveEditor\.Models\.VoltageConfiguration') | Represents voltage\-specific configuration and performance data for a motor/drive combination\. Contains the curve series for this specific voltage setting\. |
