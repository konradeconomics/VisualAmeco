# CSV Ameco Dataset Strategy

## Overview

This strategy outlines how to parse, transform, and work with the CSV dataset structured with rich metadata.
---

## CSV Structure

### 1. **Metadata Columns (First 11)**
These describe the type of data:
- `SERIES`
- `CNTRY`
- `TRN`
- `AGG`
- `UNIT`
- `REF`
- `CODE`
- `COUNTRY`
- `SUB-CHAPTER`
- `TITLE`
- `UNIT`

### 2. **Data Columns (Per-Year Values)**
- From **1960** to **2026**
- Contains data in e.g. `"1000 persons"`
- Missing values are represented as `NA`

---

## Parsing Strategy

### Step 1: **Read the CSV**

Use the provided `CsvFileReader` implementation to read all rows as string arrays.

### Step 2: Preprocessing

After reading the CSV file, apply the following preprocessing steps:

- **Trim extra columns**: Some rows may have trailing empty columns or "NA" values beyond the expected years. Remove these to maintain consistency.
- **Capture header row**: Use the header to identify the year columns dynamically (typically starting from index 11 onward).
- **Normalize missing values**: Replace `"NA"` with `null` or another sentinel value. This ensures numeric operations can handle gaps appropriately.
- **Type conversion**: Convert population values from strings to numerical types (e.g., `double`) to support calculations and analysis.

---

### Step 3: Reshape to Long Format

The CSV is originally in wide format — each year is a separate column. For analysis and filtering, reshape it into long format:

- Each row becomes a unique combination of:
    - Country
    - Metric (e.g., "Total population")
    - Year
    - Value
- This transformation supports:
    - Easier filtering (e.g., by country or year)
    - Grouping and aggregation (e.g., total EU population over time)
    - Visualization in line charts or tables

Example:

| COUNTRY  | TITLE             | Year | Value |
|----------|-------------------|------|-------|
| Belgium  | Total population  | 2000 | 10.2  |
| Belgium  | Total population  | 2001 | 10.4  |