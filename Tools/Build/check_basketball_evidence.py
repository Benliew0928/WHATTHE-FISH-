"""Compare actual movement and final server/client transforms from room probes."""
import json, math, re, sys
from pathlib import Path
folder=Path(sys.argv[1])
def states(name):
    records=[]
    for line in (folder/(name+'.txt')).read_text().splitlines():
        if 'count=10 connected=True' not in line:continue
        positions={int(m[0]):tuple(float(v) for v in m[1:]) for m in re.findall(r'(\d+):True:\{[^}]+\}:\((-?[\d.]+), (-?[\d.]+), (-?[\d.]+)\)',line)}
        if len(positions)==10:records.append((line,positions))
    return records
host=states('Ahost');first=host[0][1]
def settled(records):
    # Compare the frozen world after returning, not asynchronously sampled moving frames.
    seen_play=False;returned=[]
    for line,positions in records:
        if 'exploring=True' in line:seen_play=True
        elif seen_play and set(positions)==set(first):returned.append(positions)
    assert returned,'No complete original roster after returning to the waiting room'
    return returned[-1]
last=settled(host)
distances={i:math.dist(first[i],last[i]) for i in first}
assert all(d>1 for d in distances.values()),distances
errors={}
for n in range(1,10):
    remote=settled(states('Aguest'+str(n)))
    errors[n]=max(math.dist(last[i],remote[i]) for i in last)
assert max(errors.values())<=.30,errors
report={'result':'PASS','movement_metres_by_player':distances,'max_replication_error_metres_by_guest':errors}
(folder/'movement-results.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report))
