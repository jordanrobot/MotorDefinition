#### [MotorDefinition](index.md 'index')
### [JordanRobot\.MotorDefinition\.Services](JordanRobot.MotorDefinition.Services.md 'JordanRobot\.MotorDefinition\.Services')

## PrecisionRounding Class

Provides utility methods for detecting and correcting floating\-point precision errors
that occur during unit conversions and mathematical operations\.

```csharp
public static class PrecisionRounding
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; PrecisionRounding
### Fields

<a name='JordanRobot.MotorDefinition.Services.PrecisionRounding.DefaultThreshold'></a>

## PrecisionRounding\.DefaultThreshold Field

Default threshold for detecting precision errors \(1e\-10\)\.

```csharp
public const double DefaultThreshold = 1E-10;
```

#### Field Value
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')
### Methods

<a name='JordanRobot.MotorDefinition.Services.PrecisionRounding.CorrectPrecisionError(double,double)'></a>

## PrecisionRounding\.CorrectPrecisionError\(double, double\) Method

Corrects floating\-point precision errors by rounding values that are very close to
round numbers based on the specified threshold\.

```csharp
public static double CorrectPrecisionError(double value, double threshold=1E-10);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.PrecisionRounding.CorrectPrecisionError(double,double).value'></a>

`value` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The value to potentially round\.

<a name='JordanRobot.MotorDefinition.Services.PrecisionRounding.CorrectPrecisionError(double,double).threshold'></a>

`threshold` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The threshold for detecting precision errors\. If the fractional part of the value
is smaller than this threshold from a round number, it will be rounded\.
Default is 1e\-10\. Set to 0 or negative to disable rounding\.

#### Returns
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')  
The corrected value with precision errors removed, or the original value if no
correction is needed or threshold is disabled\.

### Remarks
This method helps fix conversion rounding errors like:
\- 50\.1300000000034 \-\> 50\.13
\- 1\.4999999999998 \-\> 1\.5
\- 6\.2000000000001 \-\> 6\.2

The method works by determining a reasonable number of decimal places for the value,
then checking if the difference between the original and rounded value is within
the threshold\. This approach preserves legitimate precision while fixing errors\.

<a name='JordanRobot.MotorDefinition.Services.PrecisionRounding.CorrectPrecisionErrors(double[],double)'></a>

## PrecisionRounding\.CorrectPrecisionErrors\(double\[\], double\) Method

Corrects floating\-point precision errors in an array of values\.

```csharp
public static double[] CorrectPrecisionErrors(double[] values, double threshold=1E-10);
```
#### Parameters

<a name='JordanRobot.MotorDefinition.Services.PrecisionRounding.CorrectPrecisionErrors(double[],double).values'></a>

`values` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')

The array of values to correct\.

<a name='JordanRobot.MotorDefinition.Services.PrecisionRounding.CorrectPrecisionErrors(double[],double).threshold'></a>

`threshold` [System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')

The threshold for detecting precision errors\.

#### Returns
[System\.Double](https://learn.microsoft.com/en-us/dotnet/api/system.double 'System\.Double')[\[\]](https://learn.microsoft.com/en-us/dotnet/api/system.array 'System\.Array')  
A new array with precision errors corrected\.