"""Build the local static website and synchronize bundled document/catalog keys.

No deployment or network access. Paths resolve from this script's repository.
Run --check after generation to validate the exact current outputs.
"""
import argparse
import html
import json
from pathlib import Path
import re
import shutil
import uuid

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Website'
RES = ROOT / 'Game/Assets/_Game/Resources'
LANGS = [('en', 'en', 'English'), ('zh', 'zh-Hans', '简体中文'), ('ms', 'ms', 'Bahasa Melayu')]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check', action='store_true')
parser.add_argument('--public-preview', action='store_true', help='Build the public website preview only; never write game resources or metadata')
parser.add_argument('--output', type=Path)
args = parser.parse_args()
if args.output is None:
    args.output = ROOT / ('Builds/WebsitePublish/Site' if args.public_preview else 'Builds/PublicInfoQA/Site')
elif not args.output.is_absolute():
    args.output = ROOT / args.output
data = json.loads((SOURCE / 'content.json').read_text(encoding='utf-8'))
catalog_path = RES / 'Localization/Catalog.json'
catalog = json.loads(catalog_path.read_text(encoding='utf-8'))
existing = {e['en']: e for e in catalog['entries']}
copy = {}
for key, values in data['copy'].items():
    assert len(values) == 3 and all(v.strip() for v in values), key
    assert all(re.findall(r'\{\d+\}', values[0]) == re.findall(r'\{\d+\}', v) for v in values), key
    entry = existing.get(values[0])
    if entry is None:
        entry = dict(zip(('en', 'zh', 'ms'), values))
        existing[entry['en']] = entry
        catalog['entries'].append(entry)
    copy[key] = [entry[k] for k in ('en', 'zh', 'ms')]

def encoded(value):
    return json.dumps(value, ensure_ascii=False, indent=2) + '\n'

expected = {}
bundle = {'documents': []}
for doc in data['documents']:
    bundle['documents'].append({
        'id': doc['id'], 'title': copy[doc['title']][0], 'intro': copy[doc['intro']][0],
        'sections': [{'heading': copy[h][0], 'body': copy[b][0]} for h, b in doc['sections']]
    })
if not args.public_preview:
    expected[RES / 'AppGallery/Documents.json'] = encoded(bundle)
    expected[catalog_path] = encoded(catalog)
else:
    # This shared website catalog changes public-hosting copy without changing
    # the already validated offline game drafts or the release's localization.
    for key, values in json.loads((SOURCE / 'public-preview.json').read_text(encoding='utf-8')).items():
        assert len(values) == 3 and all(v.strip() for v in values), key
        assert all(re.findall(r'\{\d+\}', values[0]) == re.findall(r'\{\d+\}', v) for v in values), key
        copy[key] = values
    expected[args.output/'_headers'] = "/*\n  X-Content-Type-Options: nosniff\n  Referrer-Policy: no-referrer\n  X-Robots-Tag: noindex, nofollow\n  Permissions-Policy: camera=(), microphone=(), geolocation=()\n  Content-Security-Policy: default-src 'none'; style-src 'self'; img-src 'self'; font-src 'self'; base-uri 'none'; form-action 'none'; frame-ancestors 'none'\n"
    expected[args.output/'robots.txt'] = 'User-agent: *\nDisallow: /\n'
escape = html.escape

def page(locale, lang, lang_name, index, doc=None):
    stem = doc['id'] if doc else 'index'
    prefix = '' if locale == 'en' else '../'
    tr = lambda key: escape(copy[key][index])
    link = lambda target: target + '.html'
    title = copy[doc['title']][index] if doc else 'WHATTHE FISH?'
    nav_items = []
    for target, key in [('index','home'),('terms','terms'),('privacy','privacy'),('support','support')]:
        current = ' aria-current="page"' if stem == target else ''
        nav_items.append(f'<a href="{link(target)}"{current}>{tr(key)}</a>')
    nav = ''.join(nav_items)
    language_items = []
    for code, tag, name in LANGS:
        current = ' aria-current="true"' if code == locale else ''
        directory = '' if code == 'en' else code + '/'
        language_items.append(f'<a href="{prefix}{directory}{stem}.html" lang="{tag}" hreflang="{tag}"{current}>{name}</a>')
    languages = ''.join(language_items)
    banner = f'<div class="draft-banner"><strong>{tr("draft")}</strong><span>{tr("updated")} {data["draftDate"]}</span></div>'
    header = f'<a class="skip" href="#main">{tr("skip")}</a>{banner}<header class="site-header"><a class="brand" href="index.html"><img src="{prefix}assets/logo.png" width="140" height="100" alt="WHATTHE FISH?"></a><nav aria-label="{tr("nav")}">{nav}</nav><div class="languages" aria-label="{tr("language")}">{languages}</div></header>'
    if doc:
        toc = ''.join(f'<a href="#section-{i}">{tr(h)}</a>' for i, (h, _) in enumerate(doc['sections'], 1))
        sections = ''.join(f'<section id="section-{i}" class="document-section"><h2>{tr(h)}</h2><p>{tr(b)}</p></section>' for i, (h, b) in enumerate(doc['sections'], 1))
        if args.public_preview and doc['id'] == 'privacy':
            sections += f'<p><a href="https://www.cloudflare.com/privacypolicy/">{tr("hostingPolicy")}</a></p>'
        contact = f'<a class="button" href="mailto:{data["email"]}">{tr("contactAction")} <span aria-hidden="true">↗</span></a>' if doc['id'] == 'support' else ''
        main = f'<main id="main" class="document-layout"><aside class="contents"><p class="eyebrow">{tr("contents")}</p>{toc}</aside><article class="document"><p class="eyebrow">WHATTHE FISH?</p><h1>{escape(title)}</h1><p class="lead">{tr(doc["intro"])}</p><div class="notice"><strong>{tr("draft")}</strong><p>{tr("draftNote")}</p></div>{contact}{sections}<a class="back-top" href="#main">{tr("backTop")} ↑</a></article></main>'
    else:
        sports = ''.join(f'<article class="sport-card {s}"><div class="sport-image" role="img" aria-label="{tr(s)}"></div><div class="sport-copy"><span class="sport-number">0{i}</span><h3>{tr(s)}</h3><p>{tr(s+"Body")}</p></div></article>' for i,s in enumerate(['football','basketball','golf','fishing'],1))
        main = f'''<main id="main"><section class="hero"><div class="hero-copy"><p class="eyebrow">{tr('eyebrow')}</p><h1>{tr('hero').replace(chr(10),'<br>')}</h1><p class="lead">{tr('heroBody')}</p><div class="actions"><a class="button" href="#sports">{tr('explore')} <span aria-hidden="true">↗</span></a><a class="text-link" href="support.html">{tr('getHelp')} →</a></div><p class="availability"><span aria-hidden="true"></span>{tr('availability')}</p></div><div class="hero-art"><img src="{prefix}assets/islands.png" width="1254" height="1254" alt="{tr('islandAlt')}"><span class="art-note">WHATTHE FISH?</span></div></section>
<section id="sports" class="sports"><div class="section-heading"><p class="eyebrow">01 / WHATTHE FISH?</p><h2>{tr('sportHeading')}</h2><p>{tr('sportIntro')}</p></div><div class="sports-grid">{sports}</div></section>
<section class="together"><div><p class="eyebrow">02 / WHATTHE FISH?</p><h2>{tr('together')}</h2><p>{tr('togetherBody')}</p></div><ul class="features"><li>{tr('free')}</li><li>{tr('noAds')}</li><li>{tr('noIap')}</li></ul></section>
<section class="help"><p class="eyebrow">03 / WHATTHE FISH?</p><h2>{tr('helpTitle')}</h2><p>{tr('helpBody')}</p><div class="help-links"><a href="terms.html">{tr('terms')} <span aria-hidden="true">↗</span></a><a href="privacy.html">{tr('privacy')} <span aria-hidden="true">↗</span></a><a href="support.html">{tr('support')} <span aria-hidden="true">↗</span></a></div></section></main>'''
    credits = f'<section class="document-section model-credits"><h2>{tr("modelCreditsH")}</h2><p>{tr("modelCreditsB")}</p><p><a href="https://www.meshy.ai/">Meshy</a> · <a href="https://creativecommons.org/licenses/by/4.0/">CC BY 4.0</a></p></section>' if args.public_preview else ''
    footer = f'<footer><div><strong>WHATTHE FISH?</strong><p>{tr("publisherLabel")} {data["publisher"]}</p></div><div><span>{tr("contactLabel")}</span><a href="mailto:{data["email"]}">{data["email"]}</a></div><p class="footer-draft">{tr("draft")}</p>{credits}</footer>'
    return f'<!doctype html>\n<html lang="{lang}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="robots" content="noindex,nofollow"><meta name="description" content="{tr(doc["intro"] if doc else "heroBody")}"><meta name="color-scheme" content="light"><title>{escape(title)} — WHATTHE FISH?</title><link rel="stylesheet" href="{prefix}assets/site.css"></head><body id="top">{header}{main}{footer}</body></html>\n'

for index,(locale,lang,name) in enumerate(LANGS):
    folder = args.output if locale == 'en' else args.output / locale
    expected[folder/'index.html'] = page(locale,lang,name,index)
    for doc in data['documents']:
        expected[folder/(doc['id']+'.html')] = page(locale,lang,name,index,doc)

assets = {
    args.output/'assets/site.css': SOURCE/'site.css',
    args.output/'assets/logo.png': RES/'Menu/WhatTheFishLogo.png',
    args.output/'assets/islands.png': RES/'Menu/CoveIslands.png',
    args.output/'assets/sports.png': RES/'SportyMenu/SportCards.png',
    args.output/'assets/display.ttf': RES/'SportyMenu/BarlowDisplay.ttf',
    args.output/'assets/OFL.txt': RES/'SportyMenu/Barlow-OFL.txt',
}

for path,value in expected.items():
    if args.check:
        assert path.exists() and path.read_text(encoding='utf-8') == value, f'Stale output: {path.relative_to(ROOT) if path.is_relative_to(ROOT) else path.name}'
    elif not path.exists() or path.read_text(encoding='utf-8') != value:
        path.parent.mkdir(parents=True,exist_ok=True)
        path.write_text(value,encoding='utf-8',newline='\n')
for target,source in assets.items():
    if args.check:
        assert target.exists() and target.read_bytes() == source.read_bytes(), target.name
    else:
        target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(source,target)

# New Unity sources/assets keep stable metadata; never replace an existing GUID.
unity_paths = [RES/'AppGallery',RES/'AppGallery/Documents.json',RES/'AppGallery/Links.json',
              ROOT/'Game/Assets/_Game/Shared/UI/PublicInfoDocuments.cs',
              ROOT/'Game/Assets/_Game/Editor/PublicInfoBuild.cs',
              ROOT/'Game/Assets/_Game/Shared/UI/StartupWelcome.PublicInfo.cs']
for path in ([] if args.public_preview else unity_paths):
    meta = Path(str(path)+'.meta')
    if not meta.exists():
        assert not args.check, f'Missing metadata: {path.name}'
        guid = uuid.uuid5(uuid.NAMESPACE_URL,'whatthefish/'+path.relative_to(ROOT).as_posix()).hex
        folder_line = 'folderAsset: yes\n' if path.is_dir() else ''
        meta.write_text(f'fileFormatVersion: 2\nguid: {guid}\n{folder_line}',encoding='utf-8',newline='\n')

for path in args.output.rglob('*.html'):
    for target in re.findall(r'(?:href|src)="([^"#]+)',path.read_text(encoding='utf-8')):
        if ':' not in target:
            assert (path.parent/target.split('#')[0]).exists(), (path.name,target)
print(f'PUBLIC_INFO_PASS: 12 pages, {len(copy)} three-language strings; '+('public-preview package only; game files unchanged' if args.public_preview else '3 bundled drafts; no deployment'))
