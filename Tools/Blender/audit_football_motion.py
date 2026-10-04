"""Measure rendered Unity motion traces, independent of the pose generator.

Run after Test-FootballMotion.ps1. This measures continuity/contact, not an
artistic rating, device performance, or proof of human biomechanics.
"""
import argparse
import csv
import json
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--run', type=Path)
    args = parser.parse_args()
    run = args.run or sorted((ROOT / 'Builds/FootballMotionQA').glob('Run-*'))[-1]
    fps = int((run / 'capture-rate.txt').read_text())
    stages = defaultdict(list)
    with (run / 'joints.csv').open() as stream:
        for row in csv.DictReader(stream):
            if not row['stage'].startswith('take-'):
                stages[row['stage']].append(row)
    result = {}
    for stage, rows in stages.items():
        peak = max(rows, key=lambda r: float(r['angularSpeed']))
        frames = defaultdict(dict)
        for row in rows:
            frames[int(row['frame'])][row['bone'].split(':')[-1]] = row
        slip = []
        previous = None
        for _, frame in sorted(frames.items()):
            if previous:
                for side, flag in [('Left', 'leftPlant'), ('Right', 'rightPlant')]:
                    a, b = previous[side + 'Foot'], frame[side + 'Foot']
                    if a[flag] == b[flag] == 'True':
                        slip.append(sum((float(a[k]) - float(b[k])) ** 2 for k in 'xz') ** .5)
            previous = frame
        result[stage] = dict(peak_degrees_per_second=float(peak['angularSpeed']),
                             peak_bone=peak['bone'], peak_frame=int(peak['frame']),
                             support_samples=len(slip),
                             maximum_horizontal_support_displacement_m=max(slip, default=0),
                             maximum_horizontal_support_speed_mps=max(slip, default=0) * fps)
    output = dict(capture_rate=fps, stages=result,
                  scope='Rendered skeleton continuity and support; not an artistic quality score.')
    (run / 'continuity.json').write_text(json.dumps(output, indent=2) + '\n')
    for stage, data in result.items():
        print(f"{stage:20s} {data['peak_degrees_per_second']:7.1f} deg/s "
              f"{data['maximum_horizontal_support_displacement_m'] * 1000:6.2f} mm/plant sample "
              f"{data['peak_bone']}")


if __name__ == '__main__':
    main()
