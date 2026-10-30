"""Measure real Unity peers through a loopback UDP delay/loss proxy.

Run with Python 3. Output is ignored under Builds/ResponsivenessQA; paths are
resolved from this script, independently of the caller's working directory.
The baseline switch restores the old buffering/rates in the same source build.
"""
import argparse
import csv
import heapq
import json
import random
import selectors
import socket
import statistics
import subprocess
import threading
import time
from datetime import datetime
from pathlib import Path


def main():
    root = Path(__file__).resolve().parents[2]
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument('--player', type=Path, default=root / 'Builds/ResponsivenessQA/Player/WhatTheFish.exe')
    parser.add_argument('--content-cache', type=Path, default=root / 'Builds/SharedContentCache', help='Shared hash-keyed local content cache; relative overrides resolve from the repository')
    parser.add_argument('--players', type=int, choices=range(2, 21), default=3)
    parser.add_argument('--delay-ms', type=float, default=60)
    parser.add_argument('--jitter-ms', type=float, default=8)
    parser.add_argument('--loss', type=float, default=0)
    parser.add_argument('--loss-after-join', action='store_true', help='Start loss after all peers have spawned; delay/jitter apply throughout')
    parser.add_argument('--baseline', action='store_true')
    parser.add_argument('--port', type=int, default=8030)
    args = parser.parse_args()
    args.player = (root / args.player).resolve()
    args.content_cache = (root / args.content_cache).resolve()
    if not (0 <= args.loss <= .15 and 0 <= args.jitter_ms <= args.delay_ms <= 500):
        parser.error('Use nonnegative delay/jitter <=500 ms and loss <=0.15')
    run = root / 'Builds/ResponsivenessQA' / (datetime.now().strftime('%Y%m%d-%H%M%S') + ('-baseline' if args.baseline else '-fixed') + f'-{args.players}peers')
    run.mkdir(parents=True)
    selector, queue, counters = selectors.DefaultSelector(), [], {'received': 0, 'forwarded': 0, 'dropped': 0, 'bytes': 0}
    rng, stop, links = random.Random(5169), threading.Event(), []
    loss_active = threading.Event()
    if not args.loss_after_join:
        loss_active.set()
    for i in range(1, args.players):
        downstream, upstream = socket.socket(socket.AF_INET, socket.SOCK_DGRAM), socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        downstream.bind(('127.0.0.1', args.port + i))
        upstream.bind(('127.0.0.1', 0))
        downstream.setblocking(False)
        upstream.setblocking(False)
        link = {'down': downstream, 'up': upstream, 'client': None}
        links.append(link)
        selector.register(downstream, selectors.EVENT_READ, (link, True))
        selector.register(upstream, selectors.EVENT_READ, (link, False))

    def proxy():
        serial = 0
        while not stop.is_set():
            for key, _ in selector.select(.001):
                link, to_host = key.data
                try:
                    data, sender = key.fileobj.recvfrom(65535)
                except (BlockingIOError, ConnectionResetError):
                    continue
                counters['received'] += 1
                if to_host:
                    link['client'] = sender
                if rng.random() < (args.loss if loss_active.is_set() else 0):
                    counters['dropped'] += 1
                    continue
                destination = ('127.0.0.1', args.port) if to_host else link['client']
                if destination is None:
                    continue
                serial += 1
                due = time.perf_counter() + (args.delay_ms + rng.uniform(-args.jitter_ms, args.jitter_ms)) / 1000
                heapq.heappush(queue, (due, serial, link['up'] if to_host else link['down'], destination, data))
            now = time.perf_counter()
            while queue and queue[0][0] <= now:
                _, _, sock, destination, data = heapq.heappop(queue)
                try:
                    sock.sendto(data, destination)
                    counters['forwarded'] += 1
                    counters['bytes'] += len(data)
                except (BlockingIOError, ConnectionResetError):
                    counters['dropped'] += 1

    proxy_started = time.perf_counter()
    worker = threading.Thread(target=proxy, daemon=True)
    worker.start()
    processes = []
    try:
        for i in range(args.players):
            folder = run / str(i)
            folder.mkdir()
            cmd = [str(args.player.resolve()), '-batchmode', '-force-d3d11', '-screen-fullscreen', '0', '-screen-width', '640', '-screen-height', '360', '-job-worker-count', '1', '-probe', '-sport', 'Football', '-responsivenessReview', str(folder), '-localHost' if i == 0 else '-localClient', '-port', str(args.port + i), '-expected', str(args.players), '-startAt', '15', '-returnAt', '200', '-exitAfter', '210', '-report', str(folder / 'probe.txt'), '-logFile', str(folder / 'player.log')]
            cmd += ['-contentCache', str(args.content_cache)]
            if args.baseline:
                cmd += ['-latencyBaseline']
            if args.players > 3:
                cmd += ['-capacityLatency', '-roomCapacityProbe', str(folder / 'transport')]
            startup = subprocess.STARTUPINFO()
            startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
            startup.wShowWindow = subprocess.SW_HIDE
            processes.append(subprocess.Popen(cmd, cwd=root, startupinfo=startup))
            if i == 0:
                deadline = time.monotonic() + 90
                while time.monotonic() < deadline:
                    report = folder / 'probe.txt'
                    if report.exists() and 'count=1 connected=True' in report.read_text(errors='replace'):
                        break
                    if processes[0].poll() is not None:
                        raise RuntimeError('Host exited before listening')
                    time.sleep(.25)
                else:
                    raise RuntimeError('Host did not start listening')
        deadline = time.monotonic() + 180
        while time.monotonic() < deadline:
            if args.loss_after_join and not loss_active.is_set():
                report = run / '0/probe.txt'
                if report.exists() and f'count={args.players} connected=True' in report.read_text(errors='replace'):
                    loss_active.set()
                    (run / 'loss-enabled.txt').write_text('All peers spawned; UDP packet loss now active')
            if all((run / str(i) / 'done').exists() for i in range(args.players)):
                break
            if any(p.poll() is not None for p in processes):
                raise RuntimeError('A peer exited before completing')
            time.sleep(.25)
        else:
            raise RuntimeError('Peers did not complete within timeout')
        (run / 'finish').write_text('finish')
        rows, shots = [], []
        for i, process in enumerate(processes):
            if process.wait(timeout=25) != 0:
                raise RuntimeError(f'Peer {i} failed')
            result = (run / str(i) / 'results.txt').read_text()
            log = (run / str(i) / 'player.log').read_text(errors='replace')
            if 'FAIL ' in result or 'RESPONSIVENESS_COMPLETE success=True' not in result:
                raise RuntimeError(f'Peer {i} failed assertions')
            for error in ('Exception:', 'Assertion failed', 'Fake or uninitialized surface', 'Not inside a Renderpass'):
                if error in log:
                    raise RuntimeError(f'Peer {i} runtime diagnostic: {error}')
            with (run / str(i) / 'latency.csv').open() as stream:
                rows.extend(csv.DictReader(stream))
            with (run / str(i) / 'shots.csv').open() as stream:
                shots.extend(csv.DictReader(stream))
        summary = {'peers': args.players, 'baseline': args.baseline, 'delay_ms_each_way': args.delay_ms, 'jitter_ms_each_way': args.jitter_ms, 'drop_probability': args.loss, 'loss_after_join': args.loss_after_join, 'run_seconds': time.perf_counter() - proxy_started, 'datagrams': counters}
        for side, values in [('host', [r for r in rows if r['client'] == '0']), ('guests', [r for r in rows if r['client'] != '0'])]:
            summary[side] = {metric: {'median': statistics.median(float(r[metric]) for r in values), 'max': max(float(r[metric]) for r in values)} for metric in ('first_3cm_ms', 'first_25cm_ms', 'stop_settle_ms')}
        summary['shots'] = shots
        (run / 'summary.json').write_text(json.dumps(summary, indent=2))
        print(json.dumps(summary, indent=2), flush=True)
        print(f'RESPONSIVENESS_PASS {run}', flush=True)
    finally:
        stop.set()
        worker.join(3)
        for process in processes:
            if process.poll() is None:
                process.terminate()
                process.wait(timeout=15)
        selector.close()
        for link in links:
            link['down'].close()
            link['up'].close()
        (run / 'proxy.json').write_text(json.dumps(counters, indent=2))


if __name__ == '__main__':
    main()
