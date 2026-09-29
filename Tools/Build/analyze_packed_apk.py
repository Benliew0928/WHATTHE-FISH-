"""Estimate each asset's ZIP contribution from Unity's detailed packing offsets.

Per-asset DEFLATE is an estimate (ZIP uses 1 MiB chunks); actual APK size is exact.
Usage: python analyze_packed_apk.py APK packed-layout.csv output.csv
"""
import csv, sys, zipfile, zlib
from collections import defaultdict
from pathlib import Path

apk, layout, output = map(Path, sys.argv[1:])
archive = zipfile.ZipFile(apk)
groups = defaultdict(list)
for row in csv.DictReader(layout.open()):groups[row['file']].append(row)
totals = defaultdict(lambda: [0, 0])
for filename, rows in groups.items():
    base='assets/bin/Data/'+filename
    splits=[i for i in archive.namelist() if i.startswith(base+'.split')]
    if splits:data=b''.join(archive.read(i) for i in sorted(splits,key=lambda p:int(p.rsplit('split',1)[1])))
    elif base in archive.namelist():data=archive.read(base)
    else:continue
    for row in rows:
        offset,size=int(row['offset']),int(row['bytes'])
        content=data[offset:offset+size]
        totals[row['asset']][0]+=size
        totals[row['asset']][1]+=len(zlib.compress(content,6))
with output.open('w',newline='') as f:
    writer=csv.writer(f);writer.writerow(['asset','serialized_bytes','estimated_zip_bytes'])
    writer.writerows((name,*sizes) for name,sizes in sorted(totals.items(),key=lambda i:-i[1][1]))
print('Exact APK bytes:',apk.stat().st_size)
print('\n'.join(f'{sizes[1]/1e6:.3f} MB estimate: {name}' for name,sizes in sorted(totals.items(),key=lambda i:-i[1][1])[:22]))
