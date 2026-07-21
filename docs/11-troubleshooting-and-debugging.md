# 11 - Troubleshooting and Debugging

## Symptom: Blank output

Check in this order:

1. Does resolver return rows?
2. Does logical tree builder create bands?
3. Does report layout contain pages and children?
4. Does exporter/renderer map styles/coordinates correctly?

## Symptom: Data exists but not visible

Common causes:
- Coordinate origin errors in nested elements.
- Overflow clipping with wrong relative positions.
- Text color invisible against page background.
- Unit mismatch (`pt` vs `px`/DIP).

## Symptom: Overlapping rows

Common causes:
- Font measurement mismatch with rendered CSS units.
- Line-height differences between layout and output.

## Fast Debug Playbook

```mermaid
flowchart TB
	A[Start with failing report] --> B[Log resolved rows per source]
	B --> C[Log band count and ids]
	C --> D[Inspect first page bounds]
	D --> E[Inspect exported output style units]
	E --> F[Fix at earliest failing stage]
```

## What to Log

- Data-source ID and row count.
- Band IDs and kinds.
- Element bounds (`x,y,w,h`) for first page.
- Final HTML/CSS units for text size and line height.

## Safe Fix Priority

1. Data resolver correctness.
2. Tree builder correctness.
3. Layout engine correctness.
4. Exporter/renderer mapping.
